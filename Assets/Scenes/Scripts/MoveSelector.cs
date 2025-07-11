using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveSelector : DiagramEditor
{
    public bool moving;
    public Point selectedPoint;
    public Attachable selectedAttachable;

    public override string NotificationText => "Select and drag a point or circle";

    public override void OnPointerClick(PointerEventData eventData)
    {
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        moving = true;
    }

    public override void DeactivateEdit()
    {
        moving = false;
    }

    public override void Tick()
    {
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

    /// <summary>
    /// Adjusts the position of the given point, snapping it to the closest line or point if necessary
    /// </summary>
    /// <remarks>
    /// If the point is on a semi-attached line, it will snap to the closest position on that line.
    /// </remarks>
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

    /// <summary>
    /// Modifies the given <c>Circle</c> object's radius based on the new position
    /// </summary>
    /// <param name="attachable"></param>
    /// <param name="newPosition"></param>
    public void AdjustAttachable(Attachable attachable, Vector2 newPosition) {

		if (attachable is Circle circle)
		{
			float radius = Vector2.Distance(circle.centre.position, newPosition);

			if (Input.GetKey(KeyCode.LeftShift))
			{
				circle.SetRadius(Mathf.Ceil(radius / diagram.gridSize) * diagram.gridSize);
			}
			else
			{
				circle.SetRadius(radius);
			}
		}
	}

    public override Element GetSelectedElement()
    {
        return null;
    }
}
