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

        MessageId = MessageId.OpenCircuit;
        Flags = PacketFlags.Reliable;
        Address = address;
        Port = port;
    }

    #region Serialise
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
    #endregion Serialise

    public override string ToString()
    {
        return $"{base.ToString()}: Address={Address}, Port=\"{Port}\"";
    }
}