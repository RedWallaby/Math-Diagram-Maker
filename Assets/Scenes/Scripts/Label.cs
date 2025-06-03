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

    // Setting rectTransform is more computationally effecient than toggling the object, scale 0 = Toggled Off, 1 = Toggled On
    public void SetRect(bool visible)
    {
        float scale = visible ? 1f : 0f;
        rect.localScale = new Vector3(scale, scale, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            element = diagram.GetElement(mousePos);
            SetRect(element);
            if (!element) return;
            headerText.text = element.GetType().Name;
            field.text = element.labelOverride;
            transform.position = mousePos;
        }
    }

    public void ToggleElementLabel()
    {
        if (element == null) return;
        if (element.labelText == null)
        {
            CreateLabelObject();
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

    private void CreateLabelObject()
    {
        GameObject labelObj = Instantiate(diagram.labelPrefab, element.gameObject.transform);
        element.labelText = labelObj.GetComponent<TMP_Text>();
    }
}
