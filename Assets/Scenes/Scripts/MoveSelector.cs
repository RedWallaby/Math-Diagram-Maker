using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveSelector : DiagramEditor
{
    public bool moving;
    public Point selectedPoint;
    public Attachable selectedAttachable;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (moving) return;
        ActivateEdit();
    }

    public override void ActivateEdit()
    {
        moving = true;
        diagram.SetEditor(this);
    }

    public override void DeactivateEdit()
    {
        moving = false;
    }

    public void Update()
    {
        if (!moving) return;
        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (diagram.clickedOnDiagram)
        {
            selectedPoint = diagram.GetPointAtPosition(placingPosition);
            selectedAttachable = diagram.GetProminentAttachable(ref placingPosition);
        }

        if (selectedPoint != null)
        {
            AdjustPoint(selectedPoint, placingPosition);
        }
        else if (selectedAttachable != null)
        {
            AdjustAttachable(selectedAttachable, placingPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedPoint = null;
            selectedAttachable = null;
            moving = false;
            ActivateEdit();
        }
    }

    public void AdjustPoint(Point point, Vector2 newPosition)
	{
		// Check if the point is attached to a line
		if (point.semiAttachedLine != null)
		{
			newPosition = point.semiAttachedLine.GetClosestPosition(newPosition);
			point.percentage = point.semiAttachedLine.CalculatePercentage(newPosition);
		}
		else
		{
			// Snap to other points if available
			Point closestPoint = diagram.GetPointAtPosition(newPosition, p => p == point || p.semiAttachedLine != null);
			if (closestPoint != null)
			{
				newPosition = closestPoint.position;
			}
			diagram.LockPositionToGrid(ref newPosition);
		}
		point.UpdatePoint(newPosition);
	}

    public void AdjustAttachable(Attachable attachable, Vector2 newPosition) {

		if (attachable is Circle circle)
		{
			float radius = Vector2.Distance(circle.centre.position, newPosition);

			if (Input.GetKey(KeyCode.LeftShift))
			{
				circle.SetRadius(Mathf.Ceil(radius));
			}
			else
			{
				circle.SetRadius(radius);
			}
		}
	}   

    // Snapping of moved point to line or another point (including all data correct)
}
