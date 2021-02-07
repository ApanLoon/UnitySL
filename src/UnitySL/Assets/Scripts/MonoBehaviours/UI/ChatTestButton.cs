using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ChatTestButton : MonoBehaviour
{
    protected TMP_Text Text;

    void Start()
    {
        Text = GetComponentInChildren<TMP_Text>();
        EventManager.Instance.OnChatFromSimulatorMessage += OnChatFromSimulator;
    }

    protected void OnChatFromSimulator(ChatFromSimulatorMessage message)
    {
        Text.text += $"\n{message.Message}";
    }

    public async void SendTestChat (string message)
    {
        await Agent.CurrentPlayer.Region.Circuit.SendChatFromViewer(message, ChatType.Normal, 0);
    }
    
}
