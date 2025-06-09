using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Point : Element
{
    public CircleCollider2D col;

    public Vector2 position => gameObject.transform.position;
    public List<Element> attachedElements;
    public Attachable semiAttachedLine;
	public float percentage;

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

    /// <summary>
    /// Updates the position of the point and all attached elements
    /// </summary>
    /// <param name="placingPosition">The new position of the <c>Point</c></param>
    public void UpdatePoint(Vector2 placingPosition)
    {
        foreach (Element element in attachedElements)
        {
            element.UpdatePointPosition(this, placingPosition);
        }
        gameObject.transform.position = placingPosition;
        SetLabel();
    }

    public override void Delete(Diagram diagram = null)
    {
        while (0 < attachedElements.Count)
        {
            attachedElements[0].Delete(diagram);
        }
        if (semiAttachedLine != null) semiAttachedLine.attachedPoints.Remove(this);
        diagram?.elements.Remove(this);
        DestroyImmediate(gameObject);
    }

    public void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
    }
}
