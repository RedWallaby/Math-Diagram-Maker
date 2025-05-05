using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public Point[] points = new Point[2];
    public List<Point> attatchedPoints = new();
    public LineRenderer line;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (line == null)
        {
            line = gameObject.AddComponent<LineRenderer>();
        }
    }

    //Only works given that the line contains two points
    //Uses a mathemtical formula to calculate the closest point on a line
    public Vector2 CalculateClosestPosition(Vector2 point)
    {
        Vector3[] linePoints = new Vector3[line.positionCount];
        line.GetPositions(linePoints);
        Vector2 startPoint = linePoints[0];
        Vector2 endPoint = linePoints[1];

        float gradient = (endPoint.y - startPoint.y) / (endPoint.x - startPoint.x);

        // This formula is a rearrangement of the line equation y = mx + b using two points to form a line segment
        float x = (point.y - startPoint.y + gradient * startPoint.x + point.x / gradient) / (gradient + 1 / gradient);
        float y = gradient * (x - startPoint.x) + startPoint.y;

        if (endPoint.y == startPoint.y) // Handle horizontal line case
        {
            x = point.x;
            y = startPoint.y;
        }
        else if (endPoint.x == startPoint.x) // Handle vertical line case
        {
            x = startPoint.x;
            y = point.y;
        }

        // Check if the point is within the bounds of the line segment
        if (x < Mathf.Min(startPoint.x, endPoint.x) || x > Mathf.Max(startPoint.x, endPoint.x) ||
            y < Mathf.Min(startPoint.y, endPoint.y) || y > Mathf.Max(startPoint.y, endPoint.y))
        {
            // Return the closest endpoint if the point is outside the line segment
            return Vector2.Distance(point, startPoint) < Vector2.Distance(point, endPoint) ? startPoint : endPoint;
        }
        return new Vector2(x, y);
    }
}
