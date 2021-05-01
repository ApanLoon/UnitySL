using System;
using System.Threading.Tasks;
using Assets.Scripts.MessageLogs;
using TMPro;
using UnityEngine;

public class UIChat : MonoBehaviour
{
    public UIMessageLog messageView;
    public UIChatTabTemplate tabs;
    public TMP_InputField chatInputField;

    [NonSerialized] public UIChatTab activeTab;

    protected UIChatTab localChatTab;
    protected UIChatTab debugChatTab;

    private void Start()
    {
        tabs.Initialize();
        // Create placeholder tabs
        localChatTab = CreateTab("Local", false, LogManager.Instance.ChatLog);
        debugChatTab = CreateTab("Debug", false, LogManager.Instance.DebugLog);
        //CreateTab("Quackman", true);
        //CreateTab("Bot-6542", true);
        //CreateTab("Skeleton society", true);

        localChatTab.toggle.isOn = true;

        LogManager.Instance.ChatLog.OnMessage += msg =>
        {
            string s = msg.ToRtfString();
            if (activeTab == localChatTab)
            {
                messageView.AppendMessage(s);
            }
        };

        LogManager.Instance.DebugLog.OnMessage += msg =>
        {
            string s = msg.ToRtfString();
            if (activeTab == debugChatTab)
            {
                messageView.AppendMessage(s);
            }
        };
    }

    /// <summary> Create new chat tab </summary>
    public UIChatTab CreateTab(string name, bool canClose, MessageLog log)
    {
        UIChatTab tab = tabs.InstantiateTemplate();
        tab.name = $"Tab ({name})";
        tab.label.text = name;
        tab.canClose = canClose;
        tab.toggle.onValueChanged.AddListener(isOn => { if (isOn) SelectTab(tab); });
        tab.MessageLog = log;
        return tab;
    }

    /// <summary> Close an existing chat tab </summary>
    public void CloseTab(UIChatTab uIChatTab)
    {
        tabs.ReturnItemToPool(uIChatTab);
    }

    public void SelectTab(UIChatTab tab)
    {
        activeTab = tab;
        messageView.Load(activeTab.MessageLog.AllMessagesAsRtfStrings);
        Debug.Log("Set active tab: " + activeTab.name);
    }

    public async void SendMessage()
    {
        if (activeTab == null || string.IsNullOrEmpty(chatInputField.text)) // Don't allow empty messages. TODO: The OnEndEdit event is fired when the field loses focus. We need to make out own event for catching enter, up arrow etc
        {
            return;
        }
        await (activeTab.MessageLog?.SendMessage(chatInputField.text) ?? Task.CompletedTask);
        chatInputField.text = "";

        // Return focus to the input field. We want to be able to keep typing immediately:
        chatInputField.ActivateInputField();
    }
    
    [Serializable] public class UIChatTabTemplate : Template<UIChatTab> { }
}
