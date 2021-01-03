using System;

public class AgentThrottleMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public UInt32 CircuitCode { get; set; }
    public UInt32 GenCounter { get; set; }

    public float Resend { get; set; }
    public float Land { get; set; }
    public float Wind { get; set; }
    public float Cloud { get; set; }
    public float Task { get; set; }
    public float Texture { get; set; }
    public float Asset { get; set; }

    public AgentThrottleMessage(Guid agentId, Guid sessionId, UInt32 circuitCode, UInt32 genCounter, float resend, float land, float wind, float cloud, float task, float texture, float asset)
    {
        Id = MessageId.AgentThrottle;
        Flags = PacketFlags.Reliable; //TODO: Could be zerocoded
        Frequency = MessageFrequency.Low;

        AgentId = agentId;
        SessionId = sessionId;
        CircuitCode = circuitCode;

        GenCounter = genCounter;

        Resend = resend;
        Land = land;
        Wind = wind;
        Cloud = cloud;
        Task = task;
        Texture = texture;
        Asset = asset;
    }

    public override int GetSerializedLength()
    {
        return base.GetSerializedLength()
               + 16     // AgentId
               + 16     // SessionId
               + 4      // CircuitCode
               + 4      // GenCounter
               + 1      // length
               + 7 * 4; // Throttles
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);

        o = BinarySerializer.Serialize(AgentId, buffer, o, length);
        o = BinarySerializer.Serialize(SessionId, buffer, o, length);
        o = BinarySerializer.Serialize_Le(CircuitCode, buffer, o, length);

        o = BinarySerializer.Serialize_Le(GenCounter, buffer, o, length);

        buffer[o++] = 7 * 4;
        o = BinarySerializer.Serialize_Le(Resend,  buffer, o, length);
        o = BinarySerializer.Serialize_Le(Land,    buffer, o, length);
        o = BinarySerializer.Serialize_Le(Wind,    buffer, o, length);
        o = BinarySerializer.Serialize_Le(Cloud,   buffer, o, length);
        o = BinarySerializer.Serialize_Le(Task,    buffer, o, length);
        o = BinarySerializer.Serialize_Le(Texture, buffer, o, length);
        o = BinarySerializer.Serialize_Le(Asset,   buffer, o, length);

        return o - offset;
    }
}
