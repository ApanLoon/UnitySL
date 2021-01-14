using System;
using UnityEngine;

public enum ChatSourceType : byte
{
    System  = 0,
    Agent   = 1,
    Object  = 2,
    Unknown = 3
}

public enum ChatType : byte
{
    Whisper      = 0,
    Normal       = 1,
    Shout        = 2,
    Start        = 4,
    Stop         = 5,
    DebugMessage = 6,
    Region       = 7,
    Owner        = 8,
    Direct       = 9		// From llRegionSayTo()
}

public enum ChatAudibleLevel : sbyte
{
    Not    = -1,
    Barely = 0,
    Fully  = 1
}

public class ChatFromSimulatorMessage : Message
{
    public string FromName { get; set; }
    public Guid SourceId { get; set; }
    public Guid OwnerId { get; set; }
    public ChatSourceType SourceType { get; set; }
    public ChatType ChatType { get; set; }
    public ChatAudibleLevel AudibleLevel { get; set; }
    public Vector3 Position { get; set; }
    public string Message { get; set; }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public ChatFromSimulatorMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
