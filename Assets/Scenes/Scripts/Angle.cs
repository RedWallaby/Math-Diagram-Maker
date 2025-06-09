using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Angle : Element
{
    public LineRenderer line;
    public PolygonCollider2D col;
    public float maxRadius;
    private float startAngle;
    private float angleOffset;
    private float radius;
    public Point[] points = new Point[3]; // 0: start, 1: centre, 2: end

    public override string LabelData
    {
        get
        {
            return Math.Round(angleOffset, 1).ToString();
        }
    }

    public override Vector2 LabelPosition
    {
        get
        {
            if (points[1] == null) return Vector2.zero;
            float x = points[1].transform.position.x + Mathf.Cos((startAngle + angleOffset / 2) * Mathf.Deg2Rad) * radius * 1.25f;
            float y = points[1].transform.position.y + Mathf.Sin((startAngle + angleOffset / 2) * Mathf.Deg2Rad) * radius * 1.25f;
            return new Vector3(x, y, 0);
        }
    }

    /// <summary>
    /// Sets the angle data based on the positions of the points and an optional override end point
    /// </summary>
    /// <param name="overrideEnd"></param>
    /// <remarks>
    /// Finds the minimum non-negative angle between the two directions formed by the first two points and the override end point
    /// </remarks>
    public void GetAngleData(Vector2 overrideEnd)
    {
        Vector2 direction0 = points[0].position - points[1].position;
        Vector2 direction1 = overrideEnd - points[1].position;
        angleOffset = Vector2.SignedAngle(direction0, direction1);
        if (angleOffset < 0)
        {
            startAngle = Vector2.SignedAngle(Vector2.right, direction1);
            angleOffset = -angleOffset;
        }
        else
        {
            startAngle = Vector2.SignedAngle(Vector2.right, direction0);
        }
        float lowestLength = Mathf.Min(direction0.magnitude, direction1.magnitude);
        radius = Mathf.Min(maxRadius, lowestLength / 2f);
    }

    public void GetAngleData()
    {
        GetAngleData(points[2].position);
    }

    /// <summary>
    /// Updates the line renderer to draw the angle arc based on the start angle, angle offset, and radius
    /// </summary>
    public void DrawAngle()
    {
        line.positionCount = (int) Mathf.Ceil(angleOffset / 5);
        float angle = startAngle;
        for (int i = 0; i < line.positionCount; i++)
        {
            float x = points[1].transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = points[1].transform.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            line.SetPosition(i, new Vector2(x, y));
            angle += angleOffset / (line.positionCount - 1);
        }
    }

    /// <summary>
    /// Draws the hitbox of the angle onto the polygon collider
    /// </summary>
    public void DrawHitbox()
    {
        Vector2[] positions = new Vector2[Mathf.Max((int) Mathf.Ceil(angleOffset) / 10, 2) + 1];
        positions[0] = points[1].position;
        float angle = startAngle;
        for (int i = 1; i < positions.Length; i++)
        {
            float x = points[1].transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = points[1].transform.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            positions[i] = new Vector2(x, y);
            angle += angleOffset / (positions.Length - 2);
        }
        col.pathCount = 1;
        col.SetPath(0, Array.ConvertAll(positions, point => (Vector2)transform.InverseTransformPoint(point)));
    }

    /// <summary>
    /// Updates the angle based on the positions of the points
    /// </summary>
    /// <param name="point">The updated <c>Point</c></param>
    /// <param name="position">The position that the point will be moved to</param>
    public override void UpdatePointPosition(Point point, Vector2 position)
    {
        Vector2 originalPosition = point.transform.position;
        point.transform.position = position;
        GetAngleData();
        transform.position = points[1].transform.position;
        DrawAngle();
        DrawHitbox();
        SetLabel();
        point.transform.position = originalPosition; // Reset position to avoid moving the point yet
    }

    public override void Delete(Diagram diagram = null)
    {
        foreach (Point point in points)
        {
            if (!point) continue;
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
