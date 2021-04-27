using System;
using System.Collections.Generic;

namespace Assets.Scripts.MessageLogs
{
    public class MessageLog<T> where T: LogMessage
    {
        public List<T> Log = new List<T>();

        public void Clear()
        {
            Log.Clear();
        }

        public void AddMessage(T message)
        {
            message.Timestamp = DateTimeOffset.UtcNow;
        }
    }
}
