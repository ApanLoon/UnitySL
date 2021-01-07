using System;
using System.Collections.Generic;

public enum MessageId : UInt32
{
    StartPingCheck               = 0x00000001,
    CompletePingCheck            = 0x00000002,
    SoundTrigger                 = 0x0000001d,

    CoarseLocationUpdate         = 0x0000ff06,
    AttachedSound                = 0x0000ff0d,
    PreloadSound                 = 0x0000ff0f,
    ViewerEffect                 = 0x0000ff11,

    Wrapper                      = 0xffff0001,
    UseCircuitCode               = 0xffff0003,
    AgentThrottle                = 0xffff0051,
    AgentHeightWidth             = 0xffff0053,
    HealthMessage                = 0xffff008a,
    RegionHandshake              = 0xffff0094,
    RegionHandshakeReply         = 0xffff0095,
    SimulatorViewerTimeMessage   = 0xffff0096,
    ScriptControlChange          = 0xffff00bd,
    CompleteAgentMovement        = 0xffff00f9,
    AgentMovementCompleteMessage = 0xffff00fa,
    AgentDataUpdate              = 0xffff0183,
    PacketAck                    = 0xfffffffb,
    OpenCircuit                  = 0xfffffffc
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
    /// <summary>
    /// MTU - The largest total size of a packet.
    /// </summary>
    public static readonly int MaximumTranferUnit = 1200;

    public PacketFlags Flags { get; set; }
    public UInt32 SequenceNumber { get; set; }
    public byte[] ExtraHeader { get; set; }


    public string Name { get; set; }
    public MessageFrequency Frequency { get; set; }
    public MessageId Id { get; set; }
    public MessageTrustLevel TrustLevel { get; set; }

    public List<UInt32> Acks { get; set; } = new List<UInt32>();

    public void AddAck(UInt32 sequenceNumber)
    {
        if (Acks.Count >= 255)
        {
            throw new Exception("Messasge: Too many acks in a single message. Max is 255.");
        }
        Acks.Add(sequenceNumber);
    }

    public virtual int GetSerializedLength()
    {
        int size = 1                           // Flags
                 + 4                           // SequenceNumber
                 + 1                           // Extra header length
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

        if (Acks.Count != 0)
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

        if (Acks.Count != 0)
        {
            Flags |= PacketFlags.Ack;
        }

        buffer[o++] = (byte)Flags;
        o = BinarySerializer.Serialize_Be(SequenceNumber, buffer, o, length);
        //buffer[o++] = (byte)(SequenceNumber >> 24);
        //buffer[o++] = (byte)(SequenceNumber >> 16);
        //buffer[o++] = (byte)(SequenceNumber >> 8);
        //buffer[o++] = (byte)(SequenceNumber >> 0);
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

        // Acks come at the end of the buffer. WARNING: If the buffer is too small, acks will be overwritten!
        BinarySerializer.SerializeAcks(Acks, buffer);

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
