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
    public GameObject circlePrefab;

    public PointCreator pointCreator;
    public LineCreator lineCreator;

    public DiagramEditor currentEditor;

    // Main settings
    [Header("Diagram Settings")]
    [Header("Line Settings")]
    public float lineWidth = 0.075f;
    public float colliderWidthMultiplier = 2f;

    [Header("Point Settings")]
    public float pointRadius = 0.3f;



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

        // Settings
        RectTransform rt = pointObj.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pointRadius * 100);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pointRadius * 100);
        point.GetComponent<CircleCollider2D>().radius = pointRadius * 50;

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

    // Gives first found feature (point/line/circle) on a certain position (giving a priority of point -> line / circle, hence the 2 different loops)
    public Attachable GetProminentAttachable(ref Vector2 position)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Attachable attachable))
            {
                if (attachable is Circle circle)
                {
                    if (Vector2.Distance(circle.centre.position, position) < (circle.radius - lineWidth * colliderWidthMultiplier / 2)) continue; // Skip if the point is not within the circles ring
                }
                position = attachable.GetClosestPosition(position);
                return attachable;
            }
        }
        return null;
    }

    public void GetProminentFeature(ref Vector2 position, out Point point, out Attachable attachable)
    {
        point = GetPointAtPosition(position);
        attachable = null;
        if (point)
        {
            position = point.position;
        }
        else
        {
            attachable = GetProminentAttachable(ref position);
        }
    }
/*

    public T GetAttachableAtPosition<T>(Vector2 position, Func<T, bool> exclusion = null) where T : Attachable
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Attachable attachable) && attachable is T)
            {
                if (exclusion != null && exclusion((T) attachable)) continue; // Skip the excluded points
                if (attachable is Circle) 
                {
                    if (Vector2.Distance(((Circle) attachable).centre.position, position) < ((Circle) attachable).radius - lineWidth / 2) continue; // Skip if the point is not within the circles ring
                }
                return (T) attachable;
            }
        }
        return null;
    }*/
}
