using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Region
{
    [Flags]
    public enum RegionHandshakeReplyFlags : UInt32
    {
        SendAllCacheableObjects = 1, 
        CacheFileIsEmpty        = 2,   // No need to send cache probes
        SupportsSelfAppearance  = 4
    }

    public class RegionHandshakeReplyMessage : Message
    {
        public Guid AgentId { get; set; }
        public Guid SessionId { get; set; }
        public RegionHandshakeReplyFlags ReplyFlags { get; set; }

        public RegionHandshakeReplyMessage(Guid agentId, Guid sessionId, RegionHandshakeReplyFlags replyFlags)
        {
            MessageId = MessageId.RegionHandshakeReply;
            Flags = PacketFlags.Reliable; // TODO: message_template.msg says that this should be ZeroCoded but I don't have a way of doing that yet and Firestorm doesn't do it

            AgentId    = agentId;
            SessionId  = sessionId;
            ReplyFlags = replyFlags;
        }

        #region Serialise
        public override int GetSerializedLength()
        {
            return base.GetSerializedLength()
                   + 16  // AgentId
                   + 16  // SessionId
                   +  4; // RegionFlags
        }
        public override int Serialize(byte[] buffer, int offset, int length)
        {
            int o = offset;
            o += base.Serialize(buffer, offset, length);

            o = BinarySerializer.Serialize(AgentId,     buffer, o, length);
            o = BinarySerializer.Serialize(SessionId,   buffer, o, length);
            o = BinarySerializer.Serialize_Le((UInt32)ReplyFlags, buffer, o, length);

            return o - offset;
        }
        #endregion Serialise
    }
}