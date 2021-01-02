using System;
using System.Collections.Generic;
using NUnit.Framework;


public enum MessageId : UInt32
{
    Wrapper               = 0xffff0001,
    UseCircuitCode        = 0xffff0003,
    CompleteAgentMovement = 0xffff00f9,
    AgentDataUpdate       = 0xffff0183,
    PacketAck             = 0xfffffffb,
    OpenCircuit           = 0xfffffffc
}
[Flags] public enum PacketFlags : byte
{
    ZeroCode = 0x80,
    Reliable = 0x40,
    Resent = 0x20,
    Ack = 0x10
}

public enum MessageFrequency : UInt32
{
    High, 
    Medium,
    Low,
    Fixed
}

public enum MessageTrustLevel
{
    Trusted,
    NotTrusted
}

public enum MessageEncoding
{
    Unencoded,
    Zerocoded
}

public class Message
{
    public PacketFlags Flags { get; set; }
    public UInt32 SequenceNumber { get; set; }
    public byte[] ExtraHeader { get; set; }


    public string Name { get; set; }
    public MessageFrequency Frequency { get; set; }
    public MessageId Id { get; set; }
    public MessageTrustLevel TrustLevel { get; set; }

    public List<UInt32> Acks { get; set; }

    public virtual int GetSerializedLength()
    {
        int size = 1                   // Flags
                   + 4                   // SequenceNumber
                   + 1                   // Extra header length
                   + (ExtraHeader?.Length ?? 0); // Extra header
        switch (Frequency)
        {
            case MessageFrequency.High:
                size += 1; // Message Id
                break;
            case MessageFrequency.Medium:
                size += 2; // Message Id
                break;
            case MessageFrequency.Low:
            case MessageFrequency.Fixed:
                size += 4; // Message Id
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Frequency), "Message.GetSerializedLength(Message): Unknown message frequency.");
        }

        if (Acks != null && Acks.Count != 0)
        {
            size += Acks.Count * 4 + 1;
        }
        return size;
    }

    public virtual int Serialize(byte[] buffer, int offset, int length)
    {
        if (length - offset < GetSerializedLength())
        {
            throw new ArgumentOutOfRangeException(nameof(offset),
                "Message.Serialize: Not enough room in the target buffer.");
        }

        int o = offset;

        if (Acks != null && Acks.Count != 0)
        {
            Flags |= PacketFlags.Ack;
        }

        buffer[o++] = (byte)Flags;
        buffer[o++] = (byte)(SequenceNumber >> 24);
        buffer[o++] = (byte)(SequenceNumber >> 16);
        buffer[o++] = (byte)(SequenceNumber >> 8);
        buffer[o++] = (byte)(SequenceNumber >> 0);
        buffer[o++] = (byte)(ExtraHeader?.Length ?? 0);

        UInt32 id = (UInt32)Id;
        if (Frequency == MessageFrequency.Fixed || Frequency == MessageFrequency.Low)
        {
            buffer[o++] = (byte)(id >> 24);
            buffer[o++] = (byte)(id >> 16);
        }
        if (Frequency == MessageFrequency.Medium || Frequency == MessageFrequency.Fixed || Frequency == MessageFrequency.Low)
        {
            buffer[o++] = (byte)(id >> 8);
        }
        buffer[o++] = (byte)(id >> 0);
        return o - offset;
    }

    public override string ToString()
    {
        string idString;
        switch (Frequency)
        {
            case MessageFrequency.High:
                idString = ((byte)Id).ToString();
                break;
            case MessageFrequency.Medium:
                idString = ((byte)Id).ToString();
                break;
            case MessageFrequency.Low:
                idString = ((UInt16)Id).ToString();
                break;
            case MessageFrequency.Fixed:
                idString = ((UInt32)Id).ToString("x8");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return $"{Id} {Frequency} {idString}";
    }
}
