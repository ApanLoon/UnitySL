using System;

public class SlMessageSystem
{
    public static SlMessageSystem Instance = new SlMessageSystem();

    public SlMessageSystem()
    {
    }
    public SlMessageSystem (
        string filename,
        UInt32 port,
        Int32 systemVersionMajor,
        Int32 systemVersionMinor,
        Int32 systemVersionPatch,
        bool failure_is_fatal,
        float circuit_heartbeat_interval,
        float circuit_timeout)
    {
        SystemVersionMajor = systemVersionMajor;
        SystemVersionMinor = systemVersionMinor;
        SystemVersionPatch = systemVersionPatch;

        //LoadTemplateFile(filename, failure_is_fatal);
    }

    /// <summary>
    /// Local UDP port number
    /// </summary>
    public int Port { get; protected set; } = 20000;

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

}
