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

    public void RaiseOnAOnAgentDataChanged(Agent agent)
    {
        OnAgentDataChanged?.Invoke(agent);
    }

    public event Action<AgentDataUpdateMessage> OnAgentDataUpdateMessage;
    public void RaiseOnAgentDataUpdateMessage(AgentDataUpdateMessage message)
    {
        OnAgentDataUpdateMessage?.Invoke(message);
    }
    #endregion Agent
}
