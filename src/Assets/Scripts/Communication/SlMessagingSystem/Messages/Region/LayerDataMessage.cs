
using System;

public class LayerDataMessage : Message
{
    public LayerType LayerType { get; set; }
    public byte[] Data { get; set; }

    public LayerDataMessage()
    {
        Id = MessageId.LayerData;
        Flags = 0;
        Frequency = MessageFrequency.High;
    }

    #region DeSerialise
    protected override void DeSerialise(byte[] buf, ref int o, int length)
    {
        LayerType = (LayerType)buf[o++];
        UInt16 len = BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length);
        Data = new byte[len];
        Array.Copy(buf, o, Data, 0, len);
    }
    #endregion DeSerialise

    public override string ToString()
    {
        return $"{base.ToString()}: LayerType={LayerType}, Data({Data.Length})";
    }
}
