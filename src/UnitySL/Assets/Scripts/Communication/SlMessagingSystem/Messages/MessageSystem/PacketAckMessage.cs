using System;
using System.Collections.Generic;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem
{
    public class PacketAckMessage : Message
    {
        public List<UInt32> PacketAcks { get; set; } = new List<uint>();

        public PacketAckMessage()
        {
            MessageId = MessageId.PacketAck;
            Flags = 0;
        }

        public void AddPacketAck(UInt32 sequenceNumber)
        {
            if (PacketAcks.Count >= 255)
            {
                throw new Exception("PacketAckMessasge: Too many acks in a single message. Max is 255.");
            }
            PacketAcks.Add (sequenceNumber);
        }

        #region Serialise
        public override int GetSerializedLength()
        {
            return base.GetSerializedLength()
                   + 1 // Count
                   + 4 * PacketAcks.Count;
        }
        public override int Serialize(byte[] buffer, int offset, int length)
        {
            int o = offset;
            o += base.Serialize (buffer, offset, length);

            buffer[o++] = (byte)PacketAcks.Count;
            foreach (UInt32 sequenceNumber in PacketAcks)
            {
                o = BinarySerializer.Serialize_Le(sequenceNumber, buffer, o, length);
            }
            return o - offset;
        }
        #endregion Serialise

        #region DeSerialise
        protected override void DeSerialise (byte[] buf, ref int offset, int length)
        {
            byte nAcks = buf[offset++];
            for (int i = 0; i < nAcks; i++)
            {
                UInt32 ack = BinarySerializer.DeSerializeUInt32_Le (buf, ref offset, buf.Length);

                PacketAcks.Add(ack);
            }
        }
        #endregion DeSerialise
    }
}