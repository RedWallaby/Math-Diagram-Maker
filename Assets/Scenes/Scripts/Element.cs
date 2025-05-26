using TMPro;
using UnityEngine;

public abstract class Element : MonoBehaviour
{
    protected TMP_Text labelText;
    protected bool isLabelVisible = false;

    public void Delete()
    {
        // Remove this element from the scene
        Destroy(gameObject);
    }

    public abstract void ToggleLabel();
}
