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

    public static event Action<LogLevel, string, string> OnLog;

    protected static void RaiseOnLog(LogLevel logLevel, string senderName, string message)
    {
        ThreadManager.ExecuteOnMainThread(() => OnLog?.Invoke(logLevel, senderName, message));
    }

    public static void LogDebug(string senderName, string message)
    {
        if (MinLogLevel > LogLevel.Debug)
        {
            return;
        }
        if (UseUnityLog)
        {
            Debug.Log($"{senderName}: {message}");
        }
        RaiseOnLog(LogLevel.Debug, senderName, message);
    }

    public static void LogInfo(string senderName, string message)
    {
        if (MinLogLevel > LogLevel.Info)
        {
            return;
        }
        if (UseUnityLog)
        {
            Debug.Log($"{senderName}: {message}");
        }
        RaiseOnLog(LogLevel.Info, senderName, message);
    }

    public static void LogWarning(string senderName, string message)
    {
        if (MinLogLevel > LogLevel.Warning)
        {
            return;
        }
        if (UseUnityLog)
        {
            Debug.LogWarning($"{senderName}: {message}");
        }
        RaiseOnLog(LogLevel.Warning, senderName, message);
    }

    public static void LogError(string senderName, string message)
    {
        if (MinLogLevel > LogLevel.Error)
        {
            return;
        }
        if (UseUnityLog)
        {
            Debug.LogError($"{senderName}: {message}");
        }
        RaiseOnLog(LogLevel.Error, senderName, message);
    }
}
