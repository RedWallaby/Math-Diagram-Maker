using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Diagram : MonoBehaviour
{
    public string diagramName;

    [Header("Main Elements")]
    public List<Element> elements = new();
    public GameObject pointPrefab;
    public GameObject linePrefab;
    public GameObject circlePrefab;
    public GameObject anglePrefab;

    [Header("Editor Data")]
    public GameObject editorMenu;
    public RectTransform editorR;
    public MoveSelector defaultEditor;
    private DiagramEditor currentEditor;
    private DiagramEditor storedEditor;

    [Header("Label Data")]
    public Label label;
    public RectTransform labelR;
    public GameObject labelPrefab;

    [Header("Other Data")]
    public Camera textureCamera;
    public Notification notification;
    public bool isEnabled;
    public bool clickedOnDiagram;
    private bool isMovingScreen;
    private Vector3 lastMousePosition;

    [Header("Diagram Settings")]
    [Header("Point Settings")]
    public float pointRadius = 0.3f;

    [Header("Line Settings")]
    public float lineWidth = 0.075f;
    public float colliderWidthMultiplier = 2f;

    [Header("Circle Settings")]
    public int circleAccuracy = 64;
    public float circleWidth = 0.075f;
    public float circleColliderWidthMultiplier = 2f;

    [Header("Angle Settings")]
    public float angleWidth = 0.05f;

    [Header("General Settings")]
    public float gridSize = 1f;

    /// <summary>
    /// Runs the main program loop
    /// </summary>
    /// <remarks>
    /// Resolves click data to determine if the user clicked on the diagram or not
    /// Allows for altering the camera's position and zoom level
    /// </remarks>
    public void Update()
    {
        if (!isEnabled) return;

        ResolveClickData();

        if (currentEditor != null)
        {
            currentEditor.Tick();
        }
        if (clickedOnDiagram && Input.GetKey(KeyCode.LeftAlt))
        {
            isMovingScreen = true;
            lastMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (isMovingScreen)
        {
            MoveScreen();
        }
        if (Input.mouseScrollDelta.y != 0)
        {
            Zoom();
        }
    }

    public void MoveScreen()
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

    public void Zoom()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 cameraPosition = Camera.main.transform.position;

        float originalSize = Camera.main.orthographicSize;

        float d1 = mousePosition.y - cameraPosition.y;
        float d2 = mousePosition.x - cameraPosition.x;

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - Input.mouseScrollDelta.y * 0.5f, 0.1f, 20f);

        float newSize = Camera.main.orthographicSize;
        float ratio = newSize / originalSize;

        Camera.main.transform.position = new Vector3(mousePosition.x - d2 * ratio, mousePosition.y - d1 * ratio, -10);

        label.UpdateRect();
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

    /// <summary>
    /// Determines if the mouse is within the bounds of the given rectangle defined by topLeft and bottomRight corners
    /// </summary>
    /// <param name="topLeft">The top-left-most world space position of the rectangle</param>
    /// <param name="bottomRight">The bottom-right-most world space position of the rectangle</param>
    /// <returns>If the moust is within the defined rectangle</returns>
    public bool MouseInBounds(Vector2 topLeft, Vector2 bottomRight)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return topLeft.x < mousePos.x && mousePos.x < bottomRight.x && mousePos.y < topLeft.y && bottomRight.y < mousePos.y;
    }

    /// <summary>
    /// Sets the editor for the diagram for the user to interact with the diagram elements
    /// </summary>
    /// <param name="editor">The <c>DiagramEditor</c> to set as the current editor</param>
    public void SetEditor(DiagramEditor editor)
    {
        if (!isEnabled) return; // Do not change editor if the diagram is not enabled
        if (currentEditor) // Only deactivate if there is an active editor
        {
            currentEditor.DeactivateEdit();
            notification.SetNotification(editor.NotificationText);
        }
        currentEditor = editor;
        currentEditor.ActivateEdit();
    }

    public void OpenNewDiagram()
    {
        diagramName = string.Empty;
        ResetDiagram();
        Camera.main.transform.position = new Vector3(0, 0, -10); // Reset camera position
    }

    public void ResetDiagram()
    {
        while (0 < elements.Count)
        {
            elements[0].Delete(this);
        }
    }

    /// <summary>
    /// Locks the diagram from being edited or interacted with
    /// Stores and reactivates the current editor
    /// </summary>
    public void LockEditor(bool isEnabled)
    {
        this.isEnabled = isEnabled;
        label.SetRect(false);
        if (!isEnabled)
        {
            if (!currentEditor) return;
            // Store the current editor to reactivate it on enable
            currentEditor.DeactivateEdit();
            storedEditor = currentEditor;
            currentEditor = null;
        }
        else
        {
            if (!storedEditor) return;
            currentEditor = storedEditor;
            currentEditor.ActivateEdit();
        }
    }

    /// <summary>
    /// Hides and shows the editor menu and locks the diagram from being edited or interacted with
    /// </summary>
    public void SetActive(bool isActive)
    {
        isEnabled = isActive;
        editorMenu.SetActive(isActive);
        if (isActive)
        {
            SetEditor(defaultEditor);
            CentraliseCamera();
        }
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
        circle.accuracy = circleAccuracy;
        circle.line.startWidth = circleWidth;
        circle.colliderWidthMultiplier = circleColliderWidthMultiplier;

        return circle;
    }

    public Angle CreateAngle()
    {
        GameObject angleObj = Instantiate(anglePrefab, Vector3.zero, Quaternion.identity, transform);
        Angle angle = angleObj.GetComponent<Angle>();

        // Settings
        angle.line.startWidth = angleWidth;

        return angle;
    }

    /// <summary>
    /// Gets the first point at the given position, excluding points that match the exclusion criteria
    /// </summary>
    /// <param name="position">The world space postion to check</param>
    /// <param name="exclusion">Exclusion criteria to remove certain points from the check</param>
    /// <returns>The <c>Point</c> at the given position</returns>
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

    /// <summary>
    /// Gives the first attachable at the given position
    /// </summary>
    /// <param name="position">The world space position to check</param>
    /// <returns>The <c>Attachable</c> at the given position</returns>
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

    /// <summary>
    /// Gets the most prominent feature at the given position (point > attachable)
    /// </summary>
    /// <param name="position">The world space position to check</param>
    /// <param name="point">Returns the <c>Point</c> at the given position</param>
    /// <param name="attachable">Returns the <c>Attachable</c> at the given position</param>
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

    /// <summary>
    /// Gets the highest priority element at the given position (point > attachable > angle)
    /// </summary>
    /// <param name="position">The world space position to check</param>
    /// <returns>The <c>Element</c> at the given position</returns>
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

    public void LockPositionToGrid(ref Vector2 position)
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            position.x = Mathf.Round(position.x / gridSize) * gridSize;
            position.y = Mathf.Round(position.y / gridSize) * gridSize;
        }
    }

    /// <summary>
    /// Sets the main camera to the average position of all elements in the diagram
    /// </summary>
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

    /// <summary>
    /// Sets the bounds of the texture camera based on the furthest positions of all elements in the diagram
    /// </summary>
    /// <remarks>
    /// Camera size is clamped to a minimum of 5 and a maximum of 25 and is set to cover all elements with a 10% padding
    /// </remarks>
    public void SetBoundsOnTextureCamera()
    {
        if (elements.Count == 0) return;
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
        textureCamera.transform.position = new Vector3((leftBound + rightBound) / 2, (topBound + bottomBound) / 2, textureCamera.transform.position.z);
        float width = rightBound - leftBound;
        float height = topBound - bottomBound;
        textureCamera.orthographicSize = Mathf.Clamp(Mathf.Max(width / 2, height / 2) * 1.1f, 5, 25);
    }
}
