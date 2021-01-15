
using System;

public class CompleteAgentMovementMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public UInt32 CircuitCode { get; set; }

    public CompleteAgentMovementMessage(Guid agentId, Guid sessionId, UInt32 circuitCode)
    {
        MessageId = MessageId.CompleteAgentMovement;
        Flags = PacketFlags.Reliable;

        AgentId = agentId;
        SessionId = sessionId;
        CircuitCode = circuitCode;
    }

    #region Serialise
    public override int GetSerializedLength()
    {
        return base.GetSerializedLength()
               + 16  // AgentId
               + 16  // SessionId
               +  4; // CircuitCode
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);

        o = BinarySerializer.Serialize(AgentId, buffer, o, length);
        o = BinarySerializer.Serialize(SessionId, buffer, o, length);
        o = BinarySerializer.Serialize_Le(CircuitCode, buffer, o, length);

        return o - offset;
    }
    #endregion Serialise

    public override string ToString()
    {
        return $"{base.ToString()}: AgentId={AgentId}, SessionId=\"{SessionId}\", CircuitCode=\"{CircuitCode}\"";
    }
}
