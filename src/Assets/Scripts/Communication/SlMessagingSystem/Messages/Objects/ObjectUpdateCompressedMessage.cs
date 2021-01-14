using System;

public class ObjectUpdateCompressedMessage : ObjectUpdateMessage
{
    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public ObjectUpdateCompressedMessage (PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
        : base (flags, sequenceNumber, extraHeader, frequency, id)
    {
        UpdateType = ObjectUpdateType.OUT_FULL_COMPRESSED;
    }
}