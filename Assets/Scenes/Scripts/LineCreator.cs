using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class LineCreator : DiagramEditor
{
    public PlacingStage placing;
    public Line line;

    public override string NotificationText => "Select 2 positions/points to draw a line between";

    public override void OnPointerClick(PointerEventData eventData)
    {
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        placing = PlacingStage.Point;
        line = diagram.CreateLine();
        line.col.enabled = false; // Disable the collider until placement is confirmed
    }

    public override void DeactivateEdit()
    {
        line.Delete();
        line = null;
        placing = PlacingStage.None;
    }

    public override void Tick()
    {
        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        diagram.GetProminentFeature(ref placingPosition, out Point hoveringPoint, out Attachable attachable);
        if (placing == PlacingStage.Point)
        {
			if (!diagram.clickedOnDiagram) return;

			line.line.SetPosition(0, placingPosition);
			line.line.SetPosition(1, placingPosition); // Set both points to the same position initially (prevents line from being drawn to default position [0, 0])

			ProcessLine(placingPosition, hoveringPoint, attachable);
			placing = PlacingStage.Line;
		}
		else
        {
            if (line.points[0] == hoveringPoint)
            { 
                placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                hoveringPoint = null;
                attachable = null;
            }
            if (!attachable) diagram.LockPositionToGrid(ref placingPosition);
            line.line.SetPosition(1, placingPosition);

            if (!diagram.clickedOnDiagram) return;

            ProcessLine(placingPosition, hoveringPoint, attachable);

            diagram.elements.Add(line);
            line.SetPosition();
            line.col.enabled = true;
            line.DrawLineHitbox();
            ActivateEdit();
        }
    }

    /// <summary>
    /// Creaties a new <c>Point</c> or attaches the line to an existing one
    /// </summary>
    public void ProcessLine(Vector2 position, Point hoveringPoint, Attachable attachable)
    {
		// Create a new point if no point is currently selected
		if (!hoveringPoint)
		{
			hoveringPoint = diagram.CreatePoint(position);
            diagram.elements.Add(hoveringPoint);
			// Account for if the line is snapping to another line or circle
			if (attachable)
			{
				attachable.AttachPoint(hoveringPoint);
			}
		}

		// Attach the point to the line
		line.points[placing == PlacingStage.Point ? 0 : 1] = hoveringPoint;
		hoveringPoint.attachedElements.Add(line);
	}

    public override Element GetSelectedElement()
    {
        return line;
    }

    public enum PlacingStage
    {
        None,
        Point,
        Line
    }
}
