using System;
using System.Collections.Generic;

public class ControlsChange
{
    public bool TakeControls { get; set; }
    public AgentControlFlags ControlFlags { get; set; }
    public bool PassToAgent { get; set; }
}

public class ScriptControlChangeMessage : Message
{
    public List<ControlsChange> Controls { get; set; } = new List<ControlsChange>();

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public ScriptControlChangeMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
