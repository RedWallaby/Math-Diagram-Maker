using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class LineCreator : MonoBehaviour, IPointerClickHandler
{
    public Diagram diagram;
    public Line line;
    public GameObject linePrefab;
    public PlacingStage placing;
    public Point hoveringPoint;
    public Line hoveringLine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Allows the line to lock to the nearest point or line (gives points priority)
    public void GetProminentFeature(ref Vector3 position)
    {
        hoveringPoint = null;
        hoveringLine = null;
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Point point))
            {
                position = point.position;
                hoveringPoint = point;
                return;
            }
        }
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Line line))
            {
                position = line.CalculateClosestPosition(position);
                hoveringLine = line;
                return;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (placing != PlacingStage.None) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
        mousePosition.z = 0; // Set z to 0 for 2D
        CreateLine(mousePosition);
        placing = PlacingStage.Point;
    }

    private void CreateLine(Vector3 position)
    {
        GameObject lineObj = Instantiate(linePrefab, position, Quaternion.identity, diagram.transform);
        line = lineObj.GetComponent<Line>();
    }

    public void Update()
    {
        if (placing == PlacingStage.Point)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Set z to 0 for 2D

            GetProminentFeature(ref mousePosition);
            line.gameObject.transform.position = mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                line.line.SetPosition(0, mousePosition);
                placing = PlacingStage.Line;

                // Create a new point if no point is currently selected
                if (!hoveringPoint)
                {
                    hoveringPoint = diagram.CreateInstantPoint(mousePosition);
                }

                // Check if the line is snapping to another line
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

            GetProminentFeature(ref mousePosition);
            line.line.SetPosition(1, mousePosition);

            if (Input.GetMouseButtonDown(0))
            {
                placing = PlacingStage.None;
                if (!hoveringPoint)
                {
					hoveringPoint = diagram.CreateInstantPoint(mousePosition);
				}

				// Check if the line is snapping to another line
				if (hoveringLine)
				{
					hoveringPoint.semiAttatchedLine = hoveringLine;
					hoveringLine.attatchedPoints.Add(hoveringPoint);
				}

				// Attach the point to the line
				line.points[0] = hoveringPoint;
				hoveringPoint.attatchedLines.Add(line);

				line.AddComponent<ClickableLine>();
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
