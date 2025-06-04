using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Element : MonoBehaviour
{
    public TMP_Text labelText;
    public bool isLabelVisible = false;
    public string labelOverride;

    public abstract string LabelData { get; }
    public abstract Vector2 LabelPosition { get; }

    public virtual void Delete(Diagram diagram = null)
    {
        diagram?.elements.Remove(this);
        DestroyImmediate(gameObject);
    }

    public void ToggleLabel()
    {
        isLabelVisible = !isLabelVisible;
        labelText.gameObject.SetActive(isLabelVisible);
        SetLabel();
    }

    public void SetLabel()
    {
        if (labelText == null) return;
        labelText.transform.position = LabelPosition;
        if (labelOverride == null || labelOverride == "")
        {
            labelText.text = LabelData;
        }
        else
        {
            labelText.text = labelOverride;
        }
    }
}
