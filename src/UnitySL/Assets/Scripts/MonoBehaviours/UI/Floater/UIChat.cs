using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UIChat : MonoBehaviour
{
    public UIMessageLog messageView;
    public UIChatTabTemplate tabs;

    private void Start()
    {
        tabs.Initialize();
    }

    protected void LogMessage(Logger.LogLevel logLevel, string message)
    {
        //message = $"{LogLevelToColour[logLevel]}{DateTime.Now:T} {logLevel} {message}</color>";
        messageView.AppendMessage(message);

    }

    public class Message
    {
        public string sender;
        public string text;
    }

    [Serializable] public class UIChatTabTemplate : Template<UIChatTab> { }
}
