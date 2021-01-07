using System;
using System.Collections.Generic;
using NUnit.Framework;

public class PreloadSoundMessage : Message
{
    public class SoundInfo
    {
        public Guid ObjectId { get; set; }
        public Guid OwnerId { get; set; }
        public Guid SoundId { get; set; }
    }

    public List<SoundInfo> Sounds { get; set; } = new List<SoundInfo>();

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public PreloadSoundMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
