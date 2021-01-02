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
    }

    protected void OnAgentDataChanged(Agent agent)
    {
        if (agent.Id != Agent.CurrentPlayer?.Id)
        {
            return;
        }

        PlayerName.text = $"{agent.DisplayName} ({agent.FirstName} {agent.LastName})";

    }
}
