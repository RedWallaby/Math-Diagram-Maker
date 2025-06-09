using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Circle : Attachable
{
    public CircleCollider2D col;
    public LineRenderer line;

    public Point centre;
    public float radius;
    public int accuracy;
    public float colliderWidthMultiplier;

    public override string LabelData
    {
        get
        {
            return "r = " + Math.Round(radius, 2).ToString();
        }
    }

    public override Vector2 LabelPosition
    {
        get
        {
            if (centre == null) return Vector2.zero;
            return centre.transform.position + new Vector3(0, radius, 0);
        }
    }

    /// <summary>
    /// Renders the circle based on its radius and centre <c>Point</c>
    /// </summary>
    public void DrawCircle()
    {
        col.radius = (radius + line.startWidth * colliderWidthMultiplier / 2) * 100;
        float angle = 0f;
        for (int i = 0; i < line.positionCount; i++)
        {
            angle += (360f / line.positionCount);
            float x = centre.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = centre.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            line.SetPosition(i, new Vector2(x, y));
        }
    }

    /// <summary>
    /// Sets the radius of the circle and updates the line renderer and attached points accordingly.
    /// </summary>
    /// <param name="radius">The new radius of the <c>Circle</c></param>
    public void SetRadius(float radius)
    {
        this.radius = radius;
        DrawCircle();

        foreach (Point point in attachedPoints)
        {
            point.UpdatePoint(PositionFromPercentage(point.percentage));
        }
        SetLabel();
    }


    /// <summary>
    /// Gets the closest position on the <c>Circle</c> from a given world space position
    /// </summary>
    /// <param name="point">The referenced position</param>
    /// <returns>The world space position on the <c>Circle</c></returns>
    public override Vector2 GetClosestPosition(Vector2 point)
    {
        float distance = Vector2.Distance(centre.position, point);
        // Prevent NAN Values and incorrect calculations
        if (distance == 0)
        {
            return new Vector2(centre.position.x, centre.position.y + radius);
        }
        float ratio = radius / distance;
        float xDiff = point.x - centre.position.x;
        float yDiff = point.y - centre.position.y;
        return new Vector2(centre.position.x + xDiff * ratio, centre.position.y + yDiff * ratio);
    }

    /// <summary>
    /// Calculates the percentage of a point's position along the circumference of the circle
    /// </summary>
    /// <remarks>
    /// A percentage of 0 corresponds to the rightmost point of the circle, and increases counter-clockwise, until it reaches 1 at the same point again
    /// </remarks>
    /// <param name="point">The <c>Point</c> to calculate from</param>
    /// <returns>A percentage between 0 and 1</returns>
    public override float CalculatePercentage(Vector2 point)
    {
        float angle = Mathf.Atan2(point.y - centre.position.y, point.x - centre.position.x) * Mathf.Rad2Deg;
        return angle / 360f;
    }

    /// <summary>
    /// Calculates the position of a point on the circumference of the circle based on its percentage
    /// </summary>
    /// <param name="percentage">A percentage between 0 and 1</param>
    /// <returns>The new position on the circle's circumference</returns>
    public Vector2 PositionFromPercentage(float percentage)
    {
        float angle = percentage * 360f;
        float x = centre.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
        float y = centre.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
        return new Vector2(x, y);
    }

    /// <summary>
    /// Updates the <c>Circle</c> and its attached points based on the new position of the <c>Point</c>>
    /// </summary>
    /// <param name="point">The <c>Point</c> that will move</param>
    /// <param name="placingPosition">The new position for the <c>Point<c/></param>
    public override void UpdatePointPosition(Point point, Vector2 position)
    {
        transform.position = position;
        DrawCircle();
        foreach (Point attachedPoint in attachedPoints)
        {
            attachedPoint.UpdatePoint(PositionFromPercentage(attachedPoint.percentage));
        }
    }

    public override void Delete(Diagram diagram = null)
    {
        foreach (Point point in attachedPoints)
        {
            point.semiAttachedLine = null;
            point.percentage = 0f;
        }
        if (centre) centre.attachedElements.Remove(this);
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
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
    }
}
