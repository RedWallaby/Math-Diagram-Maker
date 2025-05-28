using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Point : Element
{
    public Vector2 position => gameObject.transform.position;
    public List<Line> attatchedLines;
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
        gameObject.transform.position = placingPosition;
        SetLabel();
    }

    public override void Delete(Diagram diagram = null)
    {
        int i = 0;
        while (i < attatchedLines.Count)
        {
            diagram?.elements.Remove(attatchedLines[i]);
            attatchedLines[i].Delete();
        }
        if (semiAttachedLine != null) semiAttachedLine.attachedPoints.Remove(this);
        Destroy(gameObject);
    }
}
