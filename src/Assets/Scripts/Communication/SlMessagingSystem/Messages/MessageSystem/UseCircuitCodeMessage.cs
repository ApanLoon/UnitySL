using System;

public class UseCircuitCodeMessage : Message
{
    public UInt32 CircuitCode { get; set; }
    public Guid SessionId { get; set; }
    public Guid AgentId { get; set; }

    public UseCircuitCodeMessage(UInt32 circuitCode, Guid sessionId, Guid agentId)
    {
        MessageId = MessageId.UseCircuitCode;
        Flags = PacketFlags.Reliable;

        CircuitCode = circuitCode;
        SessionId = sessionId;
        AgentId = agentId;
    }

    #region Serialise
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

        o = BinarySerializer.Serialize_Le (CircuitCode, buffer, o, length);
        o = BinarySerializer.Serialize    (SessionId, buffer, o, length);
        o = BinarySerializer.Serialize    (AgentId, buffer, o, length);

        return o - offset;
    }
    #endregion Serialise
}