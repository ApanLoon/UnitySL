using System;

public class LogoutRequestMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }

    public LogoutRequestMessage (Guid agentId, Guid sessionId)
    {
        Id = MessageId.LogoutRequest;
        Flags = PacketFlags.Reliable;
        Frequency = MessageFrequency.Low;

        AgentId = agentId;
        SessionId = sessionId;
    }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public LogoutRequestMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
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
