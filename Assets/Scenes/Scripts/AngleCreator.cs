using UnityEngine;
using static LineCreator;
using UnityEngine.EventSystems;
using System.Drawing;

public class AngleCreator : DiagramEditor
{
    //public PlacingStage placing;
    public AngleStage placing;
    public Angle angle;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (placing != AngleStage.None) return;
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        placing = AngleStage.Start;
        angle = diagram.CreateAngle();
    }

    public override void DeactivateEdit()
    {
        Destroy(angle.gameObject);
        angle = null;
        placing = AngleStage.None;
    }

    public void Update()
    {
        if (placing == AngleStage.None) return;

        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (placing == AngleStage.End)
        {
            angle.GetAngleData(placingPosition);
            angle.DrawAngle();
        }

        if (!diagram.clickedOnDiagram) return;

        Point point = diagram.GetPointAtPosition(placingPosition);
        if (point == null) return;

        EnterNextAngleStage(point);
    }

    public void EnterNextAngleStage(Point point)
    {
        if (placing == AngleStage.Start)
        {
            angle.start = point;
            placing = AngleStage.Centre;
        }
        else if (placing == AngleStage.Centre)
        {
            angle.centre = point;
            placing = AngleStage.End;
        }
        else if (placing == AngleStage.End)
        {
            diagram.elements.Add(angle);

            angle.end = point;
            angle.GetAngleData();
            angle.DrawAngle();
            ActivateEdit();
        }
    }

	public enum AngleStage
    {
        None,
        Start,
        Centre,
        End
    }
}
