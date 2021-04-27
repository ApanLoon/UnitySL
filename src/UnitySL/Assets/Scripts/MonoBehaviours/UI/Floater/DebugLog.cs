using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DebugLog : MonoBehaviour
{
    [Tooltip("Truncate the log at the next newline if it is longer than this to keep the framerate from exploding. 0 means do not truncate.")]
    [SerializeField] private int MaxLogLength = 10000;

    [Header("Colour Settings")]
    [SerializeField] protected Color DebugColour = new Color(0.467f, 0.467f, 0.467f, 1.0f);
    [SerializeField] protected Color InfoColour = Color.green;
    [SerializeField] protected Color WarningColour = Color.yellow;
    [SerializeField] protected Color ErrorColour = Color.red;

    [Header("Object Bindings")]
    [SerializeField] protected Transform LogTextContainer;
    [SerializeField] protected Scrollbar LogHorizontalScrollbar;
    [SerializeField] protected Scrollbar LogVerticalScrollbar;

    [SerializeField] protected UIMessageLog messageView;

    protected Dictionary<Logger.LogLevel, string> LogLevelToColour = new Dictionary<Logger.LogLevel, string>();
    protected List<GameObject> LogTextObjects = new List<GameObject>();

    private void Start()
    {
        LogLevelToColour[Logger.LogLevel.Debug] = DebugColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Info] = InfoColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Warning] = WarningColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Error] = ErrorColour.ToRtfString();

        //AppdendLogTextObject();

        LogHorizontalScrollbar.value = 0f;
        LogVerticalScrollbar.value = 0f;

        Logger.OnLog += LogMessage;
    }

    protected void LogMessage(Logger.LogLevel logLevel, string senderName, string message)
    {
        message = $"{LogLevelToColour[logLevel]}{DateTime.Now:T} {logLevel} {senderName}: {message}</color>";
        messageView.AppendMessage(message);

    }
}
