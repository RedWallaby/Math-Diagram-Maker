using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineCreator : DiagramEditor
{
    public Line line;
    public PlacingStage placing;
    public Point hoveringPoint;
    public Line hoveringLine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (placing != PlacingStage.None) return;
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        placing = PlacingStage.Point;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Set z to 0 for 2D
        GameObject lineObj = Instantiate(diagram.linePrefab, mousePosition, Quaternion.identity, diagram.transform);
        line = lineObj.GetComponent<Line>();
        line.col.enabled = false; // Disable the collider until placement is confirmed
    }

    public override void DeactivateEdit()
    {
        Destroy(line.gameObject);
        line = null;
        placing = PlacingStage.None;
    }

    public void Update()
    {
        if (placing == PlacingStage.Point)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 for 2D

            diagram.GetProminentFeature(ref mousePosition, out hoveringPoint, out hoveringLine);
            line.gameObject.transform.position = mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                line.line.SetPosition(0, mousePosition);
                placing = PlacingStage.Line;

                // Create a new point if no point is currently selected
                if (!hoveringPoint)
                {
                    hoveringPoint = diagram.CreateInstantPoint(mousePosition);
                    if (hoveringLine)
                        hoveringPoint.percentage = hoveringLine.CalculatePercentage(hoveringPoint.position);
                }

                // Account for if the line is snapping to another line
                if (hoveringLine)
                {
                    hoveringPoint.semiAttatchedLine = hoveringLine;
                    hoveringLine.attatchedPoints.Add(hoveringPoint);
                }

				// Attach the point to the line
				line.points[0] = hoveringPoint;
				hoveringPoint.attatchedLines.Add(line);
            }
        }
        else if (placing == PlacingStage.Line)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 for 2D

            diagram.GetProminentFeature(ref mousePosition, out hoveringPoint, out hoveringLine);
            line.line.SetPosition(1, mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                placing = PlacingStage.None;
                if (!hoveringPoint)
                {
					hoveringPoint = diagram.CreateInstantPoint(mousePosition);
                    if (hoveringLine)
                        hoveringPoint.percentage = hoveringLine.CalculatePercentage(hoveringPoint.position);
                }

                // Account for if the line is snapping to another line
				if (hoveringLine)
				{
					hoveringPoint.semiAttatchedLine = hoveringLine;
					hoveringLine.attatchedPoints.Add(hoveringPoint);
				}

				// Attach the point to the line
				line.points[1] = hoveringPoint;
				hoveringPoint.attatchedLines.Add(line);

                line.col.enabled = true;
                line = null;

                // Keep the editing persistent
                ActivateEdit();
            }
        }
    }

    public enum PlacingStage
    {
        None,
        Point,
        Line
    }
}
