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

        if (Input.GetMouseButtonDown(0))
        {
            selectedPoint = diagram.GetPointAtPosition(placingPosition);
            selectedAttachable = diagram.GetProminentAttachable(ref placingPosition);
        }
        //Movement Logic
        if (selectedPoint != null)
        {
            // Determine if movement is along a line or free movement
            if (selectedPoint.semiAttatchedLine != null)
            {
                placingPosition = selectedPoint.semiAttatchedLine.GetClosestPosition(placingPosition);
                selectedPoint.percentage = selectedPoint.semiAttatchedLine.CalculatePercentage(placingPosition);
            }
            else
            {
                // Allows points to lock to other points
                // Using 'local functions' to filter points (not a lambda function)
                bool exclusion(Point p) => p == selectedPoint || p.semiAttatchedLine != null;
                Point newPoint = diagram.GetPointAtPosition(placingPosition, exclusion);
                if (newPoint != null)
                {
                    placingPosition = newPoint.position;
                }
            }
            selectedPoint.UpdatePoint(placingPosition);
        }
        else if (selectedAttachable != null)
        {
            if (selectedAttachable is Circle circle)
            {
                circle.SetRadius(placingPosition);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectedPoint = null;
            selectedAttachable = null;
            moving = false;
            ActivateEdit();
        }
    }

    // Snapping of moved point to line or another point (including all data correct)
}
