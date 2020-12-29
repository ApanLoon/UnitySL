using System;
using System.Net;
using System.Net.Sockets;

public class OpenCircuitMessage : Message
{
    public IPAddress Address { get; set; }
    public int Port { get; set; }

    public OpenCircuitMessage(IPAddress address, int port)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new Exception("OpenCircuitMessage.Constructor: Only IP v4 is supported.");
        }

        Id = MessageId.OpenCircuit;
        Flags = PacketFlags.Reliable;
        Frequency = MessageFrequency.Fixed;
        Address = address;
        Port = port;
    }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public OpenCircuitMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }

    public override int GetSerializedLength()
    {
        return base.GetSerializedLength()
               + 4  // Address
               + 2; // Port
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);
        if (Address.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new Exception("OpenCircuitMessage.Encode: Only IP v4 is supported.");
        }

        byte[] address = Address.GetAddressBytes();
        buffer[o++] = address[0];
        buffer[o++] = address[1];
        buffer[o++] = address[2];
        buffer[o++] = address[3];

        // Little endian
        buffer[o++] = (byte)(Port >> 0);
        buffer[o++] = (byte)(Port >> 8);

        return o - offset;
    }
}