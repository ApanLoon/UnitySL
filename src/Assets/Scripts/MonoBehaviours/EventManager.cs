using System;
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
    public event Action<AgentDataUpdateMessage> OnAgentDataUpdateMessage;
    public void RaiseOnAgentDataUpdateMessage(AgentDataUpdateMessage message)
    {
        ThreadManager.ExecuteOnMainThread(() => OnAgentDataUpdateMessage?.Invoke(message));
    }

    public event Action<AgentMovementCompleteMessage> OnAgentMovementCompleteMessage;
    public void RaiseOnAgentMovementCompleteMessage(AgentMovementCompleteMessage message)
    {
        ThreadManager.ExecuteOnMainThread(() => OnAgentMovementCompleteMessage?.Invoke(message));
    }
    #endregion Messages
    #endregion Agent

    #region Region
    public event Action<Region> OnRegionDataChanged;

    public void RaiseOnRegionDataChanged(Region region)
    {
        ThreadManager.ExecuteOnMainThread(() => OnRegionDataChanged?.Invoke(region));
    }

    public event Action<RegionHandshakeMessage> OnRegionHandshakeMessage;

    public void RaiseOnRegionHandshakeMessage(RegionHandshakeMessage message)
    {
        ThreadManager.ExecuteOnMainThread(() => OnRegionHandshakeMessage?.Invoke(message));
    }

    #endregion Region
}
