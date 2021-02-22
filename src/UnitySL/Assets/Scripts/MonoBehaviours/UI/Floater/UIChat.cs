using System;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIChat : MonoBehaviour
{
    public UIMessageLog messageView;
    public UIChatTabTemplate tabs;
    public TMP_InputField chatInputField;

    [NonSerialized] public UIChatTab activeTab;

    private void Start()
    {
        tabs.Initialize();
        // Create placeholder tabs
        CreateTab("Local", false).toggle.isOn = true;
        CreateTab("Quackman", true);
        CreateTab("Bot-6542", true);
        CreateTab("Skeleton society", true);
    }

    /// <summary> Create new chat tab </summary>
    public UIChatTab CreateTab(string name, bool canClose)
    {
        UIChatTab tab = tabs.InstantiateTemplate();
        tab.name = $"Tab ({name})";
        tab.label.text = name;
        tab.canClose = canClose;
        tab.toggle.onValueChanged.AddListener(isOn => { if (isOn) SelectTab(tab); });
        return tab;
    }

    public void SelectTab(UIChatTab tab)
    {
        activeTab = tab;
        messageView.Load(activeTab.messageLog);
        Debug.Log("Set active tab: " + activeTab.name);
    }

    public void SendMessage()
    {
        activeTab.messageLog.Add(chatInputField.text);
        messageView.AppendMessage(chatInputField.text);
        chatInputField.text = "";
    }

    public class Message
    {
        public string sender;
        public string text;
    }

    [Serializable] public class UIChatTabTemplate : Template<UIChatTab> { }
}
