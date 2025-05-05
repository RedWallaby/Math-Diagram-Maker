using System.Collections.Generic;
using UnityEngine;

public class Diagram : MonoBehaviour
{
    public List<Point> points = new();
    public List<Line> lines = new();
    public GameObject pointPrefab;
    
    public Point CreateInstantPoint(Vector2 position) {
        GameObject pointObj = Instantiate(pointPrefab, position, Quaternion.identity, transform);
		Point point = pointObj.GetComponent<Point>();
		points.Add(point);
        return point;
	}
}
