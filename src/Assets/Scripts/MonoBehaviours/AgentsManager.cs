using UnityEngine;

public class AgentsManager : MonoBehaviour
{
    [SerializeField] protected GameObject CurrentPlayer; // TODO: This should probably be rezzed

    private void Start()
    {
        EventManager.Instance.OnAgentMoved += OnAgentMoved;
    }

    protected void OnAgentMoved(Agent agent)
    {
        if (agent != Agent.CurrentPlayer)
        {
            return;
        }

        CurrentPlayer.transform.position = agent.Position;
        // Head looks at the point, the body ignores the y
        Vector3 bodyLookAt = agent.LookAt;
        bodyLookAt.y = agent.Position.y;
        CurrentPlayer.transform.LookAt(bodyLookAt);
    }
}
