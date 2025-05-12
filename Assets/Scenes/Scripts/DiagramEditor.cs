using UnityEngine;
using UnityEngine.EventSystems;

// MAYBE MAKE ABSTRACT
public class DiagramEditor : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    protected Diagram diagram;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        // This method can be overridden in derived classes
    }

    public virtual void ActivateEdit()
    {
        // This method can be overridden in derived classes
    }

    public virtual void DeactivateEdit()
    {
        // This method can be overridden in derived classes
    }

    /*public virtual void Update()
    {
        //Use this for detection of mouse cursor being in correct area
    }*/
}
