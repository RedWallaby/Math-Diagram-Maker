using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Circle : Attachable
{
    public LineRenderer line;
    public Point centre;
    public List<Point> attachedPoints = new();
    public float radius;
    public int accuracy = 360;
    public CircleCollider2D col;
    public float colliderWidthMultiplier = 2f;

    public Vector3 previousCentre;

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

    public override void AttachPoint(Point point)
    {
        point.percentage = CalculatePercentage(point.position);
        point.semiAttatchedLine = this;
        attachedPoints.Add(point);
    }

    public override float CalculatePercentage(Vector2 point)
    {
        float angle = Mathf.Atan2(point.y - centre.transform.position.y, point.x - centre.transform.position.x) * Mathf.Rad2Deg;
        return angle / 360f;
    }

    public Vector2 PositionFromPercentage(float percentage)
    {
        float angle = percentage * 360f;
        float x = centre.transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
        float y = centre.transform.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
        return new Vector2(x, y);
    }

    // Sets the radius of the circle based on the distance from the centre to the given position
    public void SetRadius(Vector2 position)
    {
        float radius = Vector2.Distance(centre.position, position);
        this.radius = radius;
        DrawCircle();

        foreach (Point point in attachedPoints)
        {
            point.UpdatePoint(PositionFromPercentage(point.percentage));
        }
    }

    public void CreateCircle()
    {
        line.positionCount = accuracy;
        line.loop = true;
        previousCentre = centre.transform.position;
        DrawCircle();
    }

    public void DrawCircle()
    {
        col.radius = (radius + line.startWidth * colliderWidthMultiplier / 2) * 100;
        float angle = 0f;
        for (int i = 0; i < line.positionCount; i++)
        {
            angle += (360f / line.positionCount);
            float x = centre.transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = centre.transform.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            line.SetPosition(i, new Vector2(x, y));
        }
    }

    public override Vector2 GetClosestPosition(Vector2 point)
    {
        float distance = Vector2.Distance(centre.transform.position, point);
        // Prevent NAN Values and incorrect calculations
        if (distance == 0)
        {
            return new Vector2(centre.transform.position.x, centre.transform.position.y + radius);
        }
        float ratio = radius / distance;
        float xDiff = point.x - centre.transform.position.x;
        float yDiff = point.y - centre.transform.position.y;
        return new Vector2(centre.transform.position.x + xDiff * ratio, centre.transform.position.y + yDiff * ratio);
    }

    public void LateUpdate()
    {
        if (!centre) return;
        if (previousCentre != centre.transform.position)
        {
            transform.position = centre.transform.position;
            DrawCircle();
            foreach (Point point in attachedPoints)
            {
                point.UpdatePoint(PositionFromPercentage(point.percentage));
            }
        }
        previousCentre = centre.transform.position;
    }

    public override void ToggleLabel()
    {
        throw new System.NotImplementedException();
    }
}
