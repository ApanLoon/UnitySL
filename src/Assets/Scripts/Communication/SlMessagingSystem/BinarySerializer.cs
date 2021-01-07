
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;

public static class BinarySerializer
{
    #region Messages

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

        byte[] dataBuffer = buf;
        int dataStart = o;
        int dataLen = buf.Length - ackLength;
        if ((flags & PacketFlags.ZeroCode) != 0)
        {
            dataBuffer = BinarySerializer.ExpandZerocode(buf, o, dataLen - o);
            dataStart = 0;
            dataLen = dataBuffer.Length;
        }
        DeSerializerResult r = DeserializeData(dataBuffer, ref dataStart, dataLen, frequency, flags, sequenceNumber, extraHeader, id);

        if (r.Message == null)
        {
            // Create a dummy message so that we parse the acks as well as ack this message if it is reliable.
            r.Message = new Message
            {
                Flags = flags,
                SequenceNumber = sequenceNumber,
                ExtraHeader = extraHeader,
                Frequency = frequency
            };
        }

        r.Message.Acks = acks;

        return r.Message;
    }

    private static DeSerializerResult DeserializeData (byte[]           buf,
                                                       ref int          o,
                                                       int              length,
                                                       MessageFrequency frequency,
                                                       PacketFlags      flags,
                                                       uint             sequenceNumber,
                                                       byte[]           extraHeader,
                                                       UInt32           id)
    {
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
            return new DeSerializerResult(){Message = null, Offset = o};
        }

        MessageId messageId = (MessageId) id;
        if (MessageDeSerializers.ContainsKey(messageId) == false)
        {
            Logger.LogError($"BinarySerializer.DeSerializeMessage: No de-serializer for message id ({messageId})");
            return new DeSerializerResult() { Message = null, Offset = o };
        }

        return MessageDeSerializers[messageId](buf, o, length, flags, sequenceNumber, extraHeader, frequency, messageId);
    }

    public class DeSerializerResult
    {
        public int Offset { get; set; }
        public Message Message { get; set; }
    }
    #endregion Message

    public static Dictionary<MessageId, Func<byte[], int, int, PacketFlags, UInt32, byte[], MessageFrequency, MessageId, DeSerializerResult>> MessageDeSerializers = new Dictionary<MessageId, Func<byte[], int, int, PacketFlags, uint, byte[], MessageFrequency, MessageId, DeSerializerResult>>()
    {
        {
            MessageId.StartPingCheck, // 0x00000001
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                StartPingCheckMessage m = new StartPingCheckMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                m.PingId = buf[o++]; 
                m.OldestUnchecked = DeSerializeUInt32_Le (buf, ref o, buf.Length);

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.CoarseLocationUpdate, // 0x0000ff06
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                CoarseLocationUpdateMessage m = new CoarseLocationUpdateMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;
                Guid guid;

                byte nLocations = buf[o++];
                for (byte i = 0; i < nLocations; i++)
                {
                    m.Locations.Add (new CoarseLocation {Position = DeSerializeVector3Byte(buf, ref o, buf.Length)});
                }
                int youIndex = DeSerializeInt16_Le (buf, ref o, buf.Length);
                if (youIndex > 0 && youIndex < nLocations - 1)
                {
                    m.Locations[youIndex].IsYou = true;
                }

                int preyIndex = DeSerializeInt16_Le (buf, ref o, buf.Length);
                if (preyIndex > 0 && preyIndex < nLocations - 1)
                {
                    m.Locations[preyIndex].IsPrey = true;
                }

                byte nAgents = buf[o++];
                for (byte i = 0; i < nAgents; i++)
                {
                    o = DeSerialize(out guid, buf, o, length);
                    if (i < nLocations)
                    {
                        m.Locations[i].AgentId = guid;
                    }
                }

                //string s = "CoarseLocationUpdateMessage:";
                //for (int i = 0; i < nLocations; i++)
                //{
                //    CoarseLocation l = m.Locations[i];
                //    s +=$"\n    Position={l.Position} IsYou={l.IsYou} Prey={l.IsPrey} AgentId={l.AgentId}";
                //}
                //Logger.LogDebug(s);

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.AttachedSound, // 0x0000ff0d
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                AttachedSoundMessage m = new AttachedSoundMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                Guid guid;
                o = DeSerialize(out guid, buf, o, length); m.SoundId = guid;
                o = DeSerialize(out guid, buf, o, length); m.ObjectId = guid;
                o = DeSerialize(out guid, buf, o, length); m.OwnerId = guid;
                m.Gain = DeSerializeUInt32_Le (buf, ref o, buf.Length);
                m.SoundFlags = (SoundFlags) buf[o++];
                //Logger.LogDebug($"AttachedSoundMessage: SoundId={m.SoundId} ObjectId={m.ObjectId} OwnerId={m.OwnerId} Gain={m.Gain} Flags={m.SoundFlags}");

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.PreloadSound, // 0x0000ff0f
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                PreloadSoundMessage m = new PreloadSoundMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;
                Guid guid;

                byte nSounds = buf[o++];
                for (byte i = 0; i < nSounds; i++)
                {
                    PreloadSoundMessage.SoundInfo si = new PreloadSoundMessage.SoundInfo();
                    o = DeSerialize(out guid, buf, o, length); si.ObjectId = guid;
                    o = DeSerialize(out guid, buf, o, length); si.OwnerId = guid;
                    o = DeSerialize(out guid, buf, o, length); si.SoundId = guid;
                    m.Sounds.Add(si);
                    // Logger.LogDebug($"PreloadSoundMessage: ObjectId={si.ObjectId} OwnerId={si.OwnerId} SoundId={si.SoundId}");
                }

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.ViewerEffect, // 0x0000ff11
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                ViewerEffectMessage m = new ViewerEffectMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                Guid guid;
                o = DeSerialize(out guid,             buf, o, length); m.AgentId = guid;
                o = DeSerialize(out guid,             buf, o, length); m.SessionId = guid;

                byte nEffects = buf[o++];
                for (byte i = 0; i < nEffects; i++)
                {
                    Guid effectId;
                    o = DeSerialize(out guid,             buf, o, length); effectId = guid;
                    Guid agentId;
                    o = DeSerialize(out guid,             buf, o, length); agentId = guid;
                    ViewerEffectType type = (ViewerEffectType) buf[o++];
                    float duration = DeSerializeFloat_Le (buf, ref o, length);
                    Color color = DeSerializeColor       (buf, ref o, length);

                    byte typeDataLength = buf[o++];
                    ViewerEffect effect = null;
                    switch (type)
                    {
                        case ViewerEffectType.Text:
                            break;
                        case ViewerEffectType.Icon:
                            break;
                        case ViewerEffectType.Connector:
                            break;
                        case ViewerEffectType.FlexibleObject:
                            break;
                        case ViewerEffectType.AnimalControls:
                            break;
                        case ViewerEffectType.LocalAnimationObject:
                            break;
                        case ViewerEffectType.Cloth:
                            break;
                        
                        case ViewerEffectType.EffectBeam:
                        case ViewerEffectType.EffectGlow:
                        case ViewerEffectType.EffectPoint:
                        case ViewerEffectType.EffectTrail:
                        case ViewerEffectType.EffectSphere:
                        case ViewerEffectType.EffectSpiral:
                        case ViewerEffectType.EffectEdit:
                            ViewerEffectSpiral spiralEffect = new ViewerEffectSpiral();
                            o = DeSerialize(out guid,             buf, o, length); spiralEffect.SourceObjectId = guid;
                            o = DeSerialize(out guid,             buf, o, length); spiralEffect.TargetObjectId = guid;
                            spiralEffect.PositionGlobal = DeSerializeVector3Double(buf, ref o, length);
                            effect = spiralEffect;
                            break;

                        case ViewerEffectType.EffectLookAt:
                            ViewerEffectLookAt lookAtEffect = new ViewerEffectLookAt();
                            o = DeSerialize(out guid,             buf, o, length); lookAtEffect.SourceAvatarId = guid;
                            o = DeSerialize(out guid,             buf, o, length); lookAtEffect.TargetObjectId = guid;
                            lookAtEffect.TargetPosition = DeSerializeVector3Double(buf, ref o, length);
                            lookAtEffect.LookAtType = (ViewerEffectLookAtType) buf[o++];
                            effect = lookAtEffect;
                            break;

                        case ViewerEffectType.EffectPointAt:
                            break;
                        case ViewerEffectType.EffectVoiceViaualizer:
                            break;
                        case ViewerEffectType.NameTag:
                            break;
                        case ViewerEffectType.EffectBlob:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (effect == null)
                    {
                        throw new NotImplementedException($"ViewerEffectMessage: ViewerEffect type {type} is not implemented.");
                    }

                    effect.Id = effectId;
                    effect.AgentId = agentId;
                    effect.EffectType = type;
                    effect.Duration = duration;
                    effect.Color = color;
                    m.Effects.Add(effect);
                }

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.Wrapper, // 0xffff0001
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                int o = offset;
                UInt32 i = 0xffff0000 + buf[o++];
                return DeserializeData(buf, ref o, length, frequency, flags, sequenceNumber, extraHeader, i);
            }
        },

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
            MessageId.HealthMessage, // 0xffff008a
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                HealthMessage m = new HealthMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                m.Health = DeSerializeFloat_Le(buf, ref o, length);

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.RegionHandshake, // 0xffff0094
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                RegionHandshakeMessage m = new RegionHandshakeMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                string s;
                Guid guid;

                m.RegionFlags = DeSerializeUInt32_Le (buf, ref o, length);
                m.SimAccess = buf[o++];
                o = DeSerialize(out s, 1, buf, o, length); m.SimName = s;
                o = DeSerialize(out guid, buf, o, length); m.SimOwner = guid;
                m.IsEstateManager = buf[o++] != 0;
                m.WaterHeight = DeSerializeFloat_Le(buf, ref o, length);
                m.BillableFactor = DeSerializeFloat_Le(buf, ref o, length);
                o = DeSerialize(out guid, buf, o, length); m.CacheId = guid;
                o = DeSerialize(out guid, buf, o, length); m.TerrainBase0 = guid;
                o = DeSerialize(out guid, buf, o, length); m.TerrainBase1 = guid;
                o = DeSerialize(out guid, buf, o, length); m.TerrainBase2 = guid;
                o = DeSerialize(out guid, buf, o, length); m.TerrainBase3 = guid;
                o = DeSerialize(out guid, buf, o, length); m.TerrainDetail0 = guid;
                o = DeSerialize(out guid, buf, o, length); m.TerrainDetail1 = guid;
                o = DeSerialize(out guid, buf, o, length); m.TerrainDetail2 = guid;
                o = DeSerialize(out guid, buf, o, length); m.TerrainDetail3 = guid;
                m.TerrainStartHeight00 = DeSerializeFloat_Le(buf, ref o, length);
                m.TerrainStartHeight01 = DeSerializeFloat_Le(buf, ref o, length);
                m.TerrainStartHeight10 = DeSerializeFloat_Le(buf, ref o, length);
                m.TerrainStartHeight11 = DeSerializeFloat_Le(buf, ref o, length);
                m.TerrainHeightRange00 = DeSerializeFloat_Le(buf, ref o, length);
                m.TerrainHeightRange01 = DeSerializeFloat_Le(buf, ref o, length);
                m.TerrainHeightRange10 = DeSerializeFloat_Le(buf, ref o, length);
                m.TerrainHeightRange11 = DeSerializeFloat_Le(buf, ref o, length);

                o = DeSerialize(out guid, buf, o, length); m.RegionId = guid;

                m.CpuClassId = DeSerializeInt32_Le (buf, ref o, length);
                m.CpuRatio = DeSerializeInt32_Le (buf, ref o, length);
                o = DeSerialize(out s, 1, buf, o, length); m.ColoName = s;
                o = DeSerialize(out s, 1, buf, o, length); m.ProductSku = s;
                o = DeSerialize(out s, 1, buf, o, length); m.ProductName = s;

                int n = buf[o++];
                for (int i = 0; i < n; i++)
                {
                    RegionInfo4 info = new RegionInfo4()
                    {
                        RegionFlagsExtended = DeSerializeUInt32_Le(buf, ref o, length),
                        RegionProtocols = DeSerializeUInt32_Le(buf, ref o, length)
                    };
                    m.RegionInfo4.Add(info);
                }

                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.AgentMovementCompleteMessage, // 0xffff00fa
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                AgentMovementCompleteMessage m = new AgentMovementCompleteMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                Guid guid;
                string s;

                o = DeSerialize(out guid,             buf, o, length); m.AgentId = guid;
                o = DeSerialize(out guid,             buf, o, length); m.SessionId = guid;
                m.Position = DeSerializeVector3 (buf, ref o, length);
                m.LookAt   = DeSerializeVector3 (buf, ref o, length);
                m.RegionHandle = new RegionHandle(DeSerializeUInt64_Le(buf, ref o, length));
                m.TimeStamp = DeSerializeDateTime(buf, ref o, length);
                o = DeSerialize(out s, 2,    buf, o, length); m.ChannelVersion = s;
                
                return new DeSerializerResult(){Message = m, Offset = o};
            }
        },

        {
            MessageId.AgentDataUpdate, // 0xffff0183
            (buf, offset, length, flags, sequenceNumber, extraHeader, frequency, id) =>
            {
                AgentDataUpdateMessage m = new AgentDataUpdateMessage(flags, sequenceNumber, extraHeader, frequency, id);
                int o = offset;

                Guid guid;
                string s;

                o = DeSerialize(out guid,             buf, o, length); m.AgentId = guid;
                o = DeSerialize(out s, 1,    buf, o, length); m.FirstName = s;
                o = DeSerialize(out s, 1,    buf, o, length); m.LastName = s;
                o = DeSerialize(out s, 1,    buf, o, length); m.GroupTitle = s;
                o = DeSerialize(out guid,             buf, o, length); m.ActiveGroupId = guid;
                m.GroupPowers = DeSerializeUInt64_Le (buf, ref o, length);
                o = DeSerialize(out s, 1,    buf, o, length); m.GroupName = s;

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

        return (UInt32)((UInt32)buffer[offset++] << 0)
               + ((UInt32)buffer[offset++] << 8)
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

        return (UInt64)((UInt64)buffer[offset++] << 0)
               + ((UInt64)buffer[offset++] << 8)
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
    public static int DeSerialize(out string s, uint lengthCount, byte[] buffer, int offset, int length)
    {
        int o = offset;
        int len;
        switch (lengthCount)
        {
            case 1:
                len = buffer[o++];
                break;

            case 2:
                len = DeSerializeUInt16_Le(buffer, ref o, length);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(lengthCount), lengthCount, "Valid values are 1 and 2");
        }

        s = Encoding.UTF8.GetString(buffer, o, len).Replace("\0", "");
        o += len;
        return o;
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
    #endregion Guid

    #region Vector3

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
