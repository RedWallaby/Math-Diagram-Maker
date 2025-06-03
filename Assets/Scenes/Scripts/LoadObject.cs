using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LoadObject : MonoBehaviour
{
    public JsonDiagram diagram;
    public Image image;
    public Image mainBody;
    public TMP_Text title;
    public LoadManager loadManager;

    public void SetAsLoad()
    {
        loadManager.SelectLoadObject(this);
    }

    public void DeleteLoad()
    {
        loadManager.DeleteLoadObject(this);
    }
}
