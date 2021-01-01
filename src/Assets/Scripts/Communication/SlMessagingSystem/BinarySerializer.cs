
using System;
using System.Collections.Generic;
using System.Net;

public static class BinarySerializer
{
    #region Message
    public static Message DeSerializeMessage(byte[] buf, int offset)
    {
        if (buf.Length - offset < 6)
        {
            throw new Exception("BinarySerializer.DeSerialize(Message): Not enough room in buffer.");
        }

        int o = offset;
        PacketFlags flags = (PacketFlags)buf[o++];
        UInt32 sequenceNumber = (((UInt32)buf[o++]) << 24)
                              + (((UInt32)buf[o++]) << 16)
                              + (((UInt32)buf[o++]) << 8)
                              + (((UInt32)buf[o++]) << 0);
        byte extraHeaderLength = buf[o++];
        byte[] extraHeader = new byte[extraHeaderLength];
        for (int i = 0; i < extraHeaderLength; i++)
        {
            extraHeader[i] = buf[o++];
        }

        MessageFrequency frequency = MessageFrequency.High;
        UInt32 id = buf[o++];
        if (id == 0xff)
        {
            id = (id << 8) + buf[o++];
            frequency = MessageFrequency.Medium;

            if (id == 0xffff)
            {
                id = id << 16;
                id += ((UInt32)buf[o++]) << 8;
                id += ((UInt32)buf[o++]) << 0;
                frequency = id < 0xfffffffa ? MessageFrequency.Low : MessageFrequency.Fixed;
            }
        }

        List<UInt32> acks = new List<UInt32>();
        int ackLength = 0;
        if ((flags & PacketFlags.Ack) != 0)
        {
            byte nAcks = buf[buf.Length - 1];
            ackLength = nAcks * 4 + 1;
            int ackOffset = buf.Length - ackLength;
            for (int i = 0; i < nAcks; i++)
            {
                UInt32 ack = DeSerializeUInt32_Le(buf, ref ackOffset, buf.Length);
                acks.Add(ack);
            }
        }

        if (Enum.IsDefined(typeof(MessageId), id) == false)
        {
            string idString = "";
            switch (frequency)
            {
                case MessageFrequency.High:
                case MessageFrequency.Medium:
                    idString = $"{frequency} {id & 0xff} (0x{id:x8})";
                    break;

                case MessageFrequency.Low:
                    idString = $"{frequency} {id & 0xffff} (0x{id:x8})";
                    break;

                case MessageFrequency.Fixed:
                    idString = $"{frequency} 0x{id:x8}";
                    break;
            }

            Logger.LogError($"BinarySerializer.DeSerializeMessage: Unknown message id {idString}");
            return null;
        }
        MessageId messageId = (MessageId)id;
        if (MessageDeSerializers.ContainsKey(messageId) == false)
        {
            Logger.LogError($"BinarySerializer.DeSerializeMessage: No de-serializer for message id ({messageId})");
            return null;
        }

        DeSerializerResult r = MessageDeSerializers[messageId](buf, o, buf.Length - ackLength, flags, sequenceNumber, extraHeader, frequency, messageId);
        o = r.Offset;

        if (r.Message != null && acks.Count != 0)
        {
            r.Message.Acks = acks;
            o = SerializeAcks(acks, buf, o, buf.Length - o);
        }

        return r.Message;
    }

    public class DeSerializerResult
    {
        public int Offset { get; set; }
        public Message Message { get; set; }
    }

