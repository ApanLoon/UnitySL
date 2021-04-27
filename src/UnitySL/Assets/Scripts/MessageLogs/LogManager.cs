
using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat;

namespace Assets.Scripts.MessageLogs
{
    public class LogManager
    {
        public static LogManager Instance = new LogManager();

        public event Action<DebugMessage> OnDebugMessage;
        public event Action<ChatMessage> OnChatMessage;

        public MessageLog<DebugMessage> DebugLog = new MessageLog<DebugMessage>();
        public MessageLog<ChatMessage> ChatLog = new MessageLog<ChatMessage>();

        public LogManager()
        {
            Logger.OnLog += OnLog;
            EventManager.Instance.OnChatFromSimulatorMessage += OnChatFromSimulatorMessage;
        }

        protected void OnLog(Logger.LogLevel level, string senderName, string content)
        {
            DebugMessage msg = new DebugMessage
            {
                Level = level,
                Timestamp = DateTimeOffset.UtcNow,
                SenderName = senderName,
                Text = content
            };
            DebugLog.AddMessage(msg);
            OnDebugMessage?.Invoke(msg);
        }

        protected void OnChatFromSimulatorMessage(ChatFromSimulatorMessage message)
        {
            ChatMessage msg = new ChatMessage
            {
                AudibleLevel = message.AudibleLevel,
                ChatType = message.ChatType,
                OwnerId = message.OwnerId,
                Position = message.Position,
                SenderId = message.SourceId,
                SenderName = message.FromName,
                Timestamp = DateTimeOffset.Now,
                SourceType = message.SourceType,
                Text = message.Message
            };
            ChatLog.AddMessage(msg);
            OnChatMessage?.Invoke(msg);
        }
    }
}
