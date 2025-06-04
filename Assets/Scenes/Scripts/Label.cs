using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Label : MonoBehaviour
{
    public RectTransform rect;
    public Diagram diagram;

    public Element element;

    public TMP_Text headerText;
    public TMP_InputField field;
    public bool isVisible = true;

    // Setting rectTransform is more computationally effecient than toggling the object, scale 0 = Toggled Off, 1 = Toggled On
    public void SetRect(bool visible)
    {
        isVisible = visible;
        float scale = visible ? 1f : 0f;
        rect.localScale = new Vector3(scale, scale, 1f);
    }

    // Update is called once per frame
    void Update()
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
        element = null;
        SetRect(false);
    }

    public void CreateLabelObject(Element element)
    {
        GameObject labelObj = Instantiate(diagram.labelPrefab, element.gameObject.transform);
        element.labelText = labelObj.GetComponent<TMP_Text>();
    }
}
