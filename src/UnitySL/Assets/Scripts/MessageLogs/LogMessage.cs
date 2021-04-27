using System;

namespace Assets.Scripts.MessageLogs
{
    public abstract class LogMessage
    {
        public string SenderName { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public abstract string ToRtfString();
    }
}
