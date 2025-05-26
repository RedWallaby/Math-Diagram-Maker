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
        GameObject circleObj = Instantiate(diagram.circlePrefab, mousePosition, Quaternion.identity, diagram.transform);
        circle = circleObj.GetComponent<Circle>();
        circle.col.enabled = false; // Disable the collider until placement is confirmed

        // Settings
        circle.line.startWidth = diagram.lineWidth;
        circle.colliderWidthMultiplier = diagram.colliderWidthMultiplier;
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
            diagram.GetProminentFeature(ref placingPosition, out Point hoveringPoint, out Attachable attachable);

            if (!diagram.clickedOverDiagram) return;

            if (!hoveringPoint)
            {
                hoveringPoint = diagram.CreateInstantPoint(placingPosition);
                if (attachable)
                {
                    attachable.AttachPoint(hoveringPoint);
                }
            }
            circle.centre = hoveringPoint;
            circle.gameObject.transform.position = placingPosition;
            circle.CreateCircle();
            placing = PlacingStage.Line;
        }
        else if (placing == PlacingStage.Line)
        {
            circle.SetRadius(placingPosition);

            if (!diagram.clickedOverDiagram) return;

            circle.col.enabled = true;
            circle = null;
            ActivateEdit();
        }
    }
}
