using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class SlMessageSystem : IDisposable
{
    public static SlMessageSystem Instance = new SlMessageSystem();

    /// <summary>
    /// Local UDP port number
    /// </summary>
    public int Port { get; protected set; } = 13028;

    public Int32 SystemVersionMajor { get; protected set; }
    public Int32 SystemVersionMinor { get; protected set; }
    public Int32 SystemVersionPatch { get; protected set; }
    public Int32 SystemVersionServer { get; protected set; }

    /// <summary>
    /// Set this flag to TRUE when you want *very* verbose logs.
    /// </summary>
    public bool VerboseLog { get; set; } = false;

    public bool Error { get; protected set; } = false;
    public Int32 ErrorCode { get; protected set; } = 0;

    /// <summary>
    /// Does the outgoing message require a pos ack?
    /// </summary>
    public bool IsSendReliable { get; set; } = false;

    //mUnackedListDepth = 0;
    //mUnackedListSize = 0;
    //mDSMaxListDepth = 0;

    //mNumberHighFreqMessages = 0;
    //mNumberMediumFreqMessages = 0;
    //mNumberLowFreqMessages = 0;
    //mPacketsIn = mPacketsOut = 0;
    //mBytesIn = mBytesOut = 0;
    //mCompressedPacketsIn = mCompressedPacketsOut = 0;
    //mReliablePacketsIn = mReliablePacketsOut = 0;

    //mCompressedBytesIn = 0;
    //mCompressedBytesOut = 0;
    //mUncompressedBytesIn = 0;
    //mUncompressedBytesOut = 0;
    //mTotalBytesIn = 0;
    //mTotalBytesOut = 0;

    //mDroppedPackets = 0;            // total dropped packets in
    //mResentPackets = 0;             // total resent packets out
    //mFailedResendPackets = 0;       // total resend failure packets out
    //mOffCircuitPackets = 0;         // total # of off-circuit packets rejected
    //mInvalidOnCircuitPackets = 0;   // total # of on-circuit packets rejected

    //mOurCircuitCode = 0;

    //mIncomingCompressedSize = 0;
    //mCurrentRecvPacketID = 0;

    //mMessageFileVersionNumber = 0.f;

    //mTimingCallback = NULL;
    //mTimingCallbackData = NULL;

    //mMessageBuilder = NULL;

    protected Dictionary<Host,       Circuit> CircuitByHost = new Dictionary<Host, Circuit>();
    protected Dictionary<IPEndPoint, Circuit> CircuitByEndPoint = new Dictionary<IPEndPoint, Circuit>();
    protected UdpClient UdpClient;

    protected class OutgoingMessage
    {
        public Circuit Circuit { get; set; }
        public byte[] MessageBytes { get; set; }
    }

    protected class IncomingMessage
    {
        public IPEndPoint EndPoint { get; set; }
        public byte[] MessageBytes { get; set; }
    }

    protected Queue<OutgoingMessage> OutGoingMessages = new Queue<OutgoingMessage>(); // ConcurrentQueue?
    protected Queue<IncomingMessage> IncomingMessages = new Queue<IncomingMessage>(); // ConcurrentQueue?

    protected class UdpState
    {
        public UdpClient Client;
        public IPEndPoint EndPoint;
    }

    public void Start()
    {
        if (_threadLoopTask != null && _threadLoopTask.Status == TaskStatus.Running)
        {
            Logger.LogDebug("SlMessageSystem.Start: Already started.");
            return;
        }
        Logger.LogDebug("SlMessageSystem.Start");

        _cts = new CancellationTokenSource();
        _threadLoopTask = Task.Run(() => ThreadLoop(_cts.Token), _cts.Token);
    }

    public void Stop()
    {
        Logger.LogDebug("SlMessageSystem.Stop");
        _cts.Cancel();

        foreach (Circuit circuit in CircuitByEndPoint.Values)
        {
            circuit.Stop();
        }

        _cts.Dispose();
        UdpClient.Close();
        UdpClient?.Dispose();
    }

    private CancellationTokenSource _cts;
    private Task _threadLoopTask;

    protected async Task ThreadLoop(CancellationToken ct)
    {
        UdpState state = new UdpState();
        state.EndPoint = new IPEndPoint(IPAddress.Any, Port);
        state.Client = new UdpClient(state.EndPoint);
        UdpClient = state.Client;
        Logger.LogInfo("SlMessageSystem.ThreadLoop: Running");

        UdpClient.BeginReceive(ReceiveData, state);
        while (ct.IsCancellationRequested == false)
        {
            try
            {
                if (OutGoingMessages.Count > 0)
                {
                    OutgoingMessage om = null;
                    lock (OutGoingMessages)
                    {
                        om = OutGoingMessages.Dequeue();
                    }

                    if (om != null && om.MessageBytes != null && om.Circuit != null)
                    {
                        await Send(om.MessageBytes, om.Circuit);
                    }
                }

                IncomingMessage im = null;
                lock (IncomingMessages)
                {
                    if (IncomingMessages.Count > 0)
                    {
                        im = IncomingMessages.Dequeue();
                    }
                }

                if (im != null && CircuitByEndPoint.ContainsKey(im.EndPoint))
                {
                    CircuitByEndPoint[im.EndPoint].ReceiveData(im.MessageBytes);
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"SlMessageSystem.ThreadLoop: {e}");
            }

            await Task.Delay(10, ct); // tune for your situation, can usually be omitted
        }
        // Cancelling appears to kill the task immediately without giving it a chance to get here
        Logger.LogInfo($"SlMessageSystem.ThreadLoop: Stopping...");
        UdpClient.Close();
        UdpClient?.Dispose();
    }

    public Circuit EnableCircuit(Host host, float heartBeatInterval = 5f, float circuitTimeout = 100f)
    {
        Logger.LogDebug("SlMessageSystem.EnableCircuit");

        if (CircuitByHost.ContainsKey(host))
        {
            return CircuitByHost[host];
        }

        Circuit circuit = new Circuit(host, this, heartBeatInterval, circuitTimeout);
        CircuitByHost[host] = circuit;
        CircuitByEndPoint[host.EndPoint] = circuit;
        return circuit;
    }

    public void EnqueueMessage(Circuit circuit, byte[] messageBytes)
    {
        lock (OutGoingMessages)
        {
            OutGoingMessages.Enqueue(new OutgoingMessage(){Circuit = circuit, MessageBytes = messageBytes });
        }
    }


    protected async Task Send(byte[] buffer, Circuit circuit)
    {
        //Logger.LogDebug($"SlMessageSystem.Send: Sending {buffer.Length} bytes...");
        await UdpClient.SendAsync(buffer, buffer.Length, circuit.Host.EndPoint);
    }
    
    protected void ReceiveData(IAsyncResult ar)
    {
        try
        {
            UdpState state = (UdpState) ar.AsyncState;
            IPEndPoint endPoint = null;
            byte[] buf = state.Client.EndReceive(ar, ref endPoint);
            state.Client.BeginReceive(ReceiveData, state);

            lock (IncomingMessages)
            {
                IncomingMessages.Enqueue(new IncomingMessage{EndPoint = endPoint, MessageBytes = buf});
            }
        }
        catch (ObjectDisposedException)
        {
            return;
        }
        catch (Exception e)
        {
            Logger.LogError($"SlMessageSystem.ReceiveData: {e}");
        }
    }

    public void Dispose()
    {
        Logger.LogDebug("SlMessagingSystem.Dispose");
        Stop();
    }
}
