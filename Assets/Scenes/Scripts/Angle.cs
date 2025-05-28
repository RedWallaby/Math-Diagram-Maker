using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Angle : Element
{
    public LineRenderer line;
    public float startAngle;
    public float angleOffset;
    public float radius;
    public float maxRadius = 0.5f;

    public PolygonCollider2D col;

    public Point start;
    public Point centre;
    public Point end;

    public Vector2 prevStart;
    public Vector2 prevCentre;
    public Vector2 prevEnd;

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
            if (centre == null) return Vector2.zero;
            float x = centre.transform.position.x + Mathf.Cos((startAngle + angleOffset / 2) * Mathf.Deg2Rad) * radius * 1.25f;
            float y = centre.transform.position.y + Mathf.Sin((startAngle + angleOffset / 2) * Mathf.Deg2Rad) * radius * 1.25f;
            return new Vector3(x, y, 0);
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

    /*public void GetAngleDataOLD()
    {
        if (lines[0] == null || lines[1] == null) return;
        centre = lines[0].GetSharedPoint(lines[1]);
        Vector2 direction0 = lines[0].GetLineVector(centre);
        Vector2 direction1 = lines[1].GetLineVector(centre);
        startAngle = Vector2.SignedAngle(Vector2.right, direction0);
        angleOffset = Vector2.SignedAngle(direction0, direction1);
        if (angleOffset < 0) angleOffset += 360f;

        float lowestLength = Mathf.Min(direction0.magnitude, direction1.magnitude);
        actualRadius = Mathf.Min(radius, lowestLength / 2f);
    }*/

    public void GetAngleData(Vector2 overrideEnd)
    {
        if (start == null || centre == null) return;
        Vector2 direction0 = start.position - centre.position;
        Vector2 direction1 = overrideEnd - centre.position;
        //startAngle = Vector2.SignedAngle(Vector2.right, direction0);
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
        if (start == null || centre == null) return;
        Vector2 direction0 = start.position - centre.position;
        Vector2 direction1 = end.position - centre.position;
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

    public void DrawAngle()
    {
        line.positionCount = (int) Mathf.Ceil(angleOffset / 5);
        float angle = startAngle;
        for (int i = 0; i < line.positionCount; i++)
        {
            float x = centre.transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = centre.transform.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            line.SetPosition(i, new Vector2(x, y));
            angle += angleOffset / (line.positionCount - 1);
        }
    }

    public void DrawHitbox()
    {
        Vector2[] positions = new Vector2[Mathf.Max((int) Mathf.Ceil(angleOffset) / 10, 2) + 1];
        positions[0] = centre.position;
        float angle = startAngle;
        for (int i = 1; i < positions.Length; i++)
        {
            float x = centre.transform.position.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            float y = centre.transform.position.y + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            positions[i] = new Vector2(x, y);
            angle += angleOffset / (positions.Length - 2);

        }
        col.pathCount = 1;
        col.SetPath(0, Array.ConvertAll(positions, point => (Vector2)transform.InverseTransformPoint(point)));
    }

    private void LateUpdate()
    {
        if (start == null || centre == null || end == null) return;

        if (prevStart == start.position && prevCentre == centre.position && prevEnd == end.position) return;

        GetAngleData();
        transform.position = centre.transform.position;
        DrawAngle();
        DrawHitbox();
        SetLabel();

        prevStart = start.position;
        prevCentre = centre.position;
        prevEnd = end.position;
    }
}
