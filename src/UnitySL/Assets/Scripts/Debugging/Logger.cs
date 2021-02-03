using System;
using UnityEngine;

public class Logger
{
    public static LogLevel MinLogLevel = LogLevel.Debug;
    public static bool UseUnityLog = true;
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        None // Must be last
    }

    public static event Action<LogLevel, string> OnLog;

    protected static void RaiseOnLog(LogLevel logLevel, string message)
    {
        ThreadManager.ExecuteOnMainThread(() => OnLog?.Invoke(logLevel, message));
    }

    public static void LogDebug(string s)
    {
        if (MinLogLevel > LogLevel.Debug)
        {
            return;
        }
        if (UseUnityLog)
        {
            Debug.Log(s);
        }
        RaiseOnLog(LogLevel.Debug, s);
    }

    public static void LogInfo(string s)
    {
        if (MinLogLevel > LogLevel.Info)
        {
            return;
        }
        if (UseUnityLog)
        {
            Debug.Log(s);
        }
        RaiseOnLog(LogLevel.Info, s);
    }

    public static void LogWarning(string s)
    {
        if (MinLogLevel > LogLevel.Warning)
        {
            return;
        }
        if (UseUnityLog)
        {
            Debug.LogWarning(s);
        }
        RaiseOnLog(LogLevel.Warning, s);
    }

    public static void LogError(string s)
    {
        if (MinLogLevel > LogLevel.Error)
        {
            return;
        }
        if (UseUnityLog)
        {
            Debug.LogError(s);
        }
        RaiseOnLog(LogLevel.Error, s);
    }
}
