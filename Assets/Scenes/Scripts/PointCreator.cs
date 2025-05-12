using UnityEngine;
using UnityEngine.EventSystems;

public class PointCreator : DiagramEditor
{
    public Point point;
    public bool placing;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (placing) return; // Prevent double placing
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        placing = true;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Set z to 0 for 2D
        GameObject pointObj = Instantiate(diagram.pointPrefab, mousePosition, Quaternion.identity, diagram.transform);
        point = pointObj.GetComponent<Point>();
        point.col.enabled = false; // Disable the collider until placement is confirmed
    }

    public override void DeactivateEdit()
    {
        Destroy(point.gameObject);
        point = null;
        placing = false;
    }

    public void Update()
    {
        if (!placing) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Set z to 0 for 2D
        point.gameObject.transform.position = mousePosition;

        // Snap to nearest point on any detected line
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        Line line = null;
        if (hit.collider != null && hit.collider.TryGetComponent(out line))
        {
            point.gameObject.transform.position = line.CalculateClosestPosition(mousePosition);
        }
            
        if (Input.GetMouseButtonDown(0))
        {
            // If snapping to line, attach the point to the line
            if (line != null)
            {
                line.attatchedPoints.Add(point);
                point.semiAttatchedLine = line;
                point.percentage = line.CalculatePercentage(point.position);
            }

            // Add the point to the diagram
            diagram.points.Add(point);
            placing = false;
            point.col.enabled = true; // Enable the collider
            point = null;

            // Keep the placing persistent
            ActivateEdit();
        }
       
    }
}
