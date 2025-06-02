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
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        point = diagram.CreatePoint(mousePosition);
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
        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Attachable attachable = diagram.GetProminentAttachable(ref placingPosition);
        if (!attachable) diagram.LockPositionToGrid(ref placingPosition);
        point.gameObject.transform.position = placingPosition;

        if (!diagram.clickedOnDiagram) return;
        PlacePoint(attachable);
    }

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
