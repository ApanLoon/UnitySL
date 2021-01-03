using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class Circuit : IDisposable
{
    public static readonly float AckTimeOut = 2f;

    public Circuit(IPAddress address, int port, SlMessageSystem messageSystem, float heartBeatInterval, float circuitTimeout)
    {
        RemoteEndPoint = new IPEndPoint(address, port);
        Address = address;
        Port = port;
        MessageSystem = messageSystem; 
        HeartBeatInterval = heartBeatInterval;
        CircuitTimeout = circuitTimeout;

        Start();
    }

    #region Thread
    public void Start()
    {
        if (_threadLoopTask != null && _threadLoopTask.Status == TaskStatus.Running)
        {
            Logger.LogDebug("Circuit.Start: Already started.");
            return;
        }
        Logger.LogDebug($"Circuit.Start: Address={Address}, Port={Port}");

        _cts = new CancellationTokenSource();
        _threadLoopTask = Task.Run(() => ThreadLoop(_cts.Token), _cts.Token);
    }

    public void Stop()
    {
        Logger.LogDebug($"Circuit.Stop: Address={Address}, Port={Port}");
        _cts.Cancel();

        _cts.Dispose();
    }

    private CancellationTokenSource _cts;
    private Task _threadLoopTask;

    protected async Task ThreadLoop(CancellationToken ct)
    {
        Logger.LogInfo($"Circuit.ThreadLoop: Running Address={Address}, Port={Port}");

        LastSendTime = DateTime.Now; // Pretend that we sent something to prevent initial keep-alive.
        while (ct.IsCancellationRequested == false)
        {
            DateTime now = DateTime.Now;
            if (now.Subtract(LastSendTime).TotalSeconds >= AckTimeOut)
            {
                if (WaitingForOutboundAck.Count > 0)
                {
                    int n = 0;
                    PacketAckMessage ackMessage = new PacketAckMessage();
                    while (WaitingForOutboundAck.Count > 0 && n < 255)
                    {
                        ackMessage.AddPacketAck(WaitingForOutboundAck.Dequeue());
                        n++;
                    }
                    await Send(ackMessage);
                }
                else
                {
                    // TODO: Should I send something else as a keep-alive message?
                    LastSendTime = now;
                }
            }
            await Task.Delay(10, ct); // tune for your situation, can usually be omitted
        }
        // Cancelling appears to kill the task immediately without giving it a chance to get here
        Logger.LogInfo($"Circuit.ThreadLoop: Stopping... Address={Address}, Port={Port}");
    }
    #endregion Thread

    protected DateTime LastSendTime;

    public IPEndPoint RemoteEndPoint { get; set; }
    public IPAddress Address { get; }
    public int Port { get; }
    public SlMessageSystem MessageSystem { get; }
    public float HeartBeatInterval { get; }
    public float CircuitTimeout { get; }

    public UInt32 LastSequenceNumber { get; protected set; }

    public HashSet<UInt32> WaitingForInboundAck = new HashSet<UInt32>();
    public Queue<UInt32> WaitingForOutboundAck = new Queue<UInt32>();

    public async Task SendUseCircuitCode(UInt32 circuitCode, Guid sessionId, Guid agentId)
    {
        Logger.LogDebug($"Circuit.SendUseCircuitCode({circuitCode:x8}, {sessionId}, {agentId}): Sending to {Address}:{Port}");

        UseCircuitCodeMessage message = new UseCircuitCodeMessage(circuitCode, sessionId, agentId);
        await SendReliable(message);
    }

    public async Task SendCompleteAgentMovement(Guid agentId, Guid sessionId, UInt32 circuitCode)
    {
        Logger.LogDebug($"Circuit.SendCompleteAgentMovement({agentId}, {sessionId}, {circuitCode:x8}): Sending to {Address}:{Port}");

        CompleteAgentMovementMessage message = new CompleteAgentMovementMessage(agentId, sessionId, circuitCode);
        await SendReliable(message);
    }

    public async Task SendRegionHandshakeReply(Guid agentId, Guid sessionId, UInt32 flags)
    {
        Logger.LogDebug($"Circuit.SendRegionHandshakeReply({agentId}, {sessionId}, {flags:x8}): Sending to {Address}:{Port}");

        RegionHandshakeReplyMessage message = new RegionHandshakeReplyMessage(agentId, sessionId, flags);
        await Send(message);
    }


    //public void SendOpenCircuit()
    //{
    //    Logger.LogDebug($"Circuit.SendOpenCircuit: Sending to {Address}:{Port}");

    //    OpenCircuitMessage message = new OpenCircuitMessage(Address, Port);
    //    SlMessageSystem.Instance.EnqueueMessage(this, message);
    //}

    protected async Task SendReliable(Message message)
    {
        WaitingForInboundAck.Add(message.SequenceNumber);
        await Send(message);
        await Ack(message.SequenceNumber);
    }

    protected async Task Send(Message message)
    {
        message.SequenceNumber = ++LastSequenceNumber;

        int len = message.GetSerializedLength();
        int nAcks = WaitingForOutboundAck.Count;
        
        // If there is room in the packet and there are SequenceNumbers in WaitingForOutboundAck, add as many as possible
        if (nAcks > 0 && len < Message.MaximumTranferUnit - 5 && message is PacketAckMessage == false)
        {
            // At least one Ack fits, calculate how many:
            nAcks = Math.Min(nAcks, (Message.MaximumTranferUnit - len - 1) / 4);
            for (int i = 0; i < nAcks; i++)
            {
                message.AddAck(WaitingForOutboundAck.Dequeue());
            }
        }

        byte[] buffer = new byte[message.GetSerializedLength()];
        message.Serialize(buffer, 0, buffer.Length);
        
        SlMessageSystem.Instance.EnqueueMessage(this, buffer);
        LastSendTime = DateTime.Now;
    }

    protected async Task Ack(UInt32 sequenceNumber, int frequency = 10, int timeout = 1000)
    {
        var waitTask = Task.Run(async () =>
        {
            while (WaitingForInboundAck.Contains(sequenceNumber)) await Task.Delay(frequency);
        });

        if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
        {
            throw new TimeoutException();
        }
    }
    
    public void ReceiveData(byte[] buf)
    {
        Message message = BinarySerializer.DeSerializeMessage(buf, 0);
        if (message == null)
        {
            return;
        }

        Logger.LogInfo($"Circuit.ReceiveData: {message}");

        switch (message)
        {
            case PacketAckMessage packetAckMessage:
                foreach (UInt32 ack in packetAckMessage.PacketAcks)
                {
                    if (WaitingForInboundAck.Contains(ack))
                    {
                        WaitingForInboundAck.Remove(ack);
                    }
                }
                break;

            case AgentDataUpdateMessage agentDataUpdateMessage:
                EventManager.Instance.RaiseOnAgentDataUpdateMessage(agentDataUpdateMessage);
                break;

            case AgentMovementCompleteMessage agentMovementCompleteMessage:
                EventManager.Instance.RaiseOnAgentMovementCompleteMessage(agentMovementCompleteMessage);
                break;

            case RegionHandshakeMessage regionHandshakeMessage:
                EventManager.Instance.RaiseOnRegionHandshakeMessage(regionHandshakeMessage);
                break;
        }

        if ((message.Flags & PacketFlags.Reliable) != 0)
        {
            WaitingForOutboundAck.Enqueue(message.SequenceNumber);
        }

        // Appended acks:
        if ((message.Flags & PacketFlags.Ack) == 0)
        {
            return;
        }
        foreach (UInt32 ack in message.Acks)
        {
            if (WaitingForInboundAck.Contains(ack))
            {
                WaitingForInboundAck.Remove(ack);
            }
        }
    }

    public void Dispose()
    {
        Logger.LogDebug($"Circuit.Dispose: Address={Address}, Port={Port}");
        Stop();
    }
}
