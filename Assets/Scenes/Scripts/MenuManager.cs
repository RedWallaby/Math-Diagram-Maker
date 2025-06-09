using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static void CloseMenu(GameObject menuObj)
    {
        menuObj.SetActive(false);
    }

    public static void OpenMenu(GameObject menuObj)
    {
        menuObj.SetActive(true);
    }

    public static void ToggleWindow(GameObject windowObj)
    {
        windowObj.SetActive(!windowObj.activeSelf);
    }
}
