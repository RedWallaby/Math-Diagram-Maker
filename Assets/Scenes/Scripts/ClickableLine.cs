using System.Collections.Generic;
using UnityEngine;

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

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log("Youve clicked");
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("You've hit something");
                if (hit.transform == transform)
                {
                    Debug.Log("Line clicked!");
                    // Add your click handling logic here
                }
            }
        }
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
        float width = line.startWidth;
        float x = pos2.x - pos1.x;
        float y = pos1.y - pos2.y;
        float deltaX = width/2 * y / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        float deltaY = width/2 * x / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        List<Vector2> points = new List<Vector2>()
        {
            pos1 + new Vector2(deltaX, deltaY),
            pos2 + new Vector2(deltaX, deltaY),
            pos2 - new Vector2(deltaX, deltaY),
            pos1 - new Vector2(deltaX, deltaY)
        };
        return points;
    }
}
