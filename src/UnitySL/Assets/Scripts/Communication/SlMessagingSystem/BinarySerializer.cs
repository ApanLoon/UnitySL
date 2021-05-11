
using System;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Primitives;
using UnityEngine;
using Assets.Scripts.Extensions.SystemExtensions;
public static class BinarySerializer
{
    #region BasicTypes

    #region Bool
    public static bool DeSerializeBool(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 1)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeBool: Not enough bytes in buffer.");
        }

        return (buffer[offset++] != 0);
    }
    #endregion Bool

    #region UInt8
    public static int GetSerializedLength(byte v)
    {
        return 1;
    }
    public static int Serialize(byte v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        buffer[o++] = v;
        return o;
    }

    public static byte DeSerializeUInt8(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 1)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeUInt8: Not enough bytes in buffer.");
        }

        return buffer[offset++];
    }
    #endregion UInt8

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

    /// <summary>
    /// Reads a variable length integer into an unsigned 64 bit integer.
    /// As long as the next byte is not zero and the top bit is set, the method will keep reading.
    /// </summary>
    /// <param name="buf"></param>
    /// <param name="i"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static ulong DeSerializeUInt64v_Be(byte[] buf, ref int offset, int length)
    {
        UInt64 v = 0;
        while ((buf[offset] & 0x80) != 0)
        {
            v |= (UInt64)buf[offset++] & 0x7f;
            v <<= 7;
        }
        v |= (UInt64)buf[offset++] & 0x7f;
        return v;
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
    public static int GetSerializedLength(string v, uint lengthCount)
    {
        return   (int)lengthCount
               + Encoding.UTF8.GetByteCount(v)
               + (v.EndsWith("\0") ? 0 : 1); // NUL terminator (Must be used in ChatFromViewer, not sure if this is true for all strings)
    }

    public static int Serialize(string v, byte[] buffer, int offset, int length, uint lengthCount)
    {
        // TODO: Verify that the message fits

        int o = offset;

        if (v.EndsWith("\0") == false)
        {
            v += '\0';
        }
        byte[] bytes = Encoding.UTF8.GetBytes(v);
        int byteCount = bytes.Length;
        switch (lengthCount)
        {
            case 1:
                buffer[o++] = (byte)byteCount;
                break;

            case 2:
                o = Serialize_Le((UInt16)byteCount, buffer, o, length);
                break;
        }
        Array.Copy(bytes, 0, buffer, o, byteCount);
        o += byteCount;
        return o;
    }

    public static string DeSerializeString (byte[] buffer, ref int offset, int length, int lengthCount)
    {
        int len;
        switch (lengthCount)
        {
            case -1: // No length, always read length bytes
                len = length;
                break;

            case 0: // NUL-terminated
                len = offset;
                while (buffer[len++] != 0) { }
                len -= offset;
                break;

            case 1:
                len = buffer[offset++];
                break;

            case 2:
                len = DeSerializeUInt16_Le(buffer, ref offset, length);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lengthCount), lengthCount, "Valid values are -1, 0, 1 and 2");
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

    #region Vector2
    public static int Serialize_Le(Vector2 v, byte[] buffer, int offset, int length)
    {
        int o = offset;
        o = Serialize_Le(v.x, buffer, o, length);
        o = Serialize_Le(v.y, buffer, o, length);
        return o;
    }
    public static Vector2 DeSerializeVector2(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4 * 2)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeVector2: Not enough bytes in buffer.");
        }

        Vector2 v = new Vector2
        {
            x = DeSerializeFloat_Le(buffer, ref offset, length),
            y = DeSerializeFloat_Le(buffer, ref offset, length)
        };
        return v;
    }
    #endregion Vector2

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
    public static Quaternion DeSerializeQuaternion (byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4 * 3)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeQuaternion: Not enough bytes in buffer.");
        }

        Quaternion v = new Quaternion // Convert handedness:
        {
            x = DeSerializeFloat_Le(buffer, ref offset, length),
            z = DeSerializeFloat_Le(buffer, ref offset, length),
            y = DeSerializeFloat_Le(buffer, ref offset, length),
            w = 1f
        };
        return v;
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

    /// <summary>
    /// Note:  This is an optimisation to send common colours (1.f, 1.f, 1.f, 1.f)
    /// as all zeros.  However, the subtraction and addition must be done in unsigned
    /// byte space, not in float space, otherwise off-by-one errors occur. JC
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static Color DeSerializeColorInv(byte[] buffer, ref int offset, int length)
    {
        if (length - offset < 4)
        {
            throw new IndexOutOfRangeException("BinarySerializer.DeSerializeColor: Not enough bytes in buffer.");
        }

        Color v = new Color
        {
            r = (255 - buffer[offset++]) / 255f,
            g = (255 - buffer[offset++]) / 255f,
            b = (255 - buffer[offset++]) / 255f,
            a = (255 - buffer[offset++]) / 255f
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

    #region Primitives
    #region ExtraData

    public static ExtraParameters DeSerializeExtraParameters(byte[] buffer, ref int offset, int length)
    {
        ExtraParameters parameters = new ExtraParameters();
        byte extraDataLen = buffer[offset++]; // TODO: The following code should check against this length instead.
        byte nParameters = buffer[offset++];
        for (int i = 0; i < nParameters; i++)
        {
            ExtraParameterType type = (ExtraParameterType)BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, length);
            int len = (int)BinarySerializer.DeSerializeUInt32_Le(buffer, ref offset, length);
            int paramLimit = offset + len;
            switch (type)
            {
                case ExtraParameterType.Light:
                    parameters.LightParameter = DeSerializeLightParameter(buffer, ref offset, paramLimit);
                    break;
                case ExtraParameterType.Flexible:
                    parameters.FlexibleObjectData = DeSerializeFlexibleObjectData(buffer, ref offset, paramLimit);
                    break;
                case ExtraParameterType.Mesh: // A Mesh is a Sculpt with SculptType = Mesh
                case ExtraParameterType.Sculpt:
                    parameters.SculptParams = DeSerializeSculptParams(buffer, ref offset, paramLimit);
                    break;
                case ExtraParameterType.LightImage:
                    parameters.LightImageParams = DeSerializeLightImageParams(buffer, ref offset, paramLimit);
                    break;
                case ExtraParameterType.ExtendedMesh:
                    parameters.ExtendedMeshParams = DeSerializeExtendedMeshParams(buffer, ref offset, paramLimit);
                    break;
                default:
                    Logger.LogWarning("ObjectUpdateCompressedMessage.DeSerialise", $"Unknown ExtraParameterType: {type}");
                    offset += len;
                    break;
            }

        }
        return parameters;
    }

    public static LightParameter DeSerializeLightParameter(byte[] buffer, ref int offset, int length)
    {
        LightParameter parameter = new LightParameter();
        parameter.SetLinearColour(DeSerializeColor(buffer, ref offset, length));
        parameter.Radius  = DeSerializeFloat_Le(buffer, ref offset, length);
        parameter.Cutoff  = DeSerializeFloat_Le(buffer, ref offset, length);
        parameter.Falloff = DeSerializeFloat_Le(buffer, ref offset, length);
        return parameter;
    }

    public static FlexibleObjectData DeSerializeFlexibleObjectData(byte[] buffer, ref int offset, int length)
    {
        int start = offset;
        FlexibleObjectData parameter = new FlexibleObjectData();

        byte bit1;
        byte bit2;
        byte b;
        b = buffer[offset++];
        bit1 = (byte)((b >> 6) & 2);
        parameter.Tension = (b & 0x7f) / 10f;
        b = buffer[offset++];
        bit2 = (byte) ((b >> 7) & 1);
        parameter.AirFriction = (b & 0x7f) / 10f;
        parameter.SimulateLod = bit1 | bit2;
        b = buffer[offset++];
        parameter.Gravity = b / 10f - 10f;
        b = buffer[offset++];
        parameter.WindSensitivity = b / 10f;

        if (length > offset - start)
        {
            parameter.UserForce = DeSerializeVector3(buffer, ref offset, length);
        }

        return parameter;
    }

    public static SculptParams DeSerializeSculptParams(byte[] buffer, ref int offset, int length)
    {
        SculptParams parameter = new SculptParams();
        parameter.SculptTextureId = DeSerializeGuid(buffer, ref offset, length);
        byte b = buffer[offset++];
        parameter.SculptType = (SculptType)(b & (byte)(SculptType.Sphere | SculptType.Torus | SculptType.Plane | SculptType.Cylinder | SculptType.Mesh));
        parameter.SculptFlags = (SculptFlags)(b & (byte)(SculptFlags.Invert | SculptFlags.Mirror));
        return parameter;
    }

    public static LightImageParams DeSerializeLightImageParams(byte[] buffer, ref int offset, int length)
    {
        LightImageParams parameter = new LightImageParams();
        parameter.LightTextureId = DeSerializeGuid(buffer, ref offset, length);
        parameter.Params = DeSerializeVector3(buffer, ref offset, length);
        return parameter;
    }

    public static ExtendedMeshParams DeSerializeExtendedMeshParams(byte[] buffer, ref int offset, int length)
    {
        ExtendedMeshParams parameter = new ExtendedMeshParams();
        parameter.Flags = (ExtendedMeshFlags)DeSerializeUInt32_Le(buffer, ref offset, length);
        return parameter;
    }

    #endregion ExtraData
    #region TextureEntry
    /// <summary>
    /// De-serialises a TextureEntry object
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="length">The number of bytes following the offset that can be used to read the TextureEntry</param>
    /// <returns></returns>
    public static TextureEntry DeSerializeTextureEntry(byte[] buffer, ref int offset, int length)
    {
        int start = offset;
        int len = offset + length;
        TextureEntry entry = new TextureEntry();

        string logMessage = "TextureEntry:\n**** image_id:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len,
            
            DeSerializeGuid,

            value =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].TextureId = v);
                entry.DefaultTexture.TextureId = value;
                logMessage += $"                        curl http://asset-cdn.glb.agni.lindenlab.com/?texture_id={value} --output {value}.j2k\n";
            },

            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].TextureId = v, mask);
                logMessage += $"    0x{mask:x16} curl http://asset-cdn.glb.agni.lindenlab.com/?texture_id={value} --output {value}.j2k\n";
            }));

        logMessage += "**** colour:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len, 
            
            DeSerializeColorInv,

            value =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].Colour = v);
                entry.DefaultTexture.Colour = value;
                logMessage += $"                        {value}\n";
            },
            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].Colour = v, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        logMessage += "**** scale_s:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len, 
            
            DeSerializeFloat_Le,
            
            value =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].RepeatU = v);
                entry.DefaultTexture.RepeatU = value;
                logMessage += $"                        {value}\n";
            },
            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].RepeatU = v, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        logMessage += "**** scale_t:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len,
            
            DeSerializeFloat_Le,
            
            value =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].RepeatV = v);
                entry.DefaultTexture.RepeatV = value;
                logMessage += $"                        {value}\n";
            },
            ((mask, value) =>
            
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].RepeatV = v, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        logMessage += "**** offset_s:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len,

            (byte[] b, ref int o, int l) => DeSerializeInt16_Le(b, ref o, l) / (float)0x7fff,

            value =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].OffsetU = v);
                entry.DefaultTexture.OffsetU = value;
                logMessage += $"                        {value}\n";
            },

            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].OffsetU = v, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        logMessage += "**** offset_t:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len,

            (byte[] b, ref int o, int l) => DeSerializeInt16_Le(b, ref o, l) / (float)0x7fff,

            value =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].OffsetV = v);
                entry.DefaultTexture.OffsetV = value;
                logMessage += $"                        {value}\n";
            },
            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].OffsetV = v, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        logMessage += "**** image_rot:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len,

            (byte[] b, ref int o, int l) => DeSerializeInt16_Le(b, ref o, l) / _teTextureRotationPackFactor * Mathf.PI * 2,

            value =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].Rotation = v);
                entry.DefaultTexture.Rotation = value;
                logMessage += $"                        {value}\n";
            },
            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].Rotation = v, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        logMessage += "**** bump:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len,

            (byte[] b, ref int o, int l) => new BumpShinyFullBright(DeSerializeUInt8(b, ref o, l)),

            value =>
            {
                ApplyTextureEntryField(value, (i, v) =>
                {
                    entry.FaceTextures[i].Bumpiness  = v.Bumpiness;
                    entry.FaceTextures[i].FullBright = v.FullBright;
                    entry.FaceTextures[i].Shininess  = v.Shininess;
                });
                entry.DefaultTexture.Bumpiness  = value.Bumpiness;
                entry.DefaultTexture.FullBright = value.FullBright;
                entry.DefaultTexture.Shininess  = value.Shininess;
                logMessage += $"                        {value}\n";
            },

            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) =>
                {
                    entry.FaceTextures[i].Bumpiness  = v.Bumpiness;
                    entry.FaceTextures[i].FullBright = v.FullBright;
                    entry.FaceTextures[i].Shininess  = v.Shininess;
                }, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        logMessage += "**** media_flags:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len,

            (byte[] b, ref int o, int l) => new MediaTexGen(DeSerializeUInt8(b, ref o, l)),

            value =>
            {
                ApplyTextureEntryField(value, (i, v) =>
                {
                    entry.FaceTextures[i].HasMedia           = v.HasMedia;
                    //entry.FaceTextures[i].TextureMappingType = v.TexGenMode; // TODO: What is the difference between TextureMappingType and TexGenMode?
                });
                entry.DefaultTexture.HasMedia = value.HasMedia;
                //entry.DefaultTexture.TextureMappingType = value.TexGenMode; // TODO: What is the difference between TextureMappingType and TexGenMode?
                logMessage += $"                        {value}\n";
            },
            
            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) =>
                {
                    entry.FaceTextures[i].HasMedia = v.HasMedia;
                    //entry.FaceTextures[i].TextureMappingType = v.TexGenMode; // TODO: What is the difference between TextureMappingType and TexGenMode?
                }, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        logMessage += "**** glow:\n";
        DeSerializeTextureEntryField(buffer, ref offset, len,

            (byte[] b, ref int o, int l) => DeSerializeUInt8(b, ref o, l) / (float)0xff,

            value =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].Glow = v);
                entry.DefaultTexture.Glow = value;
                logMessage += $"                        {value}\n";
            },

            ((mask, value) =>
            {
                ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].Glow = v, mask);
                logMessage += $"    0x{mask:x16} {value}\n";
            }));

        if (offset < len)
        {
            logMessage += "**** material_id:\n";
            DeSerializeTextureEntryField(buffer, ref offset, len,
                
                DeSerializeGuid,

                value =>
                {
                    ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].MaterialId = v);
                    entry.DefaultTexture.MaterialId = value;
                    logMessage += $"                        {value}\n";
                },
                ((mask, value) =>
                {
                    ApplyTextureEntryField(value, (i, v) => entry.FaceTextures[i].MaterialId = v, mask);
                    logMessage += $"    0x{mask:x16} {value}\n";
                }));

        }
        //Logger.LogDebug("BinarySerializer.DeSerializeTextureEntry", logMessage);
        return entry;
    }

    /// <summary>
    /// Calls the action for every face set to 1 in the given mask.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="action"></param>
    /// <param name="mask"></param>
    public static void ApplyTextureEntryField<T>(T value, Action<int, T> action, UInt64 mask = UInt64.MaxValue)
    {
        for (int i = 0; i < TextureEntry.MAX_FACES; i++)
        {
            if ((mask & 1) != 0)
            {
                action(i, value);
            }

            mask >>= 1;
        }
    }

    /// <summary>
    /// De-serialises a texture entry field.
    ///
    /// First the default value is read and after that comes a series of face exceptions. Each exception
    /// contains a variable length bit mask and a value that applies to the faces that are set in the mask.
    /// </summary>
    /// <typeparam name="T">Field type to de-serialise</typeparam>
    /// <param name="buf"></param>
    /// <param name="o"></param>
    /// <param name="length"></param>
    /// <param name="get">Method that de-serialises a single value of the field type</param>
    /// <param name="defaultAction">Method that will be called as soon as the default value is de-serialised</param>
    /// <param name="exceptionAction">Method that will be called for every face exception, arguments are (mask, value)</param>
    public static void DeSerializeTextureEntryField<T>(byte[] buf, ref int o, int length, DeSerializeTextureEntryFieldDelegate<T> get, Action<T> defaultAction, Action<UInt64, T> exceptionAction)
    {
        T value = get(buf, ref o, length);
        defaultAction(value);
        while (o < length)
        {
            UInt64 mask = BinarySerializer.DeSerializeUInt64v_Be(buf, ref o, length);
            if (mask == 0)
            {
                break;
            }
            value = get(buf, ref o, length);
            exceptionAction(mask, value);
        }
    }

    public delegate T DeSerializeTextureEntryFieldDelegate<out T>(byte[] buf, ref int o, int length);

    /// <summary>
    /// Texture rotations are sent over the wire as a S16.  This is used to scale the actual float
    /// value to a S16.   Don't use 7FFF as it introduces some odd rounding with 180 since it 
    /// can't be divided by 2.   See DEV-19108
    /// </summary>
    private static float _teTextureRotationPackFactor = 0x08000;

    // The Bump Shiny Fullbright values are bits in an eight bit field:
    // +----------+
    // | SSFBBBBB | S = Shiny, F = Fullbright, B = Bumpmap
    // | 76543210 |
    // +----------+
    private const int _teBumpMask        = 0x1f; // 5 bits
    private const int _teFullBrightMask  = 0x01; // 1 bit
    private const int _teShinyMask       = 0x03; // 2 bits
    private const int _teBumpShinyMask   = (0xc0 | 0x1f);
    private const int _teFullBrightShift = 5;
    private const int _teShinyShift      = 6;
    private struct BumpShinyFullBright
    {
        public Bumpiness Bumpiness { get; }
        public Shininess Shininess { get; }
        public bool FullBright { get; }

        public BumpShinyFullBright(byte value)
        {
            Bumpiness = (Bumpiness)(value & _teBumpMask);
            FullBright = ((value >> _teFullBrightShift) & _teFullBrightMask) != 0;
            Shininess = (Shininess)((value >> _teShinyShift) & _teShinyMask);
        }

        public override string ToString()
        {
            return $"Bumpiness={Bumpiness}, FullBright={FullBright}, Shininess={Shininess}";
        }
    }

    // The Media Tex Gen values are bits in a bit field:
    // +----------+
    // | .....TTM | M = Media Flags (web page), T = LLTextureEntry::eTexGen, . = unused
    // | 76543210 |
    // +----------+
    private const int _teMediaMask   = 0x01;
    private const int _teTexGenMask  = 0x06;
    private const int _teTexGenShift = 1;
    
    private struct MediaTexGen
    {
        public bool HasMedia { get; }
        public TexGenMode TexGenMode { get; }

        public MediaTexGen(byte value)
        {
            HasMedia = (value & _teMediaMask) != 0;
            TexGenMode = (TexGenMode) ((value >> _teTexGenShift) & _teTexGenMask);
        }

        public override string ToString()
        {
            return $"HasMedia={HasMedia}, TexGen={TexGenMode}";
        }
    }

    #endregion TextureEntry
    #endregion Primitives
}
