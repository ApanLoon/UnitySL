using System;

public class AgentDataUpdateRequestMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }

    public AgentDataUpdateRequestMessage(Guid agentId, Guid sessionId)
    {
        Id = MessageId.AgentDataUpdateRequest;
        Flags = PacketFlags.Reliable;
        Frequency = MessageFrequency.Low;

        AgentId = agentId;
        SessionId = sessionId;
    }

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

}
