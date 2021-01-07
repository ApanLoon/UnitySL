using System;
using System.Collections.Generic;
using NUnit.Framework;

public class CoarseLocation
{
    public Guid AgentId { get; set; }
    public Vector3Byte Position { get; set; }
    public bool IsYou { get; set; }

    /// <summary>
    /// True if you are tracking this agent
    /// </summary>
    public bool IsPrey { get; set; }
}

public class CoarseLocationUpdateMessage : Message
{
    public List<CoarseLocation> Locations = new List<CoarseLocation>();
    
    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public CoarseLocationUpdateMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
