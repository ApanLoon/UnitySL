using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

public class Circuit
{
    public Circuit(IPAddress address, int port, SlMessageSystem messageSystem, float heartBeatInterval, float circuitTimeout)
    {
        RemoteEndPoint = new IPEndPoint(address, port);
        Address = address;
        Port = port;
        MessageSystem = messageSystem; 
        HeartBeatInterval = heartBeatInterval;
        CircuitTimeout = circuitTimeout;
    }

    public IPEndPoint RemoteEndPoint { get; set; }
    public IPAddress Address { get; }
    public int Port { get; }
    public SlMessageSystem MessageSystem { get; }
    public float HeartBeatInterval { get; }
    public float CircuitTimeout { get; }

    public UInt32 LastSequenceNumber { get; protected set; }

    public HashSet<UInt32> WaitingForAck = new HashSet<UInt32>();

    public async Task SendUseCircuitCode(UInt32 circuitCode, Guid sessionId, Guid agentId)
    {
        Logger.LogDebug($"Circuit.SendUseCircuitCode({circuitCode:x8}, {sessionId}, {agentId}): Sending to {Address}:{Port}");

        UseCircuitCodeMessage message = new UseCircuitCodeMessage(circuitCode, sessionId, agentId)
        {
            SequenceNumber = ++LastSequenceNumber
        };
        await SendReliable(message);
    }

    public async Task SendCompleteAgentMovement(Guid agentId, Guid sessionId, UInt32 circuitCode)
    {
        Logger.LogDebug($"Circuit.SendCompleteAgentMovement({agentId}, {sessionId}, {circuitCode:x8}): Sending to {Address}:{Port}");

        CompleteAgentMovementMessage message = new CompleteAgentMovementMessage(agentId, sessionId, circuitCode)
        {
            SequenceNumber = ++LastSequenceNumber
        };
        await SendReliable(message);
    }


    //public void SendOpenCircuit()
    //{
    //    Logger.LogDebug($"Circuit.SendOpenCircuit: Sending to {Address}:{Port}");

    //    OpenCircuitMessage message = new OpenCircuitMessage(Address, Port)
    //    {
    //        SequenceNumber = ++LastSequenceNumber
    //    };
    //    SlMessageSystem.Instance.EnqueueMessage(this, message);
    //}

    protected async Task SendReliable(Message message)
    {
        WaitingForAck.Add(message.SequenceNumber);
        SlMessageSystem.Instance.EnqueueMessage(this, message);
        await Ack(message.SequenceNumber);
    }

    protected async Task Ack(UInt32 sequenceNumber, int frequency = 10, int timeout = 1000)
    {
        var waitTask = Task.Run(async () =>
        {
            while (WaitingForAck.Contains(sequenceNumber)) await Task.Delay(frequency);
        });

        if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
        {
            throw new TimeoutException();
        }
    }
    
    public void ReceiveData(byte[] buf)
    {
        Logger.LogDebug("Circuit.ReceiveData");
        Message message = BinarySerializer.DeSerializeMessage(buf, 0);
        Logger.LogDebug($"Circuit.ReceiveData: {message}");

        switch (message)
        {
            case PacketAckMessage packetAckMessage:
                foreach (UInt32 ack in packetAckMessage.PacketAcks)
                {
                    if (WaitingForAck.Contains(ack))
                    {
                        WaitingForAck.Remove(ack);
                    }
                }
                break;
        }

        if (message != null && (message.Flags & PacketFlags.Ack) != 0)
        {
            foreach (UInt32 ack in message.Acks)
            {
                if (WaitingForAck.Contains(ack))
                {
                    WaitingForAck.Remove(ack);
                }
            }
        }
    }
}
