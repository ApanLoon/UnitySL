using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DebugLog : MonoBehaviour
{
    public bool IsRandomLogActive = false;
    [SerializeField] protected TMP_Text LogText;
    [SerializeField] protected Scrollbar LogHorizontalScrollbar;
    [SerializeField] protected Scrollbar LogVerticalScrollbar;

    protected float Timer = 0f;
    protected float TimeToLog = 0f;

    private void Start()
    {
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
        if (autoScroll)
        {
            LogHorizontalScrollbar.value = 0f;
            LogVerticalScrollbar.value = 0f;
        }
        //LogText.ForceMeshUpdate();
    }

    protected Dictionary<Logger.LogLevel, string> LogLevelToColour = new Dictionary<Logger.LogLevel, string>()
    {
        { Logger.LogLevel.Debug,   "<#777777>" },
        { Logger.LogLevel.Info,    "<color=\"green\">" },
        { Logger.LogLevel.Warning, "<color=\"yellow\">" },
        { Logger.LogLevel.Error,   "<color=\"red\">" }
    };
}
