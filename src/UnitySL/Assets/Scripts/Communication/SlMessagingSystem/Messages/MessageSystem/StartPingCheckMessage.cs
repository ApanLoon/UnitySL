using System;

public class StartPingCheckMessage : Message
{
    public byte PingId { get; set; }
    public UInt32 OldestUnchecked { get; set; }

    public StartPingCheckMessage()
    {
        MessageId = MessageId.StartPingCheck;
        Flags = 0;
    }

    #region DeSerialise
    protected override void DeSerialise(byte[] buf, ref int o, int length)
    {
        PingId = buf[o++];
        OldestUnchecked = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, buf.Length);
    }
    #endregion DeSerialise
    
    public override string ToString()
    {
        return $"{base.ToString()}: PingId={PingId}, OldestUnchecked={OldestUnchecked}";
    }
}