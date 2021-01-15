public class HealthMessage : Message
{
    public float Health { get; set; }

    public HealthMessage()
    {
        Id = MessageId.HealthMessage;
        Flags = 0;
        Frequency = MessageFrequency.Low;
    }

    #region DeSerialise
    protected override void DeSerialise(byte[] buf, ref int o, int length)
    {
        Health = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
    }
    #endregion DeSerialise

    public override string ToString()
    {
        return $"{base.ToString()}: Health={Health}";
    }
}
