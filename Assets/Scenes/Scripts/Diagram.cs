using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Diagram : MonoBehaviour
{
    public string diagramName;

    public List<Element> elements = new();
    public GameObject pointPrefab;
    public GameObject linePrefab;
    public GameObject circlePrefab;
    public GameObject anglePrefab;

    public GameObject editorMenu;
    public DiagramEditor currentEditor;
    public DiagramEditor storedEditor;
    public MoveSelector defaultEditor;

    public RectTransform labelR;
    public RectTransform editorR;

    public GameObject labelPrefab;

    public bool isEnabled;

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
        if (!isEnabled) return;

        ResolveClickData();

        if (clickedOnDiagram && Input.GetKey(KeyCode.LeftAlt))
        {
            isMovingScreen = true;
            lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (isMovingScreen)
        {
            clickedOnDiagram = false;
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
        if (!Input.GetMouseButtonDown(0) || isMovingScreen)
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

        // Settings
        RectTransform rt = pointObj.GetComponent<RectTransform>();
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, pointRadius * 200);
        rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, pointRadius * 200);
        point.GetComponent<CircleCollider2D>().radius = pointRadius * 100;

        return point;
	}

    public Line CreateLine()
    {
        GameObject lineObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity, transform);
        Line line = lineObj.GetComponent<Line>();

        // Settings
        line.line.startWidth = lineWidth;
        line.colliderWidthMultiplier = colliderWidthMultiplier;

        return line;
    }

    public Circle CreateCircle(Vector2 position)
    {
        GameObject circleObj = Instantiate(circlePrefab, position, Quaternion.identity, transform);
        Circle circle = circleObj.GetComponent<Circle>();
        circle.line.positionCount = circle.accuracy;

        // Settings
        circle.line.startWidth = lineWidth;
        circle.colliderWidthMultiplier = colliderWidthMultiplier;

        return circle;
    }

    public Angle CreateAngle()
    {
        GameObject angleObj = Instantiate(anglePrefab, Vector3.zero, Quaternion.identity, transform);
        Angle angle = angleObj.GetComponent<Angle>();

        return angle;
    }

    public void SetEditor(DiagramEditor editor)
    {
        if (!isEnabled) return; // Do not change editor if the diagram is not enabled
        if (currentEditor == editor) return; // Prevent re-assigning the same editor
        if (currentEditor != null) // Only deactivate if there is an active editor
        {
            currentEditor.DeactivateEdit();
        }
        currentEditor = editor;
        if (editor == null) return; // If the editor is null, do not activate it
        currentEditor.ActivateEdit();
    }

    public void CreateNewDiagram()
    {
        diagramName = "New Diagram";
        ClearDiagram();
    }

    public void ClearDiagram()
    {
        while (0 < elements.Count)
        {
            elements[0].Delete(this);
        }
    }

    // TODO CHANGE NAME TO LOCK EDITOR
    public void SetEnabled(bool isEnabled) // Sets the enabled state of the diagram's functionality
    {
        this.isEnabled = isEnabled;
        if (!currentEditor) return;
        if (!isEnabled)
        {
            // Store the current editor to reactivate it on enable
            currentEditor.DeactivateEdit();
            storedEditor = currentEditor;
            currentEditor = null;
        }
        else
        {
            currentEditor = storedEditor;
            currentEditor.ActivateEdit();
        }
    }

    // TODO CHANGE NAME TO FULL ENABLE
    public void SetActive(bool isActive) // Sets the active state of the diagram object
    {
        isEnabled = isActive;
        editorMenu.SetActive(isActive);
        if (isActive)
        {
            SetEditor(defaultEditor);
            CentraliseCamera();
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

    public void CentraliseCamera()
    {
        Camera cam = Camera.main;
        if (elements.Count == 0) return; // No elements to centralise camera on
        float xSum = 0f;
        float ySum = 0f;
        foreach (Element element in elements)
        {
            xSum += element.transform.position.x;
            ySum += element.transform.position.y;
        }
        Vector2 centralPosition = new(xSum / elements.Count, ySum / elements.Count);
        cam.transform.position = new Vector3(centralPosition.x, centralPosition.y, cam.transform.position.z);
        cam.orthographicSize = 5;
    }

    public void SetBoundsOnCamera(Camera camera)
    {
        if (elements.Count == 0) return; // No elements to set bounds for
        Vector2 centralObject = elements[0].transform.position;
        float leftBound = centralObject.x;
        float rightBound = centralObject.x;
        float topBound = centralObject.y;
        float bottomBound = centralObject.y;
        foreach (Element element in elements)
        {
            if (element is Point point)
            {
                leftBound = Mathf.Min(leftBound, point.position.x - pointRadius);
                rightBound = Mathf.Max(rightBound, point.position.x + pointRadius);
                topBound = Mathf.Max(topBound, point.position.y + pointRadius);
                bottomBound = Mathf.Min(bottomBound, point.position.y - pointRadius);
            }
            else if (element is Circle circle)
            {
                float radius = circle.radius + lineWidth;
                leftBound = Mathf.Min(leftBound, circle.centre.position.x - radius);
                rightBound = Mathf.Max(rightBound, circle.centre.position.x + radius);
                topBound = Mathf.Max(topBound, circle.centre.position.y + radius);
                bottomBound = Mathf.Min(bottomBound, circle.centre.position.y - radius);
            }
        }
        camera.transform.position = new Vector3((leftBound + rightBound) / 2, (topBound + bottomBound) / 2, camera.transform.position.z);
        float width = rightBound - leftBound;
        float height = topBound - bottomBound;
        camera.orthographicSize = Mathf.Max(width / 2, height / 2) * 1.1f; // Set orthographic size based on the larger dimension (multiplied by 1.1 to add padding)
    }
}
