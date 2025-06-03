using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Point : Element
{
    public Vector2 position => gameObject.transform.position;
    public List<Line> attatchedLines;
    public List<Circle> circles;
    public Attachable semiAttachedLine;
	public float percentage;
    public CircleCollider2D col;

    public override string LabelData
    {
        get
        {
            return $"({Math.Round(position.x, 1)}, {Math.Round(position.y, 1)})";
        }
    }

    public override Vector2 LabelPosition
    {
        get
        {
            return position;
        }
    }

    public void Awake()
	{
		col = GetComponent<CircleCollider2D>();
		if (col == null)
		{
			col = gameObject.AddComponent<CircleCollider2D>();
		}
	}

    public void UpdatePoint(Vector2 placingPosition)
    {
        foreach (Line line in attatchedLines)
        {
            line.UpdatePointPosition(this, placingPosition);
        }
        foreach (Circle circle in circles)
        {
            circle.UpdateCentrePosition(placingPosition);
        }
        gameObject.transform.position = placingPosition;
        SetLabel();
    }

    public override void Delete(Diagram diagram = null)
    {
        while (0 < attatchedLines.Count)
        {
            attatchedLines[0].Delete(diagram);
        }
        while (0 < circles.Count)
        {
            circles[0].Delete(diagram);
        }
        if (semiAttachedLine != null) semiAttachedLine.attachedPoints.Remove(this);
        diagram?.elements.Remove(this);
        DestroyImmediate(gameObject);
    }
}
