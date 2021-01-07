using System;
using UnityEngine;

public class AgentMovementCompleteMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 LookAt { get; set; }
    public RegionHandle RegionHandle { get; set; }
    public DateTime TimeStamp { get; set; }
    public string ChannelVersion { get; set; }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public AgentMovementCompleteMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }

    public override string ToString()
    {
        return $"{base.ToString()}: AgentId={AgentId}, Position=\"{Position}\", LookAt=\"{LookAt}\", TimeStamp={TimeStamp:yyyy'-'MM'-'dd'T'HH':'mm':'ssK}";
    }
}
