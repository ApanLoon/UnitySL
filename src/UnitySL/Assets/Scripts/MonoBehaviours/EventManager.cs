using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Agent;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Audio;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Map;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Objects;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Region;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Viewer;
using Assets.Scripts.Regions;
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

    public event Action<Region, float[], UInt32, float, float> OnHeightsDecoded;
    public void RaiseOnHeightsDecoded(Region region, float[] heights, UInt32 size, float minHeight, float maxHeight)
    {
        ThreadManager.ExecuteOnMainThread(() => OnHeightsDecoded?.Invoke(region, heights, size, minHeight, maxHeight));
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

    #region Object
    public event Action<ObjectUpdateMessage> OnObjectUpdateMessage;
    #endregion Object

    #region Region
    public event Action<Region> OnParcelOverlayChanged;
    public void RaiseOnParcelOverlayChanged(Region region)
    {
        ThreadManager.ExecuteOnMainThread(() => OnParcelOverlayChanged?.Invoke(region));
    }

    public event Action<Region> OnRegionDataChanged;
    public void RaiseOnRegionDataChanged(Region region)
    {
        ThreadManager.ExecuteOnMainThread(() => OnRegionDataChanged?.Invoke(region));
    }

    public event Action<Region> OnRegionRemoved;

    public void RaiseOnRegionRemoved(Region region)
    {
        ThreadManager.ExecuteOnMainThread(() => OnRegionRemoved?.Invoke(region));
    }

    public event Action<LayerDataMessage>           OnLayerDataMessage;
    public event Action<ParcelOverlayMessage>       OnParcelOverlayMessage;
    public event Action<RegionHandshakeMessage>     OnRegionHandshakeMessage;
    public event Action<SimulatorViewerTimeMessage> OnSimulatorViewerTimeMessage;
    #endregion Region

    #region Chat
    public event Action<ChatFromSimulatorMessage> OnChatFromSimulatorMessage;
    public event Action<ImprovedInstantMessageMessage> OnImprovedInstantMessageMessage;
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

    #region Messages
    public event Action<LogoutReplyMessage> OnLogoutReplyMessage;

    protected static Dictionary<MessageId, Action<Message>> HandlerByMessageId = new Dictionary<MessageId, Action<Message>>()
    {
        {MessageId.ObjectUpdate,                 (m) => Instance.OnObjectUpdateMessage?.Invoke           ((ObjectUpdateMessage)m)            },
        //{MessageId.ObjectUpdateCompressed,       (m) => Instance.OnObjectUpdateMessage?.Invoke           ((ObjectUpdateMessage)m)            },
        {MessageId.SoundTrigger,                 (m) => Instance.OnSoundTriggerMessage?.Invoke           ((SoundTriggerMessage)m)            },

        {MessageId.CoarseLocationUpdate,         (m) => Instance.OnCoarseLocationUpdateMessage?.Invoke   ((CoarseLocationUpdateMessage)m)    },
        {MessageId.LayerData,                    (m) => Instance.OnLayerDataMessage?.Invoke              ((LayerDataMessage)m)               },
        {MessageId.AttachedSound,                (m) => Instance.OnAttachedSoundMessage?.Invoke          ((AttachedSoundMessage)m)           },
        {MessageId.PreloadSound,                 (m) => Instance.OnPreloadSoundMessage?.Invoke           ((PreloadSoundMessage)m)            },

        {MessageId.ViewerEffect,                 (m) => Instance.OnViewerEffectMessage?.Invoke           ((ViewerEffectMessage)m)            },
        {MessageId.HealthMessage,                (m) => Instance.OnHealthMessage?.Invoke                 ((HealthMessage)m)                  },
        {MessageId.ChatFromSimulator,            (m) => Instance.OnChatFromSimulatorMessage?.Invoke      ((ChatFromSimulatorMessage)m)       },
        {MessageId.RegionHandshake,              (m) => Instance.OnRegionHandshakeMessage?.Invoke        ((RegionHandshakeMessage)m)         },
        {MessageId.SimulatorViewerTimeMessage,   (m) => Instance.OnSimulatorViewerTimeMessage?.Invoke    ((SimulatorViewerTimeMessage)m)     },
        {MessageId.ScriptControlChange,          (m) => Instance.OnScriptControlChangeMessage?.Invoke    ((ScriptControlChangeMessage)m)     },
        {MessageId.ParcelOverlay,                (m) => Instance.OnParcelOverlayMessage?.Invoke          ((ParcelOverlayMessage)m)           },
        {MessageId.AgentMovementCompleteMessage, (m) => Instance.OnAgentMovementCompleteMessage?.Invoke  ((AgentMovementCompleteMessage)m)   },
        {MessageId.LogoutReply,                  (m) => Instance.OnLogoutReplyMessage?.Invoke            ((LogoutReplyMessage)m)             },
        {MessageId.ImprovedInstantMessage,       (m) => Instance.OnImprovedInstantMessageMessage?.Invoke ((ImprovedInstantMessageMessage)m)  },
        {MessageId.OnlineNotification,           (m) => Instance.OnOnlineNotificationMessage?.Invoke     ((OnlineNotificationMessage)m)      },
        {MessageId.OfflineNotification,          (m) => Instance.OnOfflineNotificationMessage?.Invoke    ((OfflineNotificationMessage)m)     },
        {MessageId.AgentDataUpdate,              (m) => Instance.OnAgentDataUpdateMessage?.Invoke        ((AgentDataUpdateMessage)m)         },
    };

    public void RaiseOnMessage(Message message)
    {
        if (HandlerByMessageId.ContainsKey(message.MessageId) == false)
        {
            return;
        }

        ThreadManager.ExecuteOnMainThread(() => HandlerByMessageId[message.MessageId](message));
    }
    #endregion Messages

    #region Logout
    /// <summary>
    /// Raised when the logout process has begun. Listen to this where you need to clean things up.
    /// </summary>
    public event Action OnLogout;
    public void RaiseOnLogout()
    {
        ThreadManager.ExecuteOnMainThread(() => OnLogout?.Invoke());
    }
    #endregion Logout
}
