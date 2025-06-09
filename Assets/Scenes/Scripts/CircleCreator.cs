using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static LineCreator;

public class CircleCreator : DiagramEditor
{
    public PlacingStage placing;
    public Circle circle;

    public override string NotificationText => "Select a position/point, then select a position to set the radius";

    public override void OnPointerClick(PointerEventData eventData)
    {
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        placing = PlacingStage.Point;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        circle = diagram.CreateCircle(mousePosition);
        circle.col.enabled = false; // Disable the collider until placement is confirmed
    }

    public override void DeactivateEdit()
    {
        circle.Delete();
        circle = null;
        placing = PlacingStage.None;
    }

    public override void Tick()
    {
        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (placing == PlacingStage.Point)
        {
            SetupPoint(placingPosition);
        }
        else if (placing == PlacingStage.Line)
        {
            AdjustRadius(placingPosition);
        }
    }

    /// <summary>
    /// Initialises the circle placement by setting up a point at the clicked position
    /// </summary>
    public void SetupPoint(Vector2 position)
    {
		diagram.GetProminentFeature(ref position, out Point hoveringPoint, out Attachable attachable);

		if (!diagram.clickedOnDiagram) return;

		if (!hoveringPoint)
		{
			hoveringPoint = diagram.CreatePoint(position);
            diagram.elements.Add(hoveringPoint);
			if (attachable)
			{
				attachable.AttachPoint(hoveringPoint);
			}
		}

        hoveringPoint.attachedElements.Add(circle);
		circle.centre = hoveringPoint;
		circle.gameObject.transform.position = position;
		circle.DrawCircle();
		placing = PlacingStage.Line;
	}

    /// <summary>
    /// Adjusts the circle's radius based on the distance from its centre <c>Point</c> to the given position
    /// </summary>
    public void AdjustRadius(Vector2 position)
    {
		float radius = Vector2.Distance(circle.centre.position, position);

		if (Input.GetKey(KeyCode.LeftShift))
		{
			circle.SetRadius(Mathf.Ceil(radius));
		}
		else
		{
			circle.SetRadius(radius);
		}

		if (!diagram.clickedOnDiagram) return;

		diagram.elements.Add(circle);

		circle.col.enabled = true;
		circle = null;
		ActivateEdit();
	}
}
