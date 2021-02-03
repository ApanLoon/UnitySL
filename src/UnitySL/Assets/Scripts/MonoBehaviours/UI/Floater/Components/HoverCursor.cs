using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class HoverCursor : MonoBehaviour
{
    [SerializeField] protected CursorManager.CursorType CursorTypeOnHover;

    protected CursorManager.CursorType PrevoiusType;

    public void OnPointerEnter()
    {
        PrevoiusType = CursorManager.Instance.CurrentType;
        CursorManager.Instance.SetCursorMode(CursorTypeOnHover);
    }
    public void OnPointerExit()
    {
        CursorManager.Instance.SetCursorMode(PrevoiusType);
    }
}
