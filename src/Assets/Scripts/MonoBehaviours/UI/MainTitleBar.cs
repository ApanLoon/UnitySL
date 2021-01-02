using TMPro;
using UnityEngine;

public class MainTitleBar : MonoBehaviour
{
    [SerializeField] protected TMP_Text PlayerName;
    [SerializeField] protected TMP_Text RegionName;
    [SerializeField] protected TMP_Text PositionText;

    private void Start()
    {
        PlayerName.text = "";
        RegionName.text = "";
        PositionText.text = "";
        EventManager.Instance.OnAgentDataChanged += OnAgentDataChanged;
        EventManager.Instance.OnRegionDataChanged += OnRegionDataChanged;
        EventManager.Instance.OnAgentMoved += OnAgentMoved;
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
        if (region.Id != Region.CurrentRegion?.Id)
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
