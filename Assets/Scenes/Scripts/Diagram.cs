using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Diagram : MonoBehaviour
{
    public List<Point> points = new();
    public List<Line> lines = new();
    public GameObject pointPrefab;
    public GameObject linePrefab;

    public PointCreator pointCreator;
    public LineCreator lineCreator;

    public DiagramEditor currentEditor;

/*    public enum EditingType
    {
        None,
        Point,
        Line,
        Move
    }
    public EditingType editingType;*/
    
    public Point CreateInstantPoint(Vector2 position) {
        GameObject pointObj = Instantiate(pointPrefab, position, Quaternion.identity, transform);
		Point point = pointObj.GetComponent<Point>();
		points.Add(point);
        return point;
	}

    public void SetEditor(DiagramEditor editor)
    {
        if (currentEditor == editor) return; // Prevent re-assigning the same editor
        if (currentEditor != null) // Only deactivate if there is an active editor
        {
            currentEditor.DeactivateEdit();
        }
        currentEditor = editor;
        currentEditor.ActivateEdit();
    }

    // Gives first found feature (point/line) on a certain position (gives points priority)
    public void GetProminentFeature(ref Vector3 position, out Point hovPoint, out Line hovLine)
    {
        hovPoint = null;
        hovLine = null;
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Point point))
            {
                position = point.position;
                hovPoint = point;
                return;
            }
        }
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Line line))
            {
                position = line.CalculateClosestPosition(position);
                hovLine = line;
                return;
            }
        }
    }

    public Point GetPointAtPosition(Vector2 position, Func<Point, bool> exclusion = null)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Point point))
            {
                if (exclusion != null && exclusion(point)) continue; // Skip the excluded points
                return point;
            }
        }
        return null;
    }
}
