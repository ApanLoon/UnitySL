using System;

public class AgentHeightWidthMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public UInt32 CircuitCode { get; set; }

    public UInt32 GenCounter { get; set; }

    public UInt16 Height { get; set; }
    public UInt16 Width { get; set; }

    public AgentHeightWidthMessage(Guid agentId, Guid sessionId, UInt32 circuitCode, UInt32 genCounter, UInt16 height, UInt16 width)
    {
        MessageId = MessageId.AgentHeightWidth;
        Flags = PacketFlags.Reliable;

        AgentId = agentId;
        SessionId = sessionId;
        CircuitCode = circuitCode;

        GenCounter = genCounter;

        Height = height;
        Width = width;
    }

    public override int GetSerializedLength()
    {
        return base.GetSerializedLength()
               + 16     // AgentId
               + 16     // SessionId
               + 4      // CircuitCode
               + 4      // GenCounter
               + 2      // Height
               + 2;     // Width
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);

        o = BinarySerializer.Serialize(AgentId, buffer, o, length);
        o = BinarySerializer.Serialize(SessionId, buffer, o, length);
        o = BinarySerializer.Serialize_Le(CircuitCode, buffer, o, length);

        o = BinarySerializer.Serialize_Le(GenCounter, buffer, o, length);

        o = BinarySerializer.Serialize_Le(Height, buffer, o, length);
        o = BinarySerializer.Serialize_Le(Width, buffer, o, length);

        return o - offset;
    }
}
