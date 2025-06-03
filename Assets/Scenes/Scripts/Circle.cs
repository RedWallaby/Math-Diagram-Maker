using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Circle : Attachable
{
    public LineRenderer line;
    public Point centre;
    public float radius;
    public int accuracy = 360;
    public CircleCollider2D col;
    public float colliderWidthMultiplier = 2f;

    public Vector3 previousCentre;

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

    public override float CalculatePercentage(Vector2 point)
    {
        float angle = Mathf.Atan2(point.y - centre.position.y, point.x - centre.position.x) * Mathf.Rad2Deg;
        return angle / 360f;
    }

    public Vector2 PositionFromPercentage(float percentage)
    {
        float angle = percentage * 360f;
        float x = centre.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
        float y = centre.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
        return new Vector2(x, y);
    }

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

    public void UpdateCentrePosition(Vector2 position)
    {
        transform.position = position;
        DrawCircle();
        foreach (Point point in attachedPoints)
        {
            point.UpdatePoint(PositionFromPercentage(point.percentage));
        }
    }

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

    public override void Delete(Diagram diagram = null)
    {
        foreach (Point point in attachedPoints)
        {
            point.semiAttachedLine = null;
            point.percentage = 0f;
        }
        if (centre) centre.circles.Remove(this);
        diagram?.elements.Remove(this);
        DestroyImmediate(gameObject);
    }
}
