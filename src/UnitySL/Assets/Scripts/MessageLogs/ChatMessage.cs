
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MessageLogs
{
    public enum ChatSourceType : byte
    {
        System = 0,
        Agent = 1,
        Object = 2,
        Unknown = 3
    }

    public enum ChatType : byte
    {
        Whisper = 0,
        Normal = 1,
        Shout = 2,
        Start = 4,
        Stop = 5,
        DebugMessage = 6,
        Region = 7,
        Owner = 8,
        Direct = 9		// From llRegionSayTo()
    }

    public enum ChatAudibleLevel : sbyte
    {
        Not = -1,
        Barely = 0,
        Fully = 1
    }

    public class ChatMessage : LogMessage
    {
        public Guid SenderId { get; set; }
        public Guid OwnerId { get; set; }
        public ChatSourceType SourceType { get; set; }
        public ChatType ChatType { get; set; }
        public ChatAudibleLevel AudibleLevel { get; set; }
        public Vector3 Position { get; set; }

        public string Text { get; set; }

        //TODO: Colours should be fetched from a UIPalette for consistency
        protected Dictionary<Logger.LogLevel, string> LogLevelToColour = new Dictionary<Logger.LogLevel, string>()
        {
            {Logger.LogLevel.Debug,   new Color(0.467f, 0.467f, 0.467f, 1.0f).ToRtfString()},
            {Logger.LogLevel.Info,    Color.green.ToRtfString() },
            {Logger.LogLevel.Warning, Color.yellow.ToRtfString() },
            {Logger.LogLevel.Error,   Color.red.ToRtfString() }
        };

        protected Color NormalColour = Color.white;
        protected Color ObjectColour = Color.green;
        protected Color SystemColour = Color.yellow;
        protected Color UnknownColour = Color.gray;

        public override string ToRtfString()
        {
            string s = $"{Timestamp:T} {SenderName}: {Text}";
            Color colour;
            switch (SourceType)
            {
                case ChatSourceType.System:
                    colour = SystemColour;
                    break;
                case ChatSourceType.Agent:
                    colour = NormalColour;
                    break;
                case ChatSourceType.Object:
                    colour = ObjectColour;
                    break;
                case ChatSourceType.Unknown:
                    colour = UnknownColour;
                    break;
                default:
                    colour = UnknownColour;
                    break;
            }

            switch (ChatType)
            {
                case ChatType.Whisper:
                    s = $"<i>{s}</i>";
                    break;
                case ChatType.Normal:
                    break;
                case ChatType.Shout:
                    s = $"<b>{s}</b>";
                    break;
                case ChatType.Start:
                    break;
                case ChatType.Stop:
                    break;
                case ChatType.DebugMessage:
                    break;
                case ChatType.Region:
                    break;
                case ChatType.Owner:
                    break;
                case ChatType.Direct:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (AudibleLevel)
            {
                case ChatAudibleLevel.Not:
                    break;
                case ChatAudibleLevel.Barely:
                    break;
                case ChatAudibleLevel.Fully:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return $"{colour.ToRtfString()}{s}</color>";
        }
    }
}