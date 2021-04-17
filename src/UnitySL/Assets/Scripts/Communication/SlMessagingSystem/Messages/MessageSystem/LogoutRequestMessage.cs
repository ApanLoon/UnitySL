using System;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem
{
    public class LogoutRequestMessage : Message
    {
        public Guid AgentId { get; set; }
        public Guid SessionId { get; set; }

        public LogoutRequestMessage (Guid agentId, Guid sessionId)
        {
            MessageId = MessageId.LogoutRequest;
            Flags = PacketFlags.Reliable;

            AgentId = agentId;
            SessionId = sessionId;
        }

        #region Serialise
        public override int GetSerializedLength()
        {
            return base.GetSerializedLength()
                   + 16  // AgentId
                   + 16; // SessionId
        }
        public override int Serialize(byte[] buffer, int offset, int length)
        {
            int o = offset;
            o += base.Serialize(buffer, offset, length);

            o = BinarySerializer.Serialize(AgentId, buffer, o, length);
            o = BinarySerializer.Serialize(SessionId, buffer, o, length);

            return o - offset;
        }
        #endregion Serialise
    }
}
