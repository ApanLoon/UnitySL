using System;
using System.Collections.Generic;

public class PacketAckMessage : Message
{
    public List<UInt32> PacketAcks { get; set; } = new List<uint>();

    public PacketAckMessage()
    {
        Id = MessageId.PacketAck;
        Flags = 0;
        Frequency = MessageFrequency.Fixed;
    }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public PacketAckMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }

    public void AddAck(UInt32 ack)
    {
        PacketAcks.Add(ack);
    }
}