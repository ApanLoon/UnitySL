using System;
using System.Collections.Generic;

public class LogoutReplyMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public List<Guid> InventoryItems { get; set; } = new List<Guid>();

    public LogoutReplyMessage()
    {
        Id = MessageId.LogoutRequest;
        Flags = 0;
        Frequency = MessageFrequency.Low;
    }

    #region DeSerialise
    protected override void DeSerialise(byte[] buf, ref int o, int length)
    {
        AgentId                  = BinarySerializer.DeSerializeGuid (buf, ref o, length);
        SessionId                = BinarySerializer.DeSerializeGuid (buf, ref o, length);
        byte nItems              = buf[o++];
        for (byte i = 0; i < nItems; i++)
        {
            InventoryItems.Add(BinarySerializer.DeSerializeGuid (buf, ref o, length));
        }
        Logger.LogDebug(ToString());
    }
    #endregion DeSerialise

    public override string ToString()
    {
        string s = $"{base.ToString()}: AgentId={AgentId}, SessionId={SessionId}";
        foreach (Guid item in InventoryItems)
        {
            s += $"\n    ItemId={item}";
        }
        return s;
    }
}
