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
    [SerializeField] protected Transform LogTextContainer;
    [SerializeField] protected Scrollbar LogHorizontalScrollbar;
    [SerializeField] protected Scrollbar LogVerticalScrollbar;

    [SerializeField] protected GameObject LogTextPrefab;

    protected float Timer = 0f;
    protected float TimeToLog = 0f;

    protected Dictionary<Logger.LogLevel, string> LogLevelToColour = new Dictionary<Logger.LogLevel, string>();
    protected List<GameObject> LogTextObjects = new List<GameObject>();
    protected TMP_Text CurrentLogText;
    
    private void Start()
    {
        LogLevelToColour[Logger.LogLevel.Debug]   = DebugColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Info]    = InfoColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Warning] = WarningColour.ToRtfString();
        LogLevelToColour[Logger.LogLevel.Error]   = ErrorColour.ToRtfString();

        // Remove the Lorem ipsum:
        foreach (Transform child in LogTextContainer)
        {
            Destroy(child.gameObject);
        }

        AppdendLogTextObject();

        LogHorizontalScrollbar.value = 0f;
        LogVerticalScrollbar.value = 0f;

        Logger.OnLog += LogMessage;
    }

    protected void AppdendLogTextObject()
    {
        GameObject go = Instantiate(LogTextPrefab, LogTextContainer);
        LogTextObjects.Add(go);
        CurrentLogText = go.GetComponent<TMP_Text>();
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
        if (CurrentLogText.text.Length > MaxLogLength)
        {
            AppdendLogTextObject();
        }

        CurrentLogText.text += $"{LogLevelToColour[logLevel]}{DateTime.Now:T} {logLevel} {message}\n";

        if (autoScroll)
        {
            LogHorizontalScrollbar.value = 0f;
            LogVerticalScrollbar.value = 0f;
        }
    }

}
