using System;
using Assets.Scripts.MessageLogs;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Toggle))]
public class UIChatTab : MonoBehaviour, IPointerClickHandler
{
    public UIChat uiChat { get; protected set; }
    public Toggle toggle { get; protected set; }

    public Text label;

    [NonSerialized] public MessageLog MessageLog;
    [NonSerialized] public bool canClose;

    private void OnEnable()
    {
        uiChat = GetComponentInParent<UIChat>();
        toggle = GetComponent<Toggle>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canClose && eventData.button == PointerEventData.InputButton.Middle) uiChat.CloseTab(this);
    }
}
