using TMPro;
using UnityEngine;

public class MainTitleBar : MonoBehaviour
{
    [SerializeField] protected TMP_Text PlayerName;
    [SerializeField] protected TMP_Text PlayerHealth;
    [SerializeField] protected TMP_Text RegionName;
    [SerializeField] protected TMP_Text PositionText;

    private void Start()
    {
        PlayerName.text   = "";
        PlayerHealth.text = "";
        RegionName.text   = "";
        PositionText.text = "";
        EventManager.Instance.OnHealthChanged     += OnHealthChanged;
        EventManager.Instance.OnAgentDataChanged  += OnAgentDataChanged;
        EventManager.Instance.OnRegionDataChanged += OnRegionDataChanged;
        EventManager.Instance.OnAgentMoved        += OnAgentMoved;

        EventManager.Instance.OnLogout += () =>
        {
            PlayerName.text = "";
            PlayerHealth.text = "";
            RegionName.text = "";
            PositionText.text = "";
        };
    }

    protected void OnHealthChanged(Agent agent)
    {
        if (agent.Id != Agent.CurrentPlayer?.Id)
        {
            return;
        }

        PlayerHealth.text = $"Health: {agent.Health}%";
    }

    protected void OnAgentDataChanged(Agent agent)
    {
        if (agent.Id != Agent.CurrentPlayer?.Id)
        {
            return;
        }

        PlayerName.text = $"{agent.DisplayName} ({agent.FirstName} {agent.LastName})";
    }

    protected void OnRegionDataChanged(Region region)
    {
        if (region.Id != Agent.CurrentPlayer?.Region?.Id)
        {
            return;
        }

        RegionName.text = $"{region.Name}";
    }

    protected void OnAgentMoved(Agent agent)
    {
        if (agent != Agent.CurrentPlayer)
        {
            return;
        }

        PositionText.text = $"{agent.Position}";
    }

}
