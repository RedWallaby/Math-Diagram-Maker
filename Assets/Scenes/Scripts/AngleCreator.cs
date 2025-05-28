using UnityEngine;
using static LineCreator;
using UnityEngine.EventSystems;

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
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameObject angleObj = Instantiate(diagram.anglePrefab, mousePosition, Quaternion.identity, diagram.transform);
        angle = angleObj.GetComponent<Angle>();

        // Settings
        //circle.line.startWidth = diagram.lineWidth;
        //circle.colliderWidthMultiplier = diagram.colliderWidthMultiplier;
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
            placing = AngleStage.None;
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
