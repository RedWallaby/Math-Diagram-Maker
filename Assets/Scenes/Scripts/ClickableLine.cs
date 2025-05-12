using System.Collections.Generic;
using UnityEngine;

//-------------------------------------------ARTIFACT CLASS - NOT IN USE-----------------------------------
public class ClickableLine : MonoBehaviour
{
    PolygonCollider2D col;
    LineRenderer line;

    public void Awake()
    {
        col = GetComponent<PolygonCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<PolygonCollider2D>();
        }
        line = GetComponent<LineRenderer>();
    }

    public void LateUpdate()
    {   
        col.pathCount = line.positionCount - 1;
        Vector3[] linePoints = new Vector3[line.positionCount];
        line.GetPositions(linePoints);
        for (int i = 0; i < linePoints.Length - 1; i++)
        {
            List<Vector2> colliderPoints = CalculatePoints(linePoints[i], linePoints[i + 1]);
            col.SetPath(i, colliderPoints.ConvertAll(point => (Vector2)transform.InverseTransformPoint(point)).ToArray());
        }
    }

    public List<Vector2> CalculatePoints(Vector2 pos1, Vector2 pos2)
    {
        float width = line.startWidth * 2; //multiplied by 2 in order to extent the width of the line (2 is an arbitrary multiplier)
        float x = pos2.x - pos1.x;
        float y = pos1.y - pos2.y;
        float deltaX = width/2 * y / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        float deltaY = width/2 * x / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        List<Vector2> points = new()
        {
            pos1 + new Vector2(deltaX, deltaY),
            pos2 + new Vector2(deltaX, deltaY),
            pos2 - new Vector2(deltaX, deltaY),
            pos1 - new Vector2(deltaX, deltaY)
        };
        return points;
    }
}