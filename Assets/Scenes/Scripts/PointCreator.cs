using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

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

        // Settings
        RectTransform rt = pointObj.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, diagram.pointRadius * 100);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, diagram.pointRadius * 100);
        point.GetComponent<CircleCollider2D>().radius = diagram.pointRadius * 50;
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
        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Attachable attachable = diagram.GetProminentAttachable(ref placingPosition);

        if (!attachable) diagram.LockPositionToGrid(ref placingPosition);

        point.gameObject.transform.position = placingPosition;

        if (!diagram.clickedOnDiagram) return;

        if (attachable) attachable.AttachPoint(point);

        // Add the point to the diagram
        diagram.elements.Add(point);
        placing = false;
        point.col.enabled = true; // Enable the collider
        point = null;

        // Keep the placing persistent
        ActivateEdit();
    }
}
