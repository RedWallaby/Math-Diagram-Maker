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
    public abstract void Delete(Diagram diagram = null);

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

    /// <summary>
    /// Updates the <c>Element</c> based on the new position of the point
    /// </summary>
    /// <param name="point">The <c>Point</c> that will move</param>
    /// <param name="placingPosition">The new position for the <c>Point<c/></param>
    public virtual void UpdatePointPosition(Point point, Vector2 placingPosition)
    {
        // This method can be overridden by non-point derived classes to update the position of points attached to this element
    }
}
