using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat;

namespace Assets.Scripts.MessageLogs
{
    public class LogManager
    {
        public static LogManager Instance = new LogManager();

        public MessageLog DebugLog = new MessageLog(log => Logger.OnLog += (level, senderName, message) => log.AddMessage(new DebugMessage(level, senderName, message)));
        public MessageLog ChatLog  = new MessageLog(
            log           => EventManager.Instance.OnChatFromSimulatorMessage += (message) => log.AddMessage(new ChatMessage(message)),
            async s => await (Agent.CurrentPlayer?.Region?.Circuit?.SendChatFromViewer(s, ChatType.Normal, 0) ?? Task.CompletedTask)
        );

        public event Action<Guid, string, MessageLog> OnNewInstantMessageSession;
        public Dictionary<Guid, MessageLog> InstantMessageLogs = new Dictionary<Guid, MessageLog>();

        public LogManager()
        {
            EventManager.Instance.OnImprovedInstantMessageMessage += OnImprovedInstantMessageMessage;
        }

        protected void OnImprovedInstantMessageMessage(ImprovedInstantMessageMessage message)
        {
            Guid dialogId = message.Id;
            if (InstantMessageLogs.ContainsKey(dialogId) == false)
            {
                Logger.LogInfo("LogManager.OnImprovedInstantMessageMessage", $"Adding message log for {message.FromAgentName}. Dialog Id={dialogId}");
                InstantMessageLogs.Add(dialogId, new MessageLog(null));
                OnNewInstantMessageSession?.Invoke(dialogId, message.FromAgentName, InstantMessageLogs[dialogId]);
            }

            MessageLog log = InstantMessageLogs[dialogId];
            Logger.LogDebug("LogManager.OnImprovedInstantMessageMessage", $"{message.DialogType}: \"{message.MessageText}\"");
            log.AddMessage(new InstantMessage(message));
        }
    }
}
