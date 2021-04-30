
using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat;
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

        public ChatMessage (ChatFromSimulatorMessage message)
        {
            AudibleLevel = message.AudibleLevel;
            ChatType = message.ChatType;
            OwnerId = message.OwnerId;
            Position = message.Position;
            SenderId = message.SourceId;
            SenderName = message.FromName;
            Timestamp = DateTimeOffset.Now;
            SourceType = message.SourceType;
            Text = message.Message;
        }

        //TODO: Colours should be fetched from a UIPalette for consistency
        protected Dictionary<Logger.LogLevel, string> LogLevelToColour = new Dictionary<Logger.LogLevel, string>()
        {
            {Logger.LogLevel.Debug,   new Color(0.467f, 0.467f, 0.467f, 1.0f).ToRtfString()},
            {Logger.LogLevel.Info,    Color.green.ToRtfString() },
            {Logger.LogLevel.Warning, Color.yellow.ToRtfString() },
            {Logger.LogLevel.Error,   Color.red.ToRtfString() }
        };

        protected Color NameColour = new Color(0.7333f, 0.898f, 0.4824f, 1.0f);
        protected Color NormalColour = Color.white;
        protected Color ObjectColour = Color.green;
        protected Color SystemColour = Color.cyan;
        protected Color UnknownColour = Color.red;
        protected Color DebugColour = Color.magenta;
        protected Color RegionColour = Color.red;
        protected Color OwnerColour = Color.gray;
        protected Color DirectColour = Color.yellow;


        public override string ToRtfString()
        {
            string s = $"[{Timestamp:t}] {NameColour.ToRtfString()}{SenderName}</color>";
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
                    s = $"<i>{s} whispers: {Text}</i>";
                    break;
                case ChatType.Normal:
                    s = $"{s}: {Text}";
                    break;
                case ChatType.Shout:
                    s = $"<b>{s} shouts: {Text}</b>";
                    break;
                case ChatType.Start:
                    s = $"{s} starts typing...";
                    break;
                case ChatType.Stop:
                    s = $"{s} stops typing.";
                    break;
                case ChatType.DebugMessage:
                    s = $"{s}: {Text}";
                    colour = DebugColour;
                    break;
                case ChatType.Region:
                    s = $"{s}: {Text}";
                    colour = RegionColour;
                    break;
                case ChatType.Owner:
                    s = $"{s}: {Text}";
                    colour = OwnerColour;
                    break;
                case ChatType.Direct:
                    s = $"{s}: {Text}";
                    colour = DirectColour;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // TODO: Fade the alpha depending on audible level?
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