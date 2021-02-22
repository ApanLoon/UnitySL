using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Toggle))]
public class UIChatTab : MonoBehaviour, IPointerClickHandler
{
    public UIChat uiChat { get { return _uiChat != null ? _uiChat : _uiChat = GetComponentInParent<UIChat>(); } }
    private UIChat _uiChat;
    public Toggle toggle { get { return _toggle != null ? _toggle : _toggle = GetComponent<Toggle>(); } }
    private Toggle _toggle;
    public Text label;
    [NonSerialized] public readonly List<string> messageLog = new List<string>();
    [NonSerialized] public bool canClose;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canClose && eventData.button == PointerEventData.InputButton.Middle) uiChat.CloseTab(this);
    }
}
