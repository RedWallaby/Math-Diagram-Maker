using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static LineCreator;

public class CircleCreator : DiagramEditor
{
    public Circle circle;
    public PlacingStage placing;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (placing != PlacingStage.None) return;
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
        Destroy(circle.gameObject);
        circle = null;
        placing = PlacingStage.None;
    }

    public void Update()
    {
        if (placing == PlacingStage.None) return;
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

    public void SetupPoint(Vector2 position)
    {
		diagram.GetProminentFeature(ref position, out Point hoveringPoint, out Attachable attachable);

		if (!diagram.clickedOnDiagram) return;

		if (!hoveringPoint)
		{
			hoveringPoint = diagram.CreatePoint(position);
			if (attachable)
			{
				attachable.AttachPoint(hoveringPoint);
			}
		}

		circle.centre = hoveringPoint;
		circle.gameObject.transform.position = position;
		circle.CreateCircle();
		placing = PlacingStage.Line;
	}

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
