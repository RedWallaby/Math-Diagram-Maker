using TMPro;
using UnityEngine;

public class Label : MonoBehaviour
{
    public RectTransform rect;
    public Diagram diagram;

    public Element element;

    public TMP_Text headerText;

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
            headerText.text = element.GetType().Name;
            transform.position = mousePos;
        }
    }

    public void ToggleElementLabel()
    {
       if (element == null) return;


        SetRect(false);
    }

    public void DeleteElement()
    {
        if (element == null) return;
        element.Delete();
        element = null;
        SetRect(false);
    }
}
