
using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat;
using TMPro;
using UnityEngine;

public class ImTest : MonoBehaviour
{
    public TMP_InputField AgentId;
    public TMP_InputField Message;

    public async void Send()
    {
        Guid toAgentId = Guid.Parse(AgentId.text.Trim());
        string message = Message.text;

        await Agent.CurrentPlayer.Region.Circuit.SendInstantMessage (false, toAgentId, 1, DialogType.NothingSpecial, Guid.Empty, message, null);
        await Agent.CurrentPlayer.Region.Circuit.SendInstantMessage(false, toAgentId, 0, DialogType.TypingStart, Guid.Empty, "typing", null);
        await Agent.CurrentPlayer.Region.Circuit.SendInstantMessage(false, toAgentId, 0, DialogType.TypingStop, Guid.Empty, "typing", null);
    }
}
