using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Point : Element
{
    public Vector2 position => gameObject.transform.position;
    public List<Line> attatchedLines;
    public Attachable semiAttatchedLine;
	public float percentage;
    public CircleCollider2D col;

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
    }

    public override void ToggleLabel()
    {
        labelText.gameObject.SetActive(!labelText.gameObject.activeSelf);
        labelText.gameObject.transform.position = position;
        labelText.text = $"({position.x}, {position.y})";
    }
}
