using System;
using System.Text;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.MessageLogs;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Chat
{
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
                   + BinarySerializer.GetSerializedLength(Message, 2)
                   + 1      // Type
                   + 4;     // Channel
        }
        public override int Serialize(byte[] buffer, int offset, int length)
        {
            int o = offset;
            o += base.Serialize(buffer, offset, length);

            o = BinarySerializer.Serialize(AgentId, buffer, o, length);
            o = BinarySerializer.Serialize(SessionId, buffer, o, length);
            o = BinarySerializer.Serialize(Message, buffer, o, length, 2);
            buffer[o++] = (byte)ChatType;
            o = BinarySerializer.Serialize_Le(Channel, buffer, o, length);

            return o - offset;
        }
        #endregion Serialise

        public override string ToString()
        {
            return $"{base.ToString()}: AgentId={AgentId}, SessionId={SessionId}, ChatType={ChatType}, Channel={Channel}, Message={Message}";
        }
    }
}
