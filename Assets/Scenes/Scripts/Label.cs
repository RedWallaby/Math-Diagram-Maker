using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Label : MonoBehaviour
{
    public Diagram diagram;

    public RectTransform rect;
    public TMP_Text headerText;
    public TMP_InputField field;

    private Element element;
    private bool isVisible = true;

    /// <summary>
    /// The main label update loop that checks for mouse input and updates the label's position and content
    /// </summary>
    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(1) && diagram.isEnabled)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            element = diagram.GetElement(mousePos);
            SetRect(element);
            if (!element) return;
            headerText.text = element.GetType().Name;
            field.text = element.labelOverride;
            transform.position = mousePos;
            UpdateRect();
        }
        if (Input.GetMouseButtonDown(0) && diagram.isEnabled && isVisible && diagram.clickedOnDiagram)
        {
            SetRect(false);
        }
    }

    /// <summary>
    /// Toggles the label object's visibility by setting its scale
    /// </summary>
    /// <remarks>
    /// This method is more efficient than setting the active state of the GameObject
    /// </remarks>
    public void SetRect(bool visible)
    {
        isVisible = visible;
        float scale = visible ? 1f : 0f;
        rect.localScale = new Vector3(scale, scale, 1f);
    }

    public void UpdateRect()
    {
        if (!isVisible) return;
        float size = Camera.main.orthographicSize / 5;
        rect.localScale = new Vector3(size, size, 1f);
    }

    public void ToggleElementLabel()
    {
        if (element == null) return;
        if (element.labelText == null)
        {
            CreateLabelObject(element);
        }
        element.ToggleLabel();
        SetRect(false);
    }

    public void SetElementLabelOverride(string str)
    {
        if (element == null) return;
        element.labelOverride = str;
        element.SetLabel();
    }

    public void DeleteElement()
    {
        if (element == null) return;
        element.Delete(diagram);
        if (diagram.currentEditor != null && diagram.currentEditor.GetSelectedElement() == null) // If the current editor is editing this element, reset it
        {
            diagram.currentEditor.ActivateEdit();
        }
        element = null;
        SetRect(false);
    }

    public void CreateLabelObject(Element element)
    {
        GameObject labelObj = Instantiate(diagram.labelPrefab, element.gameObject.transform);
        element.labelText = labelObj.GetComponent<TMP_Text>();
    }
}
