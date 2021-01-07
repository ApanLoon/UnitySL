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
    public event Action<AgentDataUpdateMessage> OnAgentDataUpdateMessage;
    public event Action<AgentMovementCompleteMessage> OnAgentMovementCompleteMessage;
    #endregion Messages
    #endregion Agent

    #region Region
    public event Action<Region> OnRegionDataChanged;

    public void RaiseOnRegionDataChanged(Region region)
    {
        ThreadManager.ExecuteOnMainThread(() => OnRegionDataChanged?.Invoke(region));
    }

    public event Action<RegionHandshakeMessage> OnRegionHandshakeMessage;
    #endregion Region

    #region Audio
    public event Action<AttachedSoundMessage> OnAttachedSoundMessage;
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
        {MessageId.AttachedSound,                (m) => Instance.OnAttachedSoundMessage?.Invoke         ((AttachedSoundMessage)m)         },
        {MessageId.ViewerEffect,                 (m) => Instance.OnViewerEffectMessage?.Invoke          ((ViewerEffectMessage)m)          },
        {MessageId.HealthMessage,                (m) => Instance.OnHealthMessage?.Invoke                ((HealthMessage)m)                },
        {MessageId.RegionHandshake,              (m) => Instance.OnRegionHandshakeMessage?.Invoke       ((RegionHandshakeMessage)m)       },
        {MessageId.AgentMovementCompleteMessage, (m) => Instance.OnAgentMovementCompleteMessage?.Invoke ((AgentMovementCompleteMessage)m) },
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
