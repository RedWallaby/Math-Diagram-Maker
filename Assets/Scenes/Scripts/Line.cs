using System;
using System.Collections.Generic;
using UnityEngine;

public class Line : Attachable
{
    public PolygonCollider2D col;
    public LineRenderer line;

    public Point[] points = new Point[2];
    public float colliderWidthMultiplier = 2f;
    public float Length => Vector2.Distance(line.GetPosition(0), line.GetPosition(1));

    public override string LabelData
    {
        get
        {
            return Math.Round(Length, 2).ToString();
        }
    }

    public override Vector2 LabelPosition
    {
        get
        {
            return gameObject.transform.position;
        }
    }

    /// <summary>
    /// Sets the polygon collider path based on the line's <c>Point</c> positions.
    /// </summary>
    public void DrawLineHitbox()
    {
        col.pathCount = line.positionCount - 1;
        Vector3[] linePoints = new Vector3[line.positionCount];
        line.GetPositions(linePoints);
        List<Vector2> colliderPoints = CalculatePoints(linePoints[0], linePoints[1]);
        col.SetPath(0, colliderPoints.ConvertAll(point => (Vector2)transform.InverseTransformPoint(point)).ToArray()); // InverseTransformPoint converts world space to local space which is required for collider paths
    }

    /// <summary>
    /// Gets the world space coordinates of the points that form the collider of the line
    /// </summary>
    /// <param name="pos1">The first position</param>
    /// <param name="pos2">The second position</param>
    /// <returns>A list containing 4 elements, corresponding to the vertexes of the line's collider</returns>
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

    public void SetPosition()
    {
        transform.position = (line.GetPosition(0) + line.GetPosition(1)) / 2;
    }


    public void ForceUpdateLineRenderer()
    {
        line.SetPosition(0, points[0].transform.position);
        line.SetPosition(1, points[1].transform.position);
    }

    /// <summary>
    /// Gets the closest position on the <c>Line</c>> from a given world space position
    /// </summary>
    /// <param name="point">The referenced position</param>
    /// <returns>The world space position on the <c>Line</c></returns>
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

    /// <summary>
    /// Updates the <c>Line</c> and its attached points based on the new position of the <c>Point</c>>
    /// </summary>
    /// <param name="point">The <c>Point</c> that will move</param>
    /// <param name="placingPosition">The new position for the <c>Point<c/></param>
    public override void UpdatePointPosition(Point point, Vector2 position)
    {
        int index = Array.IndexOf(points, point);
        if (index == -1)
        {
            Debug.LogError("Point not attached to line");
            return;
        }

        // Update attached points
        foreach (Point attachedPoint in attachedPoints)
        {
            Vector2 newPosition = Vector2.Lerp(index == 0 ? position : line.GetPosition(0), index == 1 ? position : line.GetPosition(1), attachedPoint.percentage);
            foreach (Element element in attachedPoint.attachedElements)
            {
                element.UpdatePointPosition(attachedPoint, newPosition);
            }
            attachedPoint.gameObject.transform.position = newPosition;
        }

        // Update the line's position to the average of its point's positions
        SetPosition();
        SetLabel();
        line.SetPosition(index, position);
        DrawLineHitbox();
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
            point.attachedElements.Remove(this);
        }
        diagram?.elements.Remove(this);
        DestroyImmediate(gameObject);
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
}
