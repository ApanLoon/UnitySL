using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"Multiple instances of EventManager in scene, disabling the one on {name}.");
            enabled = false;
            return;
        }

        Instance = this;
    }

    #region Agent

    public event Action<Agent> OnHealthChanged;
    public void RaiseOnHealthChanged(Agent agent)
    {
        ThreadManager.ExecuteOnMainThread(() => OnHealthChanged?.Invoke(agent));
    }

    public event Action<Agent> OnAgentDataChanged;
    public void RaiseOnAgentDataChanged(Agent agent)
    {
        ThreadManager.ExecuteOnMainThread(() => OnAgentDataChanged?.Invoke(agent));
    }

    public event Action<Agent> OnAgentMoved;
    public void RaiseOnAgentMoved(Agent agent)
    {
        ThreadManager.ExecuteOnMainThread(() => OnAgentMoved?.Invoke(agent));
    }

    #region Messages
    public event Action<HealthMessage> OnHealthMessage;
    public event Action<ScriptControlChangeMessage> OnScriptControlChangeMessage;
    public event Action<AgentMovementCompleteMessage> OnAgentMovementCompleteMessage;
    public event Action<OnlineNotificationMessage> OnOnlineNotificationMessage;
    public event Action<OfflineNotificationMessage> OnOfflineNotificationMessage;
    public event Action<AgentDataUpdateMessage> OnAgentDataUpdateMessage;
    #endregion Messages
    #endregion Agent

    #region Region
    public event Action<Region> OnRegionDataChanged;

    public void RaiseOnRegionDataChanged(Region region)
    {
        ThreadManager.ExecuteOnMainThread(() => OnRegionDataChanged?.Invoke(region));
    }

    public event Action<RegionHandshakeMessage> OnRegionHandshakeMessage;
    public event Action<SimulatorViewerTimeMessage> OnSimulatorViewerTimeMessage;
    #endregion Region

    #region Chat
    public event Action<ChatFromSimulatorMessage> OnChatFromSimulatorMessage;
    #endregion Chat

    #region Map
    public event Action<CoarseLocationUpdateMessage> OnCoarseLocationUpdateMessage;
    #endregion Map

    #region Audio
    public event Action<SoundTriggerMessage> OnSoundTriggerMessage;
    public event Action<AttachedSoundMessage> OnAttachedSoundMessage;
    public event Action<PreloadSoundMessage> OnPreloadSoundMessage;
    #endregion Audio

    #region ViewerEffect
    public event Action<ViewerEffectMessage> OnViewerEffectMessage;
    #endregion ViewerEffect

    #region Progress
    public event Action<string, string, float, bool, float> OnProgressUpdate;
    public void RaiseOnProgressUpdate (string title, string message, float progress, bool close = false, float maxProgress = 1.0f)
    {
        ThreadManager.ExecuteOnMainThread(() => OnProgressUpdate?.Invoke(title, message, progress, close, maxProgress));
    }
    #endregion Progress

    protected static Dictionary<MessageId, Action<Message>> HandlerByMessageId = new Dictionary<MessageId, Action<Message>>()
    {
        {MessageId.SoundTrigger,                 (m) => Instance.OnSoundTriggerMessage?.Invoke          ((SoundTriggerMessage)m)  },

        {MessageId.CoarseLocationUpdate,         (m) => Instance.OnCoarseLocationUpdateMessage?.Invoke  ((CoarseLocationUpdateMessage)m)  },
        {MessageId.AttachedSound,                (m) => Instance.OnAttachedSoundMessage?.Invoke         ((AttachedSoundMessage)m)         },
        {MessageId.PreloadSound,                 (m) => Instance.OnPreloadSoundMessage?.Invoke          ((PreloadSoundMessage)m)          },

        {MessageId.ViewerEffect,                 (m) => Instance.OnViewerEffectMessage?.Invoke          ((ViewerEffectMessage)m)          },
        {MessageId.HealthMessage,                (m) => Instance.OnHealthMessage?.Invoke                ((HealthMessage)m)                },
        {MessageId.ChatFromSimulator,            (m) => Instance.OnChatFromSimulatorMessage?.Invoke     ((ChatFromSimulatorMessage)m)     },
        {MessageId.RegionHandshake,              (m) => Instance.OnRegionHandshakeMessage?.Invoke       ((RegionHandshakeMessage)m)       },
        {MessageId.SimulatorViewerTimeMessage,   (m) => Instance.OnSimulatorViewerTimeMessage?.Invoke   ((SimulatorViewerTimeMessage)m)   },
        {MessageId.ScriptControlChange,          (m) => Instance.OnScriptControlChangeMessage?.Invoke   ((ScriptControlChangeMessage)m)   },
        {MessageId.AgentMovementCompleteMessage, (m) => Instance.OnAgentMovementCompleteMessage?.Invoke ((AgentMovementCompleteMessage)m) },
        {MessageId.OnlineNotification,           (m) => Instance.OnOnlineNotificationMessage?.Invoke    ((OnlineNotificationMessage)m)    },
        {MessageId.OfflineNotification,          (m) => Instance.OnOfflineNotificationMessage?.Invoke   ((OfflineNotificationMessage)m)   },
        {MessageId.AgentDataUpdate,              (m) => Instance.OnAgentDataUpdateMessage?.Invoke       ((AgentDataUpdateMessage)m)       },
    };

    public void RaiseOnMessage(Message message)
    {
        if (HandlerByMessageId.ContainsKey(message.Id) == false)
        {
            return;
        }

        ThreadManager.ExecuteOnMainThread(() => HandlerByMessageId[message.Id](message));
    }
}
