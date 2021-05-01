using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.MessageLogs;
using UnityEngine;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat
{
    public class ChatFromSimulatorMessage : Message
    {
        public string FromName { get; set; }
        public Guid SourceId { get; set; }
        public Guid OwnerId { get; set; }
        public ChatSourceType SourceType { get; set; }
        public ChatType ChatType { get; set; }
        public ChatAudibleLevel AudibleLevel { get; set; }
        public Vector3 Position { get; set; }
        public string Message { get; set; }

        public ChatFromSimulatorMessage()
        {
            MessageId = MessageId.ChatFromSimulator;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            FromName     = BinarySerializer.DeSerializeString(buf, ref o, length, 1);
            SourceId     = BinarySerializer.DeSerializeGuid(buf, ref o, length);
            OwnerId      = BinarySerializer.DeSerializeGuid(buf, ref o, length);
            SourceType   = (ChatSourceType)buf[o++];
            ChatType     = (ChatType)buf[o++];
            AudibleLevel = (ChatAudibleLevel)buf[o++];
            Position     = BinarySerializer.DeSerializeVector3(buf, ref o, length);
            Message      = BinarySerializer.DeSerializeString(buf, ref o, length, 2);

            Logger.LogDebug ("ChatFromSimulatorMessage.DeSerialise", ToString());
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: FromName={FromName}, SourceId={SourceId}, OwnerId={OwnerId}, SourceType={SourceType}, ChatType={ChatType}, AudibleLevel={AudibleLevel}, Position={Position}, Message={Message}";
        }
    }
}