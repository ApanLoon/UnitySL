using System;

[Flags]
public enum SoundFlags : byte
{
    None        = 0x00,
    Loop        = 0x01,
    SyncMaster  = 0x02,
    SyncSlave   = 0x04,
    SyncPending = 0x08,
    Queue       = 0x10,
    Stop        = 0x20,
    SyncMask    = SyncMaster | SyncSlave | SyncPending
}

public class AttachedSoundMessage : Message
{
    public Guid SoundId { get; set; }
    public Guid ObjectId { get; set; }
    public Guid OwnerId { get; set; }
    public float Gain { get; set; }
    public SoundFlags SoundFlags { get; set; }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public AttachedSoundMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
