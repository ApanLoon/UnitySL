using System;

public class UseCircuitCodeMessage : Message
{
    public UInt32 CircuitCode { get; set; }
    public Guid SessionId { get; set; }
    public Guid AgentId { get; set; }

    public UseCircuitCodeMessage(UInt32 circuitCode, Guid sessionId, Guid agentId)
    {
        Id = MessageId.UseCircuitCode;
        Flags = PacketFlags.Reliable;
        Frequency = MessageFrequency.Low;

        CircuitCode = circuitCode;
        SessionId = sessionId;
        AgentId = agentId;
    }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public UseCircuitCodeMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
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
               + 4   // Code
               + 16  // SessionId
               + 16; // AgentId
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);

        o = BinarySerializer.Serialize(CircuitCode, buffer, o, length);
        o = BinarySerializer.Serialize(SessionId, buffer, o, length);
        o = BinarySerializer.Serialize(AgentId, buffer, o, length);

        return o - offset;
    }
}