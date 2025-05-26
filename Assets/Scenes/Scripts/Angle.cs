using UnityEngine;
using UnityEngine.Rendering;

public class Angle : Element
{
    public LineRenderer line;
    public float startAngle;
    public float angleOffset;
    public float actualRadius;
    public float radius = 0.25f;

    public Point centre;
    public Line[] lines = new Line[2];

    // Create the angle with only one given line
    public void GetTempAngleData(Vector2 position)
    {

    }

    public void GetAngleData()
    {
        if (lines[0] == null || lines[1] == null) return;
        centre = lines[0].GetSharedPoint(lines[1]);
        Vector2 direction0 = lines[0].GetLineVector(centre);
        Vector2 direction1 = lines[1].GetLineVector(centre);
        startAngle = Vector2.SignedAngle(Vector2.right, direction0);
        angleOffset = Vector2.SignedAngle(direction0, direction1); //SWITCH THESE FOR THE OTHER ANGLE
        if (angleOffset < 0) angleOffset += 360f;

        float lowestLength = Mathf.Min(direction0.magnitude, direction1.magnitude);
        actualRadius = Mathf.Min(radius, lowestLength / 2f);
    }

    public void DrawAngle()
    {
        line.positionCount = (int) Mathf.Ceil(angleOffset);
        float angle = startAngle;
        for (int i = 0; i < line.positionCount; i++)
        {
            angle += angleOffset / line.positionCount; //SWITCH FROM += to -= FOR THE OTHER ANGLE
            float x = centre.transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * actualRadius;
            float y = centre.transform.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * actualRadius;
            line.SetPosition(i, new Vector2(x, y));
        }
    }

    private void LateUpdate()
    {
        if (lines[0] == null || lines[1] == null) return;
        GetAngleData();
        if (centre == null) return;
        transform.position = centre.transform.position;
        DrawAngle();
    }

    public override void ToggleLabel()
    {
        throw new System.NotImplementedException();
    }
}
