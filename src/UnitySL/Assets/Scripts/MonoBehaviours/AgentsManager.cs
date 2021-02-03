using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentsManager : MonoBehaviour
{
    [SerializeField] protected GameObject AgentPrefab;
    
    protected static Dictionary<Guid, GameObject> AgentGoById = new Dictionary<Guid, GameObject>();

    private void Start()
    {
        EventManager.Instance.OnAgentMoved += OnAgentMoved;
    }

    protected void OnAgentMoved(Agent agent)
    {
        if (AgentGoById.ContainsKey(agent.Id) == false)
        {
            AddAgentGameObject(agent);
        }

        GameObject go = AgentGoById[agent.Id];
        go.transform.position = agent.Position;

        // Head looks at the point, the body ignores the y
        Vector3 bodyLookAt = agent.LookAt;
        bodyLookAt.y = agent.Position.y;
        go.transform.LookAt(bodyLookAt);
    }

    protected void AddAgentGameObject(Agent agent)
    {
        GameObject go = Instantiate(AgentPrefab, transform);
        AgentGoById[agent.Id] = go;

        AgentController controller = go.GetComponent<AgentController>();
        if (controller == null)
        {
            Logger.LogError("AgentsManager.AddAgentGameObject: Agent prefab has no AgentController.");
            return;
        }

        go.name = $"{agent.FirstName} {agent.LastName}";
        controller.SetName(agent.DisplayName);
        controller.SetGroupTitle(agent.GroupTitle);
    }
}
