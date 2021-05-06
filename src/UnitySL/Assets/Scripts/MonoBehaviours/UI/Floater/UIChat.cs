using System;
using System.Collections.Generic;
using System.Linq;
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

    protected Dictionary<MessageLog, UIChatTab> TabByLog = new Dictionary<MessageLog, UIChatTab>();

    private void OnEnable()
    {
        tabs.Initialize();
        localChatTab = CreateTab("Local", false, LogManager.Instance.ChatLog);
        debugChatTab = CreateTab("Debug", false, LogManager.Instance.DebugLog);

        foreach (MessageLog log in LogManager.Instance.InstantMessageLogs.Values)
        {
            CreateTab(log.Name, true, log);
        }

        LogManager.Instance.OnNewInstantMessageSession += OnNewInstantMessageSession;

        localChatTab.toggle.isOn = true;
    }

    private void OnDisable()
    {
        LogManager.Instance.OnNewInstantMessageSession -= OnNewInstantMessageSession;
        foreach (UIChatTab tab in TabByLog.Values.ToArray()) // Copy to array to avoid errors due to modifying the dictionary
        {
            tab.MessageLog.OnMessage -= OnMessage;
            CloseTab(tab);
        }
    }

    protected void OnNewInstantMessageSession(Guid dialogId, string senderName, MessageLog log)
    {
        localChatTab = CreateTab(senderName, true, log);
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

        TabByLog[log] = tab;
        log.OnMessage += OnMessage;

        return tab;
    }

    protected void OnMessage(MessageLog log, LogMessage msg)
    {
        if (TabByLog.ContainsKey(log) == false)
        {
            // If the tab doesn't exist we have to create it. (This happens if a message comes in for a tab that has been closed)
            log.OnMessage -= OnMessage; // We are still listening, so remove the listener to avoid multiple registrations
            CreateTab(msg.SenderName, true, log);
        }

        UIChatTab tab = TabByLog[log];
        string s = msg.ToRtfString();
        if (activeTab == tab)
        {
            messageView.AppendMessage(s);
        }
    }
    /// <summary> Close an existing chat tab </summary>
    public void CloseTab(UIChatTab uIChatTab)
    {
        TabByLog.Remove(uIChatTab.MessageLog);
        tabs.ReturnItemToPool(uIChatTab);
    }

    public void SelectTab(UIChatTab tab)
    {
        activeTab = tab;
        messageView.Load(activeTab.MessageLog.AllMessagesAsRtfStrings);
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
