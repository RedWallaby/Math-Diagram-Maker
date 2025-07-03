using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class AngleCreator : DiagramEditor
{
    public AngleStage placing;
    public Angle angle;

    public override string NotificationText => "Select 3 points to create an angle between";

    public override void OnPointerClick(PointerEventData eventData)
    {
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        placing = AngleStage.Start;
        angle = diagram.CreateAngle();
    }

    public override void DeactivateEdit()
    {
        angle.Delete();
        angle = null;
        placing = AngleStage.None;
    }

    public override void Tick()
    {
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

    /// <summary>
    /// Proceeds to the next stage of angle creation based on the current placing stage
    /// </summary>
    public void EnterNextAngleStage(Point point)
    {
        if (angle.points.Contains(point))
        {
            diagram.notification.SetNotification("This point is already part of the angle", 2f);
            return;
        }
        if (placing == AngleStage.Start)
        {
            angle.points[0] = point;
            point.attachedElements.Add(angle);
            placing = AngleStage.Centre;
        }
        else if (placing == AngleStage.Centre)
        {
            angle.points[1] = point;
            point.attachedElements.Add(angle);
            placing = AngleStage.End;
        }
        else if (placing == AngleStage.End)
        {
            diagram.elements.Add(angle);

            angle.points[2] = point;
            point.attachedElements.Add(angle);
            angle.GetAngleData();
            angle.DrawAngle();
            angle.DrawHitbox();
            ActivateEdit();
        }
    }

    public override Element GetSelectedElement()
    {
        return angle;
    }

	public enum AngleStage
    {
        None,
        Start,
        Centre,
        End
    }
}
