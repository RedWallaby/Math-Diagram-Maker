using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public Diagram diagram;
    public TMP_Text notificationText;

    void Update()
    {
        if (!diagram.isEnabled) return;
        
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            HideNotification();
        }
    }

    /// <summary>
    /// Sets the notification text and displays it for a specified duration
    /// </summary>
    /// <param name="text">The notifcation text to display</param>
    /// <param name="duration">The duration of the notification, defaults to unlimited duration (-1)</param>
    public void SetNotification(string text, float duration = -1)
    {
        notificationText.text = text;
        gameObject.SetActive(true);

        if (duration > 0)
        {
            Invoke(nameof(HideNotification), duration);
        }
    }

    public void HideNotification()
    {
        gameObject.SetActive(false);
    }
}
