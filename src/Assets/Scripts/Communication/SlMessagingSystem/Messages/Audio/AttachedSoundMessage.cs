using System;

[Flags]
public enum SoundFlags : byte
{
    None        = 0x00,
    Loop        = 0x01,
    SyncMaster  = 0x02,
    SyncSlave   = 0x04,
    SyncPending = 0x08,
    Queue       = 0x10,
    Stop        = 0x20,
    SyncMask    = SyncMaster | SyncSlave | SyncPending
}

public class AttachedSoundMessage : Message
{
    public Guid SoundId { get; set; }
    public Guid ObjectId { get; set; }
    public Guid OwnerId { get; set; }
    public float Gain { get; set; }
    public SoundFlags SoundFlags { get; set; }

    public AttachedSoundMessage()
    {
        Id = MessageId.AttachedSound;
        Flags = 0;
        Frequency = MessageFrequency.Medium;
    }

    #region DeSerialise
    protected override void DeSerialise(byte[] buf, ref int o, int length)
    {
        SoundId  = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
        ObjectId = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
        OwnerId  = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
        Gain     = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, buf.Length);
        SoundFlags = (SoundFlags)buf[o++];
    }
    #endregion DeSerialise

    public override string ToString()
    {
        return $"{base.ToString()}: SoundId={SoundId}, ObjectId={ObjectId}, OwnerId={OwnerId}, Gain={Gain}, SoundFlags={SoundFlags}";
    }
}
