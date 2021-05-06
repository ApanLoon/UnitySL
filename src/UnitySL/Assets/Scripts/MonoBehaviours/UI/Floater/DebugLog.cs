using Assets.Scripts.MessageLogs;
using UnityEngine;
using UnityEngine.UI;

public class DebugLog : MonoBehaviour
{
    [Tooltip("Truncate the log at the next newline if it is longer than this to keep the frame-rate from exploding. 0 means do not truncate.")]
    [SerializeField] private int MaxLogLength = 10000;

    [Header("Object Bindings")]
    [SerializeField] protected Scrollbar LogHorizontalScrollbar;
    [SerializeField] protected Scrollbar LogVerticalScrollbar;

    [SerializeField] protected UIMessageLog messageView;

    protected bool FirstFrame = true;

    private void OnEnable()
    {
        LogHorizontalScrollbar.value = 0f;
        LogVerticalScrollbar.value = 0f;

        LogManager.Instance.DebugLog.OnMessage += OnDebugMessage;
    }

    private void OnDisable()
    {
        LogManager.Instance.DebugLog.OnMessage -= OnDebugMessage;
    }

    private void Update()
    {
        if (FirstFrame)
        {
            FirstFrame = false;

            foreach (LogMessage message in LogManager.Instance.DebugLog.Log)
            {
                OnDebugMessage(LogManager.Instance.DebugLog, message);
            }
        }
    }
    protected void OnDebugMessage(MessageLog log, LogMessage msg)
    {
        messageView?.AppendMessage(msg.ToRtfString());
    }
}
