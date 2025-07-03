using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DiagramEditor : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    protected Diagram diagram;
    public abstract string NotificationText { get; }
    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void ActivateEdit();
    public abstract void DeactivateEdit();
    public abstract void Tick();
    public abstract Element GetSelectedElement();
}
