using System;
using UnityEngine;

public class SoundTriggerMessage : Message
{
    public Guid SoundId { get; set; }
    public Guid OwnerId { get; set; }
    public Guid ObjectId { get; set; }
    public Guid ParentId { get; set; }
    public RegionHandle Handle { get; set; }
    public Vector3 Position { get; set; }
    public float Gain { get; set; }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public SoundTriggerMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
