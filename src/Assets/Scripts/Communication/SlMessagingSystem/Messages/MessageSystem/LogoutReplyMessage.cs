using System;
using System.Collections.Generic;

public class LogoutReplyMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public List<Guid> InventoryItems { get; set; } = new List<Guid>();

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public LogoutReplyMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }

}
