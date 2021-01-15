
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class BinarySerializer
{

    #region BasicTypes

    #region UInt16
    public static int GetSerializedLength(UInt16 v)
    {
        return 2;
    }
    public static int Serialize_Le(UInt16 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >> 0);
        buffer[o++] = (byte)(v >> 8);
        return o;
    }
    public static int Serialize_Be(UInt16 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >> 8);
        buffer[o++] = (byte)(v >> 0);
        return o;
    }

    public static UInt16 DeSerializeUInt16_Be(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 2)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt16_Be: Not enough bytes in buffer.");
        }

        return (UInt16)((buffer[offset++] << 8)
                      + (buffer[offset++] << 0));
    }
    public static UInt16 DeSerializeUInt16_Le(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 2)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt16_Le: Not enough bytes in buffer.");
        }

        return (UInt16)((buffer[offset++] << 0)
                        + (buffer[offset++] << 8));
    }
    #endregion UInt16

    #region Int16
    public static int GetSerializedLength(Int16 v)
    {
        return 2;
    }
    public static int Serialize_Le(Int16 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >> 0);
        buffer[o++] = (byte)(v >> 8);
        return o;
    }
    public static int Serialize_Be(Int16 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >> 8);
        buffer[o++] = (byte)(v >> 0);
        return o;
    }

    public static Int16 DeSerializeInt16_Be(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 2)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeInt16_Be: Not enough bytes in buffer.");
        }

        return (Int16)((buffer[offset++] << 8)
                     + (buffer[offset++] << 0));
    }
    public static Int16 DeSerializeInt16_Le(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 2)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt16_Le: Not enough bytes in buffer.");
        }

        return (Int16)((buffer[offset++] << 0)
                     + (buffer[offset++] << 8));
    }
    #endregion Int16

    #region UInt32
    public static int GetSerializedLength(UInt32 v)
    {
        return 4;
    }
    public static int Serialize_Le(UInt32 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        // Little endian
        buffer[o++] = (byte)(v >> 0);
        buffer[o++] = (byte)(v >> 8);
        buffer[o++] = (byte)(v >> 16);
        buffer[o++] = (byte)(v >> 24);
        return o;
    }
    public static int Serialize_Be(UInt32 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >> 24);
        buffer[o++] = (byte)(v >> 16);
        buffer[o++] = (byte)(v >>  8);
        buffer[o++] = (byte)(v >>  0);
        return o;
    }

    public static UInt32 DeSerializeUInt32_Be(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt32_Be: Not enough bytes in buffer.");
        }

        return (UInt32) ((UInt32) buffer[offset++] << 24)
                      + ((UInt32) buffer[offset++] << 16)
                      + ((UInt32) buffer[offset++] <<  8)
                      + ((UInt32) buffer[offset++] <<  0);
    }
    public static UInt32 DeSerializeUInt32_Le(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt32_Le: Not enough bytes in buffer.");
        }

        return (UInt32)((UInt32)buffer[offset++] <<  0)
                     + ((UInt32)buffer[offset++] <<  8)
                     + ((UInt32)buffer[offset++] << 16)
                     + ((UInt32)buffer[offset++] << 24);
    }
    #endregion UInt32

    #region UInt64
    public static int GetSerializedLength(UInt64 v)
    {
        return 8;
    }
    public static int Serialize_Le(UInt64 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >>  0);
        buffer[o++] = (byte)(v >>  8);
        buffer[o++] = (byte)(v >> 16);
        buffer[o++] = (byte)(v >> 24);
        buffer[o++] = (byte)(v >> 32);
        buffer[o++] = (byte)(v >> 40);
        buffer[o++] = (byte)(v >> 48);
        buffer[o++] = (byte)(v >> 56);
        return o;
    }
    public static int Serialize_Be(UInt64 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >> 56);
        buffer[o++] = (byte)(v >> 48);
        buffer[o++] = (byte)(v >> 40);
        buffer[o++] = (byte)(v >> 32);
        buffer[o++] = (byte)(v >> 24);
        buffer[o++] = (byte)(v >> 16);
        buffer[o++] = (byte)(v >>  8);
        buffer[o++] = (byte)(v >>  0);
        return o;
    }

    public static UInt64 DeSerializeUInt64_Be(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 8)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt64_Be: Not enough bytes in buffer.");
        }

        return (UInt64)((UInt64)buffer[offset++] << 56)
                     + ((UInt64)buffer[offset++] << 48)
                     + ((UInt64)buffer[offset++] << 40)
                     + ((UInt64)buffer[offset++] << 32)
                     + ((UInt64)buffer[offset++] << 24)
                     + ((UInt64)buffer[offset++] << 16)
                     + ((UInt64)buffer[offset++] <<  8)
                     + ((UInt64)buffer[offset++] <<  0);
    }
    public static UInt64 DeSerializeUInt64_Le(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 8)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt64_Le: Not enough bytes in buffer.");
        }

        return (UInt64)((UInt64)buffer[offset++] <<  0)
                     + ((UInt64)buffer[offset++] <<  8)
                     + ((UInt64)buffer[offset++] << 16)
                     + ((UInt64)buffer[offset++] << 24)
                     + ((UInt64)buffer[offset++] << 32)
                     + ((UInt64)buffer[offset++] << 40)
                     + ((UInt64)buffer[offset++] << 48)
                     + ((UInt64)buffer[offset++] << 56);
    }
    #endregion UInt64

    #region Int32
    public static int GetSerializedLength(Int32 v)
    {
        return 4;
    }
    public static int Serialize_Be(Int32 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >> 24);
        buffer[o++] = (byte)(v >> 16);
        buffer[o++] = (byte)(v >>  8);
        buffer[o++] = (byte)(v >>  0);
        return o;
    }
    public static int Serialize_Le(Int32 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = (byte)(v >> 0);
        buffer[o++] = (byte)(v >> 8);
        buffer[o++] = (byte)(v >> 16);
        buffer[o++] = (byte)(v >> 24);
        return o;
    }

    public static Int32 DeSerializeInt32_Be(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeInt32_Be: Not enough bytes in buffer.");
        }

        return (Int32)(((UInt32)buffer[offset++] << 24)
                     + ((UInt32)buffer[offset++] << 16)
                     + ((UInt32)buffer[offset++] <<  8)
                     + ((UInt32)buffer[offset++] <<  0));
    }
    public static Int32 DeSerializeInt32_Le(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeInt32_Le: Not enough bytes in buffer.");
        }

        return (Int32)(((UInt32)buffer[offset++] << 0)
                       + ((UInt32)buffer[offset++] << 8)
                       + ((UInt32)buffer[offset++] << 16)
                       + ((UInt32)buffer[offset++] << 24));
    }
    #endregion Int32

    #region Float
    public static int GetSerializedLength(float v)
    {
        return 4;
    }
    public static int Serialize_Le(float v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        byte[] b = BitConverter.GetBytes(v);
        buffer[o++] = b[0]; //TODO: Verify byte order!
        buffer[o++] = b[1];
        buffer[o++] = b[2];
        buffer[o++] = b[3];
        return o;
    }
    public static int Serialize_Be(float v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        byte[] b = BitConverter.GetBytes(v);
        buffer[o++] = b[3]; 
        buffer[o++] = b[2];
        buffer[o++] = b[1];
        buffer[o++] = b[0];
        return o;
    }

    public static float DeSerializeFloat_Le(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeFloat_Le: Not enough bytes in buffer.");
        }
        float v = BitConverter.ToSingle(buffer, offset); // TODO: Is the byte order guaranteed?
        offset += 4;
        return v;
    }
    #endregion Float

    #region Double
    public static int GetSerializedLength(double v)
    {
        return 8;
    }
    public static int Serialize_Le(double v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        byte[] b = BitConverter.GetBytes(v);
        buffer[o++] = b[0]; //TODO: Verify byte order!
        buffer[o++] = b[1];
        buffer[o++] = b[2];
        buffer[o++] = b[3];
        buffer[o++] = b[4];
        buffer[o++] = b[5];
        buffer[o++] = b[6];
        buffer[o++] = b[7];
        return o;
    }
    public static int Serialize_Be(double v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        byte[] b = BitConverter.GetBytes(v);
        buffer[o++] = b[7]; //TODO: Verify byte order!
        buffer[o++] = b[6];
        buffer[o++] = b[5];
        buffer[o++] = b[4];
        buffer[o++] = b[3];
        buffer[o++] = b[2];
        buffer[o++] = b[1];
        buffer[o++] = b[0];
        return o;
    }

    public static double DeSerializeDouble_Le(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 8)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeDouble_Le: Not enough bytes in buffer.");
        }
        byte[] buf = new byte[8];
        buf[0] = buffer[offset++];
        buf[1] = buffer[offset++];
        buf[2] = buffer[offset++];
        buf[3] = buffer[offset++];
        buf[4] = buffer[offset++];
        buf[5] = buffer[offset++];
        buf[6] = buffer[offset++];
        buf[7] = buffer[offset++];
        double v = BitConverter.ToDouble(buf, 0); // TODO: Is the byte order guaranteed?
        return v;
    }
    #endregion Double

    #region String
    public static string DeSerializeString (byte[] buffer, ref int offset, int length, uint lengthCount)
    {
        int len;
        switch (lengthCount)
        {
            case 1:
                len = buffer[offset++];
                break;

            case 2:
                len = DeSerializeUInt16_Le(buffer, ref offset, length);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lengthCount), lengthCount, "Valid values are 1 and 2");
        }

        string s = Encoding.UTF8.GetString(buffer, offset, len).Replace("\0", "");
        offset += len;
        return s;
    }
    #endregion String

    #region DateTime

    public static DateTime DeSerializeDateTime(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeDateTime: Not enough bytes in buffer.");
        }

        DateTime v = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // UNIX Epoch
        return v.AddSeconds(DeSerializeUInt32_Le(buffer, ref offset, length));
    }
    
    #endregion DateTime

    #region Guid
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

    public static Guid DeSerializeGuid(byte[] buffer, ref int offset, int length)
    {
        byte[] buf = new byte[16];
        // Weird order
        buf[3] = buffer[offset++];
        buf[2] = buffer[offset++];
        buf[1] = buffer[offset++];
        buf[0] = buffer[offset++];

        buf[5] = buffer[offset++];
        buf[4] = buffer[offset++];

        buf[7] = buffer[offset++];
        buf[6] = buffer[offset++];

        buf[8] = buffer[offset++];
        buf[9] = buffer[offset++];

        buf[10] = buffer[offset++];
        buf[11] = buffer[offset++];
        buf[12] = buffer[offset++];
        buf[13] = buffer[offset++];
        buf[14] = buffer[offset++];
        buf[15] = buffer[offset++];

        return new Guid(buf);
    }
    #endregion Guid

    #region Vector3
    public static int Serialize_Le(Vector3 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        // Convert handedness
        o = Serialize_Le(v.x, buffer, o, length);
        o = Serialize_Le(v.z, buffer, o, length);
        o = Serialize_Le(v.y, buffer, o, length);
        return o;
    }
    public static Vector3 DeSerializeVector3(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4 * 3)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeVector3: Not enough bytes in buffer.");
        }

        Vector3 v = new Vector3 // Convert handedness:
        {
            x = DeSerializeFloat_Le(buffer, ref offset, length),
            z = DeSerializeFloat_Le(buffer, ref offset, length),
            y = DeSerializeFloat_Le(buffer, ref offset, length)
        };
        return v;
    }
    #endregion Vector3

    #region Vector3Byte

    public static Vector3Byte DeSerializeVector3Byte(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 3)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeVector3Byte: Not enough bytes in buffer.");
        }

        Vector3Byte v = new Vector3Byte // Convert handedness:
        {
            x = buffer[offset++],
            z = buffer[offset++],
            y = buffer[offset++]
        };
        return v;
    }
    #endregion Vector3Byte

    #region Vector3Double

    public static Vector3Double DeSerializeVector3Double(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 8 * 3)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeVector3Double: Not enough bytes in buffer.");
        }

        Vector3Double v = new Vector3Double // Convert handedness:
        {
            x = DeSerializeDouble_Le(buffer, ref offset, length),
            z = DeSerializeDouble_Le(buffer, ref offset, length),
            y = DeSerializeDouble_Le(buffer, ref offset, length)
        };
        return v;
    }
    #endregion Vector3Double

    #region Quaternion
    public static int Serialize_Le (Quaternion q, byte[] buffer, int offset, int length)
    {
        if (length - offset < 4 * 3)
        {
            throw new IndexOutOfRangeException("BinarySerializer.Serialize (Quaternion): Not enough bytes in buffer.");
        }

        Quaternion v = q.normalized;
        // Convert handedness TODO: Verify that this is done correctly
        offset = Serialize_Le (v.x, buffer, offset, length);
        offset = Serialize_Le (v.z, buffer, offset, length);
        offset = Serialize_Le (v.y, buffer, offset, length);
        return offset;
    }
    #endregion Quaternion

    #region Color

    public static Color DeSerializeColor(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeColor: Not enough bytes in buffer.");
        }

        Color v = new Color
        {
            r = buffer[offset++] / 255f,
            g = buffer[offset++] / 255f,
            b = buffer[offset++] / 255f,
            a = buffer[offset++] / 255f
        };
        return v;
    }
    #endregion Color
    
    #endregion BasicTypes

    #region Acks

    /// <summary>
    /// Serializes any acked serial numbers at the end of the buffer
    /// </summary>
    /// <param name="acks"></param>
    /// <param name="buffer"></param>
    public static void SerializeAcks(List<UInt32> acks, byte[] buffer)
    {
        int i = acks.Count;
        if (i == 0)
        {
            return;
        }

        if (i > 255)
        {
            throw new ArgumentOutOfRangeException("BinarySerializer.SerializeAcks: Too many acks in list. Max is 255.");
        }

        int length = buffer.Length;
        int o = length - (4 * i + 1);
        if (o < 0)
        {
            throw new ArgumentOutOfRangeException("BinarySerializer.SerializeAcks: Not enough bytes in the buffer.");
        }

        byte nAcks = (byte)i;
        for (i = 0; i < nAcks; i++)
        {
            UInt32 ack = acks[i];
            o = Serialize_Be(ack, buffer, o, length);
        }

        buffer[o++] = nAcks;
    }
    #endregion Acks
    
    #region ZeroCode
    public static byte[] ExpandZerocode(byte[] src, int start, int length)
    {
        // Count:
        int destIndex = 0;
        int srcIndex = start;
        while (srcIndex < start + length)
        {
            byte b = src[srcIndex++];
            if (b != 0)
            {
                destIndex++;
            }
            else
            {
                int repeatCount = 0;
                b = src[srcIndex++];
                while (b == 0)
                {
                    repeatCount += 256;
                    b = src[srcIndex++];
                }
                repeatCount += b;

                destIndex += repeatCount;
            }
        }

        // Expand:
        byte[] dest = new byte[destIndex];
        destIndex = 0;
        srcIndex = start;
        while (srcIndex < start + length)
        {
            byte b = src[srcIndex++];
            if (b != 0)
            {
                dest[destIndex++] = b;
            }
            else
            {
                int repeatCount = 0;
                b = src[srcIndex++];
                while (b == 0)
                {
                    repeatCount += 256;
                    b = src[srcIndex++];
                }
                repeatCount += b;

                for (int i = 0; i < repeatCount; i++)
                {
                    dest[destIndex++] = 0;
                }
            }
        }

        return dest;
    }
    #endregion ZeroCode
}
