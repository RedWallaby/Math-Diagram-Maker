using UnityEngine;
using UnityEngine.EventSystems;

// MAYBE MAKE ABSTRACT
public abstract class DiagramEditor : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    protected Diagram diagram;
    public abstract string NotificationText { get; }
    public abstract void OnPointerClick(PointerEventData eventData);
    public abstract void ActivateEdit();
    public abstract void DeactivateEdit();
    public abstract void Tick();
}
