using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveSelector : DiagramEditor
{
    public bool moving;
    public Point selectedPoint;
    public Point hoveringPoint;
    public Line hoveringLine;

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
        Vector3 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        placingPosition.z = 0; // Set z to 0 for 2D

        if (Input.GetMouseButtonDown(0))
        {
            selectedPoint = diagram.GetPointAtPosition(placingPosition);
        }

        //Movement Logic
        if (selectedPoint != null)
        {
            if (selectedPoint.semiAttatchedLine != null)
            {
                placingPosition = selectedPoint.semiAttatchedLine.CalculateClosestPosition(placingPosition);
                selectedPoint.percentage = selectedPoint.semiAttatchedLine.CalculatePercentage(placingPosition);
            }
            else
            {
                // Using 'local functions' to filter points (not a lambda function)
                bool exclusion(Point p) => p == selectedPoint || p.semiAttatchedLine != null;
                Point newPoint = diagram.GetPointAtPosition(placingPosition, exclusion);
                if (newPoint != null && newPoint.semiAttatchedLine == null)
                {
                    placingPosition = newPoint.position;
                }
            }
            foreach (Line line in selectedPoint.attatchedLines)
            {
                line.UpdatePointPosition(selectedPoint, placingPosition);
            }
            selectedPoint.gameObject.transform.position = placingPosition;

            if (Input.GetMouseButtonUp(0))
            {
                

                selectedPoint = null;
                moving = false;
                ActivateEdit();
            }
        }
    }

    // Snapping of moved point to line or another point (including all data correct)
}
