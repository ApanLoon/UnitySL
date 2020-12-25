using System;
using UnityEngine;

public class Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public static event Action<LogLevel, string> OnLog;

    public static void LogDebug(string s)
    {
        Debug.Log(s);
        OnLog?.Invoke(LogLevel.Debug, $"{s}\n");
    }

    public static void LogInfo(string s)
    {
        Debug.Log(s);
        OnLog?.Invoke(LogLevel.Info, $"{s}\n");
    }

    public static void LogWarning(string s)
    {
        Debug.LogWarning(s);
        OnLog?.Invoke(LogLevel.Warning, $"{s}\n");
    }

    public static void LogError(string s)
    {
        Debug.LogError(s);
        OnLog?.Invoke(LogLevel.Error, $"{s}\n");
    }
}
