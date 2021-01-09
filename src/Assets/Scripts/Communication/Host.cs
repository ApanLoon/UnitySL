using System;
using System.Net;

public class Host
{
    public int Port { get; set; }
    public IPAddress Address { get; set; }
    public string UntrustedSimCap { get; set; }

    public IPEndPoint EndPoint { get; protected set; }

    public Host()
    {
        Port = 0;
        Address = IPAddress.None;
    }

    public Host(IPAddress address, int port)
    {
        Address = address;
        Port = port;
        EndPoint = new IPEndPoint(Address, Port);
    }

    public Host(string address, int port)
    {
        Address = IPAddress.Parse(address);
        Port = port;
        EndPoint = new IPEndPoint(Address, Port);
    }

    public Host(UInt64 ipPort)
    {
        UInt32 ip = (UInt32)(ipPort >> 32);
        int port = (int)(ipPort & (UInt64)0xFFFFFFFF);
        Address = new IPAddress(ip); // TODO: Verify that this does the right thing
        Port = port;
        EndPoint = new IPEndPoint(Address, Port);
    }

    public override string ToString()
    {
        return $"{Address}:{Port}";
    }
}
