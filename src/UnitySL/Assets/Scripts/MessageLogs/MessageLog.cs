using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Scripts.MessageLogs
{
    public class MessageLog
    {
        public event Action<MessageLog, LogMessage> OnMessage;
        public string Name { get; protected set; }
        public List<LogMessage> Log = new List<LogMessage>();
        public IEnumerable<string> AllMessagesAsRtfStrings => Log.Select(x => x.ToRtfString());


        public bool CanSend => Send != null;
        protected Func<string, Task> Send { get; set; }

        public MessageLog(string name, Func<string, Task> send = null)
        {
            Name = name;
            Send = send;
        }

        public void Clear()
        {
            Log.Clear();
        }

        public void AddMessage(LogMessage message)
        {
            message.Timestamp = DateTimeOffset.UtcNow;
            Log.Add(message);
            OnMessage?.Invoke(this, message);
        }

        public async Task SendMessage(string message)
        {
            if (CanSend)
            {
                await Send(message);
            }
        }
    }
}
