using System;

public class AgentDataUpdateMessage : Message
{
    public Guid AgentId { get; set; }

    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public string GroupTitle { get; set; }

    public Guid ActiveGroupId { get; set; }

    public UInt64 GroupPowers { get; set; }

    public string GroupName { get; set; }

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public AgentDataUpdateMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }

    public override string ToString()
    {
        return $"{base.ToString()}: Name={FirstName} {LastName}, Group=\"{GroupName}\", Title=\"{GroupTitle}\"";
    }
}
