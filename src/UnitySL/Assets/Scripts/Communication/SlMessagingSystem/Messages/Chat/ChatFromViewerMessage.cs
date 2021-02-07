using System;
using System.Text;

public class ChatFromViewerMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public string Message { get; set; }
    public ChatType ChatType { get; set; }
    public Int32 Channel { get; set; }

    public ChatFromViewerMessage(Guid agentId, Guid sessionId, string message, ChatType chatType, Int32 channel)
    {
        MessageId = MessageId.ChatFromViewer;
        Flags = PacketFlags.Reliable; //TODO: Could be zerocoded

        AgentId = agentId;
        SessionId = sessionId;
        Message = message;
        ChatType = chatType;
        Channel = channel;
    }


    #region Serialise
    public override int GetSerializedLength()
    {
        return base.GetSerializedLength()
               + 16     // AgentId
               + 16     // SessionId
               + 2      // messageLength
               + Encoding.UTF8.GetByteCount(Message)
               + 1      // Type
               + 4;     // Channel
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);

        o = BinarySerializer.Serialize(AgentId, buffer, o, length);
        o = BinarySerializer.Serialize(SessionId, buffer, o, length);

        byte[] messageBytes = Encoding.UTF8.GetBytes(Message);
        UInt16 messageLength = (UInt16)messageBytes.Length;
        o = BinarySerializer.Serialize_Le(messageLength, buffer, o, length);
        Array.Copy(messageBytes, 0, buffer, o, messageLength);
        o += messageLength;

        buffer[o++] = (byte)ChatType;
        o = BinarySerializer.Serialize_Le(Channel, buffer, o, length);

        return o - offset;
    }
    #endregion Serialise
}
