using System;

public class RegionHandshakeReplyMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public UInt32 RegionFlags { get; set; }

    public RegionHandshakeReplyMessage(Guid agentId, Guid sessionId, UInt32 regionFlags)
    {
        Id = MessageId.RegionHandshakeReply;
        Flags = 0; // TODO: message_template.msg says that this should be ZeroCoded but I don't have a way of doing that yet
        Frequency = MessageFrequency.Low;

        AgentId = agentId;
        SessionId = sessionId;
        RegionFlags = regionFlags;
    }

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
        o = BinarySerializer.Serialize_Le(RegionFlags, buffer, o, length);

        return o - offset;
    }

}
