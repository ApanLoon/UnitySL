using System;
using UnityEngine;

public class SimulatorViewerTimeMessage : Message
{
    /// <summary>
    /// Micro seconds since start
    /// </summary>
    public UInt64 UsecSinceStart { get; set; }
    public UInt32 SecPerDay { get; set; }
    public UInt32 SecPerYear { get; set; }
    public Vector3 SunDirection { get; set; }
    public float SunPhase { get; set; }
    public Vector3 SunAngVelocity { get; set; }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public SimulatorViewerTimeMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
