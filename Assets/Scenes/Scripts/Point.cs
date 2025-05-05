using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public Vector2 position => gameObject.transform.position;
    public List<Line> attatchedLines;
    public Line semiAttatchedLine;
    public CircleCollider2D col;

	public void Awake()
	{
		col = GetComponent<CircleCollider2D>();
		if (col == null)
		{
			col = gameObject.AddComponent<CircleCollider2D>();
		}
	}
}
