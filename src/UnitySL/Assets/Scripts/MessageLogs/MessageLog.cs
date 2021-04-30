using System;
using System.Collections.Generic;

namespace Assets.Scripts.MessageLogs
{
    public class MessageLog
    {
        public event Action<LogMessage> OnMessage;
        public List<LogMessage> Log = new List<LogMessage>();

        public MessageLog(Action<MessageLog> init)
        {
            init(this);
        }

        public void Clear()
        {
            Log.Clear();
        }

        public void AddMessage(LogMessage message)
        {
            message.Timestamp = DateTimeOffset.UtcNow;
            OnMessage?.Invoke(message);
        }
    }
}
