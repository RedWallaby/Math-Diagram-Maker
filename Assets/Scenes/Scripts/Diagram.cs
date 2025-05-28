using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Diagram : MonoBehaviour
{
    public List<Element> elements = new();
    public GameObject pointPrefab;
    public GameObject linePrefab;
    public GameObject circlePrefab;
    public GameObject anglePrefab;

    public DiagramEditor currentEditor;

    public RectTransform labelR;
    public RectTransform editorR;

    public GameObject labelPrefab;

    // Click Data
    public bool isMovingScreen;
    private Vector3 lastMousePosition;

    public bool clickedOnDiagram;

    // Main settings
    [Header("Diagram Settings")]
    [Header("Line Settings")]
    public float lineWidth = 0.075f;
    public float colliderWidthMultiplier = 2f;

    [Header("Point Settings")]
    public float pointRadius = 0.3f;

    public void Update()
    {
        ResolveClickData();
        if (clickedOnDiagram && Input.GetKey(KeyCode.LeftAlt))
        {
            isMovingScreen = true;
            lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (isMovingScreen)
        {
            if (!Input.GetMouseButtonUp(0))
            {
                Camera.main.transform.position -= Camera.main.ScreenToWorldPoint(Input.mousePosition) - lastMousePosition;
            }
            else
            {
                isMovingScreen = false;
            }
            lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 cameraPosition = Camera.main.transform.position;
            
            float originalSize = Camera.main.orthographicSize;

            float d1 = mousePosition.y - cameraPosition.y;
            float d2 = mousePosition.x - cameraPosition.x;

            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - Input.mouseScrollDelta.y * 0.5f, 0.1f, 5f);

            float newSize = Camera.main.orthographicSize;
            float ratio = newSize / originalSize;

            Camera.main.transform.position = new Vector3(mousePosition.x - d2 * ratio, mousePosition.y - d1 * ratio, -10);
        }
    }

    public void ResolveClickData()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            clickedOnDiagram = false;
            return;
        }

        bool withinLabel = MouseInBounds(labelR.position, labelR.position + new Vector3(labelR.rect.width * labelR.localScale.x, -labelR.rect.height * labelR.localScale.y) / 100);
        bool withinEditor = Input.mousePosition.x < editorR.position.x + editorR.rect.width / 2;
        clickedOnDiagram = !withinLabel && !withinEditor;
    }

    public void LockPositionToGrid(ref Vector2 position)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            position.x = Mathf.Round(position.x);
            position.y = Mathf.Round(position.y);
        }
    }

    public bool MouseInBounds(Vector2 topLeft, Vector2 bottomRight)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return topLeft.x < mousePos.x && mousePos.x < bottomRight.x && mousePos.y < topLeft.y && bottomRight.y < mousePos.y;
    }

    public Point CreatePoint(Vector2 position) {
        GameObject pointObj = Instantiate(pointPrefab, position, Quaternion.identity, transform);
		Point point = pointObj.GetComponent<Point>();
		elements.Add(point);

        // Settings
        RectTransform rt = pointObj.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pointRadius * 100);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pointRadius * 100);
        point.GetComponent<CircleCollider2D>().radius = pointRadius * 50;

        return point;
	}

    public Line CreateLine()
    {
        GameObject lineObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, transform);
        Line line = lineObj.GetComponent<Line>();
        //line.Awake();
        elements.Add(line);

        // Settings
        line.line.startWidth = lineWidth;
        line.colliderWidthMultiplier = colliderWidthMultiplier;

        return line;
    }

    public Circle CreateCircle(Vector2 position)
    {
        GameObject circleObj = Instantiate(circlePrefab, position, Quaternion.identity, transform);
        Circle circle = circleObj.GetComponent<Circle>();
        //circle.Awake();
        elements.Add(circle);

        // Settings
        circle.line.startWidth = lineWidth;
        circle.colliderWidthMultiplier = colliderWidthMultiplier;

        return circle;
    }

    public Angle CreateAngle()
    {
        GameObject angleObj = Instantiate(anglePrefab, Vector3.zero, Quaternion.identity, transform);
        Angle angle = angleObj.GetComponent<Angle>();
        //angle.Awake();
        elements.Add(angle);

        return angle;
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

    public Element GetElement(Vector2 position) // Done with multiple functions ensuring correct ordering of checks (point -> attachable -> angle)
    {
        Point point = GetPointAtPosition(position);
        if (point != null)
        {
            return point;
        }
        Attachable attachable = GetProminentAttachable(ref position);
        if (attachable != null)
        {
            return attachable;
        }
        RaycastHit2D[] hits = Physics2D.RaycastAll(position, Vector2.zero);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent(out Angle element))
            {
                return element;
            }
        }
        return null;
    }
}
