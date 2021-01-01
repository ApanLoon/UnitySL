using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DebugLog : MonoBehaviour
{
    public bool IsRandomLogActive = false;
    [Tooltip("Truncate the log at the next newline if it is longer than this to keep the framerate from exploding. 0 means do not truncate.")]
    [SerializeField] private int MaxLogLength = 10000;

    [Header("Colour Settings")]
    [SerializeField] protected Color DebugColour   = new Color(0.467f, 0.467f, 0.467f, 1.0f);
    [SerializeField] protected Color InfoColour    = Color.green;
    [SerializeField] protected Color WarningColour = Color.yellow;
    [SerializeField] protected Color ErrorColour   = Color.red;

    [Header("Object Bindings")]
    [SerializeField] protected TMP_Text LogText;
    [SerializeField] protected Scrollbar LogHorizontalScrollbar;
    [SerializeField] protected Scrollbar LogVerticalScrollbar;
    
    protected float Timer = 0f;
    protected float TimeToLog = 0f;

    protected Dictionary<Logger.LogLevel, string> LogLevelToColour = new Dictionary<Logger.LogLevel, string>();
    
    private void Start()
    {
        LogLevelToColour[Logger.LogLevel.Debug]   = DebugColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Info]    = InfoColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Warning] = WarningColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Error]   = ErrorColour.ToRtfString();

        LogText.text = ""; // Remove the Lorem ipsum
        LogHorizontalScrollbar.value = 0f;
        LogVerticalScrollbar.value = 0f;

        Logger.OnLog += LogMessage;
    }

    private void Update()
    {
        Timer += Time.deltaTime;
        RandomLog();
    }

    protected void RandomLog()
    {
        if (IsRandomLogActive == false)
        {
            return;
        }

        if (TimeToLog < Timer)
        {
            LogMessage(Logger.LogLevel.Debug, $"Random log message.");
            TimeToLog = Timer + Random.Range(0f, 1f);
        }
    }

    protected void LogMessage(Logger.LogLevel logLevel, string message)
    {
        bool autoScroll = Mathf.Abs(LogVerticalScrollbar.value) < 0.1f;
        LogText.text += $"{LogLevelToColour[logLevel]}{DateTime.Now:T} {logLevel} {message}\n";

        //TODO: Ugly truncation:
        if (MaxLogLength > 0 && LogText.text.Length > MaxLogLength)
        {
            int start = LogText.text.Length - MaxLogLength;
            while (start < LogText.text.Length && LogText.text[start] != '\n')
            {
                start++;
            }
            LogText.text = LogText.text.Substring(start);
        }

        if (autoScroll)
        {
            LogHorizontalScrollbar.value = 0f;
            LogVerticalScrollbar.value = 0f;
        }
    }

}
