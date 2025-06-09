using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PointCreator : DiagramEditor
{
    public bool placing;
    public Point point;

    public override string NotificationText => "Select a position to place a point";

    public override void OnPointerClick(PointerEventData eventData)
    {
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        placing = true;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        point = diagram.CreatePoint(mousePosition);
		point.col.enabled = false; // Disable the collider until placement is confirmed
	}

    public override void DeactivateEdit()
    {
        point.Delete();
        point = null;
        placing = false;
    }

    public override void Tick()
    {
        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Attachable attachable = diagram.GetProminentAttachable(ref placingPosition);
        if (!attachable) diagram.LockPositionToGrid(ref placingPosition);
        point.gameObject.transform.position = placingPosition;

        if (!diagram.clickedOnDiagram) return;
        PlacePoint(attachable);
    }

    /// <summary>
    /// Places the point at the specified attachable, if provided, otherwise simply registers the point in the diagram
    /// </summary>
    /// <param name="attachable"></param>
    public void PlacePoint(Attachable attachable)
	{
		if (attachable) attachable.AttachPoint(point);

		point.col.enabled = true;
		diagram.elements.Add(point);
		placing = false;
		point = null;

		// Keep the placing persistent
		ActivateEdit();
	}
}
