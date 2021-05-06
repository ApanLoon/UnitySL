using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat;

namespace Assets.Scripts.MessageLogs
{
    public class LogManager
    {
        public static LogManager Instance = new LogManager();

        public MessageLog DebugLog = new MessageLog("Debug");
        public MessageLog ChatLog  = new MessageLog("Local", async s => await (Agent.CurrentPlayer?.Region?.Circuit?.SendChatFromViewer(s, ChatType.Normal, 0) ?? Task.CompletedTask)
        );

        public event Action<Guid, string, MessageLog> OnNewInstantMessageSession;
        public Dictionary<Guid, MessageLog> InstantMessageLogs = new Dictionary<Guid, MessageLog>();

        public LogManager()
        {
            Logger.OnLog += OnLog;
            EventManager.Instance.OnChatFromSimulatorMessage += OnChatFromSimulatorMessage;
            EventManager.Instance.OnImprovedInstantMessageMessage += OnImprovedInstantMessageMessage;
        }

        protected void OnLog(Logger.LogLevel level, string senderName, string message)
        {
            DebugLog.AddMessage(new DebugMessage(level, senderName, message));
        }

        protected void OnChatFromSimulatorMessage(ChatFromSimulatorMessage message)
        {
            ChatLog.AddMessage(new ChatMessage(message));
        }

        protected void OnImprovedInstantMessageMessage(ImprovedInstantMessageMessage message)
        {
            Guid dialogId = message.Id;
            if (InstantMessageLogs.ContainsKey(dialogId) == false)
            {
                Logger.LogInfo("LogManager.OnImprovedInstantMessageMessage", $"Adding message log for {message.FromAgentName}. Dialog Id={dialogId}");
                InstantMessageLogs.Add(dialogId, 
                    new MessageLog(
                        message.FromAgentName,
                    async s =>
                    {
                        if (Agent.CurrentPlayer?.Region?.Circuit == null)
                        {
                            return;
                        }

                        ImprovedInstantMessageMessage msg = await Agent.CurrentPlayer.Region.Circuit.SendInstantMessage (
                            false,
                            message.AgentId,
                            message.ParentEstateId, // TODO: Do I ever need to specify this?
                            DialogType.NothingSpecial,
                            message.Id,
                            s,
                            null);

                        // Pretend that we received this message to get it into the log:
                        OnImprovedInstantMessageMessage(msg);
                    })
                );
                OnNewInstantMessageSession?.Invoke(dialogId, message.FromAgentName, InstantMessageLogs[dialogId]);
            }

            MessageLog log = InstantMessageLogs[dialogId];
            Logger.LogDebug("LogManager.OnImprovedInstantMessageMessage", $"{message.DialogType}: \"{message.MessageText}\"");
            log.AddMessage(new InstantMessage(message));
        }
    }
}
