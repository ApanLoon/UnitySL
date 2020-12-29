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

    protected static void RaiseOnLog(LogLevel logLevel, string message)
    {
        ThreadManager.ExecuteOnMainThread(() => OnLog?.Invoke(logLevel, message));
    }

    public static void LogDebug(string s)
    {
        Debug.Log(s);
        RaiseOnLog(LogLevel.Debug, $"{s}\n");
    }

    public static void LogInfo(string s)
    {
        Debug.Log(s);
        RaiseOnLog(LogLevel.Info, $"{s}\n");
    }

    public static void LogWarning(string s)
    {
        Debug.LogWarning(s);
        RaiseOnLog(LogLevel.Warning, $"{s}\n");
    }

    public static void LogError(string s)
    {
        Debug.LogError(s);
        RaiseOnLog(LogLevel.Error, $"{s}\n");
    }
}
