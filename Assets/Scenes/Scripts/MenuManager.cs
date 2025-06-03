using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public void CloseMenu(GameObject menuObj)
    {
        menuObj.SetActive(false);
    }

    public void OpenMenu(GameObject menuObj)
    {
        menuObj.SetActive(true);
    }

    public void ToggleWindow(GameObject windowObj)
    {
        windowObj.SetActive(!windowObj.activeSelf);
    }
}
