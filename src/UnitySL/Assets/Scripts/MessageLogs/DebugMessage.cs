using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MessageLogs
{
    public class DebugMessage : LogMessage
    {
        public Logger.LogLevel Level { get; set; }
        public string Text { get; set; }

        public DebugMessage (Logger.LogLevel level, string senderName, string message)
        {
            Level = level;
            Timestamp = DateTimeOffset.UtcNow;
            SenderName = senderName;
            Text = message;
        }

        //TODO: Colours should be fetched from a UIPalette for consistency
        protected Dictionary<Logger.LogLevel, string> LogLevelToColour = new Dictionary<Logger.LogLevel, string>()
        {
            {Logger.LogLevel.Debug,   new Color(0.467f, 0.467f, 0.467f, 1.0f).ToRtfString()},
            {Logger.LogLevel.Info,    Color.green.ToRtfString() },
            {Logger.LogLevel.Warning, Color.yellow.ToRtfString() },
            {Logger.LogLevel.Error,   Color.red.ToRtfString() }
        };
        
        public override string ToRtfString()
        {
            return $"{LogLevelToColour[Level]}{Timestamp:T} {SenderName}: {Text}</color>";
        }
    }
}
