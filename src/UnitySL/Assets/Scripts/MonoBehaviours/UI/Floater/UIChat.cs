using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.MessageLogs;
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

    protected UIChatTab localChatTab;
    protected UIChatTab debugChatTab;

    private void Start()
    {
        tabs.Initialize();
        // Create placeholder tabs
        localChatTab = CreateTab("Local", false);
        debugChatTab = CreateTab("debug", false);
        //CreateTab("Quackman", true);
        //CreateTab("Bot-6542", true);
        //CreateTab("Skeleton society", true);

        localChatTab.toggle.isOn = true;

        LogManager.Instance.ChatLog.OnMessage += msg =>
        {
            string s = msg.ToRtfString();
            localChatTab.messageLog.Add(s);
            if (activeTab == localChatTab)
            {
                messageView.AppendMessage(s);
            }
        };

        LogManager.Instance.DebugLog.OnMessage += msg =>
        {
            string s = msg.ToRtfString();
            debugChatTab.messageLog.Add(s);
            if (activeTab == debugChatTab)
            {
                messageView.AppendMessage(s);
            }
        };
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

    /// <summary> Close an existing chat tab </summary>
    public void CloseTab(UIChatTab uIChatTab)
    {
        tabs.ReturnItemToPool(uIChatTab);
    }

    public void SelectTab(UIChatTab tab)
    {
        activeTab = tab;
        messageView.Load(activeTab.messageLog);
        Debug.Log("Set active tab: " + activeTab.name);
    }

    public async void SendMessage()
    {
        if (activeTab == localChatTab)
        {
            string s = chatInputField.text;
            await (Agent.CurrentPlayer?.Region?.Circuit?.SendChatFromViewer(s, ChatType.Normal, 0) ?? Task.CompletedTask);
        }
        //activeTab.messageLog.Add(chatInputField.text);
        //messageView.AppendMessage(chatInputField.text);
        chatInputField.text = "";
    }

    public class Message
    {
        public string sender;
        public string text;
    }

    [Serializable] public class UIChatTabTemplate : Template<UIChatTab> { }
}
