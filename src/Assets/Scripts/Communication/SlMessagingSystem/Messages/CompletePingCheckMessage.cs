public class CompletePingCheckMessage : Message
{
    public byte PingId { get; set; }
    public CompletePingCheckMessage(byte pingId)
    {
        Id = MessageId.CompletePingCheck;
        Flags = 0;
        Frequency = MessageFrequency.High;

        PingId = pingId;
    }

    public override int GetSerializedLength()
    {
        return base.GetSerializedLength()
               + 1;  // PingId
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);

        buffer[o++] = PingId;

        return o - offset;
    }
}
