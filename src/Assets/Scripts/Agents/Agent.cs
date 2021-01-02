
using System;
using System.Collections.Generic;
using UnityEngine;

public class Agent :IDisposable
{
    public static Agent CurrentPlayer;

    public static void SetCurrentPlayer(Agent agent)
    {
        AddAgent(agent.Id, agent);
        CurrentPlayer = agent;
    }

    protected static Dictionary<Guid, Agent> AgentById = new Dictionary<Guid, Agent>();

    public static void AddAgent(Guid id, Agent agent)
    {
        AgentById[id] = agent;
    }
    public static void RemoveAgent(Guid id)
    {
        if (AgentById.ContainsKey(id) == true)
        {
            AgentById.Remove(id);
        }
    }

    public Agent GetAgentById(Guid id)
    {
        return AgentById.ContainsKey(id) == false ? null : AgentById[id];
    }

    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }

    public Guid ActiveGroupId { get; set; }
    public string GroupName { get; set; }
    public string GroupTitle { get; set; }
    public UInt64 GroupPowers { get; set; }

    public Vector3 Position { get; set; }

    public Agent(Guid id)
    {
        Id = id;
        EventManager.Instance.OnAgentDataUpdateMessage += OnAgentDataUpdateMessage;
    }

    protected void OnAgentDataUpdateMessage(AgentDataUpdateMessage message)
    {
        if (message.AgentId != Id)
        {
            return;
        }

        FirstName = message.FirstName;
        LastName = message.LastName;
        GroupTitle = message.GroupTitle;
        ActiveGroupId = message.ActiveGroupId;
        GroupPowers = message.GroupPowers;
        GroupName = message.GroupName;

        EventManager.Instance.RaiseOnAOnAgentDataChanged(this);
    }

    public void Dispose()
    {
        EventManager.Instance.OnAgentDataUpdateMessage -= OnAgentDataUpdateMessage;
    }
}
