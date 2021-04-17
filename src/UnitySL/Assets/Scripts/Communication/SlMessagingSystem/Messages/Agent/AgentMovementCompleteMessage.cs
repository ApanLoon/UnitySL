using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using UnityEngine;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Agent
{
    public class AgentMovementCompleteMessage : Message
    {
        public Guid AgentId { get; set; }
        public Guid SessionId { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 LookAt { get; set; }
        public RegionHandle RegionHandle { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ChannelVersion { get; set; }

        public AgentMovementCompleteMessage()
        {
            MessageId = MessageId.AgentMovementCompleteMessage;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            AgentId        = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            SessionId      = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            Position       = BinarySerializer.DeSerializeVector3  (buf, ref o, length);
            LookAt         = BinarySerializer.DeSerializeVector3  (buf, ref o, length);
            RegionHandle   = new RegionHandle(BinarySerializer.DeSerializeUInt64_Le(buf, ref o, length));
            TimeStamp      = BinarySerializer.DeSerializeDateTime (buf, ref o, length);
            ChannelVersion = BinarySerializer.DeSerializeString   (buf, ref o, length, 2);
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: AgentId={AgentId}, Position=\"{Position}\", LookAt=\"{LookAt}\", TimeStamp={TimeStamp:yyyy'-'MM'-'dd'T'HH':'mm':'ssK}";
        }
    }
}
