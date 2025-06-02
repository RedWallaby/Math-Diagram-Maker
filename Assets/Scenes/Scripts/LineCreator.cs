using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class LineCreator : DiagramEditor
{
    public Line line;
    public PlacingStage placing;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (placing != PlacingStage.None) return;
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
        Destroy(line.gameObject);
        line = null;
        placing = PlacingStage.None;
    }

    public void Update()
    {
        if (placing == PlacingStage.None) return;
        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        diagram.GetProminentFeature(ref placingPosition, out Point hoveringPoint, out Attachable attachable);
        if (placing == PlacingStage.Point)
        {
			if (!diagram.clickedOnDiagram) return;

			line.line.SetPosition(0, placingPosition);
			line.line.SetPosition(1, placingPosition); // Set both points to the same position initially (prevents random line from being drawn)

			ProcessLine(placingPosition, hoveringPoint, attachable);
			placing = PlacingStage.Line;
		}
		else
        {
            if (!attachable) diagram.LockPositionToGrid(ref placingPosition);
            line.line.SetPosition(1, placingPosition);

            if (!diagram.clickedOnDiagram) return;

            ProcessLine(placingPosition, hoveringPoint, attachable);

            diagram.elements.Add(line);
            line.SetPosition();
            line.col.enabled = true;
            line = null;
            ActivateEdit();
        }
    }

    public void ProcessLine(Vector2 position, Point hoveringPoint, Attachable attachable)
    {
		// Create a new point if no point is currently selected
		if (!hoveringPoint)
		{
			hoveringPoint = diagram.CreatePoint(position);
			// Account for if the line is snapping to another line or circle
			if (attachable)
			{
				attachable.AttachPoint(hoveringPoint);
			}
		}

		// Attach the point to the line
		line.points[placing == PlacingStage.Point ? 0 : 1] = hoveringPoint;
		hoveringPoint.attatchedLines.Add(line);
	}

    public enum PlacingStage
    {
        None,
        Point,
        Line
    }
}
