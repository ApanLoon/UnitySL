using System;
using UnityEngine;

public enum ChatSourceType : byte
{
    System  = 0,
    Agent   = 1,
    Object  = 2,
    Unknown = 3
}

public enum ChatType : byte
{
    Whisper      = 0,
    Normal       = 1,
    Shout        = 2,
    Start        = 4,
    Stop         = 5,
    DebugMessage = 6,
    Region       = 7,
    Owner        = 8,
    Direct       = 9		// From llRegionSayTo()
}

public enum ChatAudibleLevel : sbyte
{
    Not    = -1,
    Barely = 0,
    Fully  = 1
}

public class ChatFromSimulatorMessage : Message
{
    public string FromName { get; set; }
    public Guid SourceId { get; set; }
    public Guid OwnerId { get; set; }
    public ChatSourceType SourceType { get; set; }
    public ChatType ChatType { get; set; }
    public ChatAudibleLevel AudibleLevel { get; set; }
    public Vector3 Position { get; set; }
    public string Message { get; set; }

    public ChatFromSimulatorMessage()
    {
        MessageId = MessageId.ChatFromSimulator;
        Flags = 0;
    }

    #region DeSerialise
    protected override void DeSerialise(byte[] buf, ref int o, int length)
    {
        FromName     = BinarySerializer.DeSerializeString(buf, ref o, length, 1);
        SourceId     = BinarySerializer.DeSerializeGuid(buf, ref o, length);
        OwnerId      = BinarySerializer.DeSerializeGuid(buf, ref o, length);
        SourceType   = (ChatSourceType)buf[o++];
        ChatType     = (ChatType)buf[o++];
        AudibleLevel = (ChatAudibleLevel)buf[o++];
        Position     = BinarySerializer.DeSerializeVector3(buf, ref o, length);
        Message      = BinarySerializer.DeSerializeString(buf, ref o, length, 2);

        Logger.LogDebug (ToString());
    }
    #endregion DeSerialise

    public override string ToString()
    {
        return $"{base.ToString()}: FromName={FromName}, SourceId={SourceId}, OwnerId={OwnerId}, SourceType={SourceType}, ChatType={ChatType}, AudibleLevel={AudibleLevel}, Position={Position}, Message={Message}";
    }
}
