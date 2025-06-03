using System;
using System.Collections.Generic;
using UnityEngine;

public class Line : Attachable
{
    public Point[] points = new Point[2];
    public LineRenderer line;
    public PolygonCollider2D col;
    public float colliderWidthMultiplier = 2f;
    public float length => Vector2.Distance(line.GetPosition(0), line.GetPosition(1));

    public override string LabelData
    {
        get
        {
            return Math.Round(length, 2).ToString();
        }
    }

    public override Vector2 LabelPosition     {
        get
        {
            return gameObject.transform.position;
        }
    }

    public void Awake()
    {
        line = GetComponent<LineRenderer>();
        if (line == null)
        {
            line = gameObject.AddComponent<LineRenderer>();
        }
        col = GetComponent<PolygonCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<PolygonCollider2D>();
        }
    }

    public void ForceUpdateLineRenderer()
    {
        line.SetPosition(0, points[0].transform.position);
        line.SetPosition(1, points[1].transform.position);
    }

    public void SetPosition()
    {
        transform.position = (line.GetPosition(0) + line.GetPosition(1)) / 2;
    }

    // Calculates the distance of a position as a ratio to each end of the line, where 0 is at the start of the line and 1 is at the end
    public override float CalculatePercentage(Vector2 point)
    {
        Vector2 startPoint = line.GetPosition(0);
        Vector2 endPoint = line.GetPosition(1);
        float percentage = 0;
        if (startPoint.x == endPoint.x) //Vertical line
            percentage = (point.y - startPoint.y) / (endPoint.y - startPoint.y);
        else
            percentage = (point.x - startPoint.x) / (endPoint.x - startPoint.x);
        return percentage;
    }

    // Move one of the points to a new position
    public void UpdatePointPosition(Point point, Vector3 position)
    {
        int index = System.Array.IndexOf(points, point);
        if (index == -1)
        {
            Debug.LogError("Point not attached to line");
            return;
        }

        // Update attached points
        foreach (Point attachedPoint in attachedPoints)
        {
            Vector2 newPosition = Vector2.Lerp(index == 0 ? position : line.GetPosition(0), index == 1 ? position : line.GetPosition(1), attachedPoint.percentage);
            foreach (Line attachedLine in attachedPoint.attatchedLines)
            {
                attachedLine.UpdatePointPosition(attachedPoint, newPosition);
            }
            attachedPoint.gameObject.transform.position = newPosition;
        }

        // Update the point's position to the average of its point's positions
        SetPosition();
        SetLabel();
        //Update the line renderer position
        line.SetPosition(index, position);
    }

    // Uses a mathemtical formula to calculate the closest point on a line
    public override Vector2 GetClosestPosition(Vector2 point)
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


    // CLICKABLE LINE 
    public void LateUpdate()
    {
        col.pathCount = line.positionCount - 1;
        Vector3[] linePoints = new Vector3[line.positionCount];
        line.GetPositions(linePoints);
        List<Vector2> colliderPoints = CalculatePoints(linePoints[0], linePoints[1]);
        col.SetPath(0, colliderPoints.ConvertAll(point => (Vector2)transform.InverseTransformPoint(point)).ToArray()); // InverseTransformPoint converts world space to local space which is required for collider paths
    }

    public List<Vector2> CalculatePoints(Vector2 pos1, Vector2 pos2)
    {
        float width = line.startWidth * colliderWidthMultiplier;
        float x = pos2.x - pos1.x;
        float y = pos1.y - pos2.y;
        float deltaX = width / 2 * y / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        float deltaY = width / 2 * x / Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        List<Vector2> points = new()
        {
            pos1 + new Vector2(deltaX, deltaY),
            pos2 + new Vector2(deltaX, deltaY),
            pos2 - new Vector2(deltaX, deltaY),
            pos1 - new Vector2(deltaX, deltaY)
        };
        return points;
    }


    // EASE OF USE FUNCTIONS
    public Point GetSharedPoint(Line line)
    {
        foreach (Point point in points)
        {
            if (line.points[0] == point || line.points[1] == point)
            {
                return point;
            }
        }
        return null;
    }

    public Vector2 GetLineVector(Point point)
    {
        if (point == points[0])
        {
            return points[1].position - points[0].position;
        }
        else if (point == points[1])
        {
            return points[0].position - points[1].position;
        }
        else
        {
            Debug.LogError("Point not attached to line");
            return Vector2.zero;
        }
    }

    public override void Delete(Diagram diagram = null)
    {
        foreach (Point point in attachedPoints)
        {
            point.semiAttachedLine = null;
            point.percentage = 0f;
        }
        foreach (Point point in points)
        {
            if (!point) continue; // In case the point was already deleted or hasn't been registered yet
            point.attatchedLines.Remove(this);
        }
        diagram?.elements.Remove(this);
        DestroyImmediate(gameObject);
    }
}
