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

    public void AddPacketAck(UInt32 sequenceNumber)
    {
        if (PacketAcks.Count >= 255)
        {
            throw new Exception("PacketAckMessasge: Too many acks in a single message. Max is 255.");
        }
        PacketAcks.Add(sequenceNumber);
    }

    #region Serialize
    public override int GetSerializedLength()
    {
        return base.GetSerializedLength()
               + 1 // Count
               + 4 * PacketAcks.Count;
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);

        buffer[o++] = (byte)PacketAcks.Count;
        foreach (UInt32 sequenceNumber in PacketAcks)
        {
            o = BinarySerializer.Serialize_Le(sequenceNumber, buffer, o, length);
        }
        return o - offset;
    }
    #endregion Serialize
}