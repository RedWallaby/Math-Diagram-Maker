using UnityEngine;
using static LineCreator;
using UnityEngine.EventSystems;

public class AngleCreator : DiagramEditor
{
    public PlacingStage placing;
    public Angle angle;

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (placing != PlacingStage.None) return;
        diagram.SetEditor(this);
    }

    public override void ActivateEdit()
    {
        placing = PlacingStage.Point;
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
        placing = PlacingStage.None;
    }

    public void Update()
    {
        if (placing == PlacingStage.None) return;

        if (!diagram.clickedOverDiagram) return;

        Vector2 placingPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Line line = diagram.GetProminentAttachable(ref placingPosition) as Line;
        if (line == null) return; // No line found, exit early

        if (placing == PlacingStage.Point)
        {
            angle.lines[0] = line;
            placing = PlacingStage.Line;
        }
        else if (placing == PlacingStage.Line)
        {
            if (angle.lines[0] == line || !line.GetSharedPoint(angle.lines[0])) return;
            angle.lines[1] = line;
            angle.GetAngleData();
            angle.DrawAngle();
            placing = PlacingStage.None;
            angle = null;
            ActivateEdit();
        }
    }
}