    public static Dictionary<MessageId, Func<byte[], int, int, PacketFlags, UInt32, byte[], MessageFrequency, MessageId, DeSerializerResult>> MessageDeSerializers = new Dictionary<MessageId, Func<byte[], int, int, PacketFlags, uint, byte[], MessageFrequency, MessageId, DeSerializerResult>>()
    {
        {
            MessageId.UseCircuitCode, // 0xffff0003
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                UseCircuitCodeMessage m = new UseCircuitCodeMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                m.CircuitCode = DeSerializeUInt32_Le (buf, ref o, buf.Length);
                Guid guid;
                o = DeSerialize(out guid, buf, o, length); m.SessionId = guid;
                o = DeSerialize(out guid, buf, o, length); m.AgentId = guid;

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.PacketAck, // 0xfffffffb
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                PacketAckMessage m = new PacketAckMessage (flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                byte nAcks = buf[o++];
                for (int i = 0; i < nAcks; i++)
                {
                    UInt32 ack = DeSerializeUInt32_Le (buf, ref o, buf.Length);

                    m.PacketAcks.Add(ack);
                }

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.OpenCircuit, // 0xfffffffc
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                OpenCircuitMessage m = new OpenCircuitMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                string s = $"{buf[o++]}.{buf[o++]}.{buf[o++]}.{buf[o++]}";
                m.Address = IPAddress.Parse(s);
                m.Port = buf[o++] << 8 + buf[o++];

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        }

    };

    #endregion Messages

    #region BasicTypes
    public static int GetSerializedLength(UInt16 v)
    {
        return 2;
    }
    public static int Serialize(UInt16 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        // Little endian
        buffer[o++] = (byte)(v >> 0);
        buffer[o++] = (byte)(v >> 8);
        return o;
    }

    #region UInt32
    public static int GetSerializedLength(UInt32 v)
    {
        return 4;
    }
    public static int Serialize(UInt32 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        // Little endian
        buffer[o++] = (byte)(v >> 0);
        buffer[o++] = (byte)(v >> 8);
        buffer[o++] = (byte)(v >> 16);
        buffer[o++] = (byte)(v >> 24);
        return o;
    }

    public static UInt32 DeSerializeUInt32_Le(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt32_Le: Not enough bytes in buffer.");
        }

        return   ((UInt32) buffer[offset++] << 0)
               + ((UInt32) buffer[offset++] << 8)
               + ((UInt32) buffer[offset++] << 16)
               + ((UInt32) buffer[offset++] << 24);
    }
    #endregion UInt32

    public static int GetSerializedLength(Guid v)
    {
        return 16;
    }
    public static int Serialize(Guid guid, byte[] buffer, int offset, int length)
    {
        int o = offset;
        byte[] buf = guid.ToByteArray();
        // Weird order
        buffer[o++] = buf[3];
        buffer[o++] = buf[2];
        buffer[o++] = buf[1];
        buffer[o++] = buf[0];

        buffer[o++] = buf[5];
        buffer[o++] = buf[4];

        buffer[o++] = buf[7];
        buffer[o++] = buf[6];

        buffer[o++] = buf[8];
        buffer[o++] = buf[9];

        buffer[o++] = buf[10];
        buffer[o++] = buf[11];
        buffer[o++] = buf[12];
        buffer[o++] = buf[13];
        buffer[o++] = buf[14];
        buffer[o++] = buf[15];
        return o;
    }

    public static int DeSerialize(out Guid guid, byte[] buffer, int offset, int length)
    {
        int o = offset;
        byte[] buf = new byte[16];
        // Weird order
        buf[3]  = buffer[o++];
        buf[2]  = buffer[o++];
        buf[1]  = buffer[o++];
        buf[0]  = buffer[o++];

        buf[5]  = buffer[o++];
        buf[4]  = buffer[o++];

        buf[7]  = buffer[o++];
        buf[6]  = buffer[o++];

        buf[8]  = buffer[o++];
        buf[9]  = buffer[o++];

        buf[10] = buffer[o++];
        buf[11] = buffer[o++];
        buf[12] = buffer[o++];
        buf[13] = buffer[o++];
        buf[14] = buffer[o++];
        buf[15] = buffer[o++];

        guid = new Guid(buf);
        return o;
    }

    public static int SerializeAcks(List<UInt32> acks, byte[] buffer, int offset, int length)
    {
        int o = offset;

        int i = acks.Count;
        if (i > 255)
        {
            throw new ArgumentOutOfRangeException("BinarySerializer.SerializeAcks: Too many acks in list. Max is 255.");
        }
        byte nAcks = (byte)i;
        for (i = 0; i < nAcks; i++)
        {
            UInt32 ack = acks[i];
            buffer[o++] = (byte)(ack >> 0);
            buffer[o++] = (byte)(ack >> 8);
            buffer[o++] = (byte)(ack >> 16);
            buffer[o++] = (byte)(ack >> 24);
        }

        buffer[o++] = nAcks;
        return o;
    }

    #endregion BasicTypes
}
