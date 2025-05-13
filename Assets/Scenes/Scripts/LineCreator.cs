using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineCreator : DiagramEditor
{
    public Line line;
    public PlacingStage placing;

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
        GameObject lineObj = Instantiate(diagram.linePrefab, mousePosition, Quaternion.identity, diagram.transform);
        line = lineObj.GetComponent<Line>();
        line.col.enabled = false; // Disable the collider until placement is confirmed

        // Settings
        line.line.startWidth = diagram.lineWidth;
        line.colliderWidthMultiplier = diagram.colliderWidthMultiplier;
    }

    public override void DeactivateEdit()
    {
        Destroy(line.gameObject);
        line = null;
        placing = PlacingStage.None;
    }

    public void Update()
    {
        if (placing == PlacingStage.None) return;
        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        diagram.GetProminentFeature(ref placingPosition, out Point hoveringPoint, out Attachable attachable);

        if (placing == PlacingStage.Line)
        {
            line.line.SetPosition(1, placingPosition);
        }

        if (!Input.GetMouseButtonDown(0)) return;

        if (placing == PlacingStage.Point)
        {
            line.line.SetPosition(0, placingPosition);
        }

        // Create a new point if no point is currently selected
        if (!hoveringPoint)
        {
            hoveringPoint = diagram.CreateInstantPoint(placingPosition);
            // Account for if the line is snapping to another line or circle
            if (attachable)
            {
                attachable.AttachPoint(hoveringPoint);
            }
        }

        // Attach the point to the line
        line.points[placing == PlacingStage.Point ? 0 : 1] = hoveringPoint;
		hoveringPoint.attatchedLines.Add(line);

        if (placing == PlacingStage.Line)
        {
            line.col.enabled = true;
            line = null;

            // Keep the editing persistent
            ActivateEdit();
            return;
        }
        placing = PlacingStage.Line;
    }

    public enum PlacingStage
    {
        None,
        Point,
        Line
    }
}
