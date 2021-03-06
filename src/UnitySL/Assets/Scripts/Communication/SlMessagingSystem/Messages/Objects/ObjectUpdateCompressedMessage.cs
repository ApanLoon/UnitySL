﻿using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Audio;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.Primitives;
using UnityEngine;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Objects
{
    [Flags]
    public enum CompressedFlags : UInt32
    {
        None = 0x00,
        /// <summary>Unknown</summary>
        ScratchPad = 0x01,
        /// <summary>Whether the object has a TreeSpecies</summary>
        Tree = 0x02,
        /// <summary>Whether the object has floating text ala llSetText</summary>
        HasText = 0x04,
        /// <summary>Whether the object has an active particle system</summary>
        HasParticles = 0x08,
        /// <summary>Whether the object has sound attached to it</summary>
        HasSound = 0x10,
        /// <summary>Whether the object is attached to a root object or not</summary>
        HasParent = 0x20,
        /// <summary>Whether the object has texture animation settings</summary>
        TextureAnimation = 0x40,
        /// <summary>Whether the object has an angular velocity</summary>
        HasAngularVelocity = 0x80,
        /// <summary>Whether the object has a name value pairs string</summary>
        HasNameValues = 0x100,
        /// <summary>Whether the object has a Media URL set</summary>
        MediaURL = 0x200
    }

    public class ObjectUpdateCompressedMessage : ObjectUpdateMessage
    {
        public ObjectUpdateCompressedMessage()
        {
            MessageId = MessageId.ObjectUpdateCompressed;
            Flags = 0;

            UpdateType = ObjectUpdateType.OUT_FULL_COMPRESSED;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            RegionHandle = new RegionHandle(BinarySerializer.DeSerializeUInt64_Le(buf, ref o, length));
            TimeDilation = BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length);

            string logMessage = $"ObjectUpdateCompressed: RegionHandle={RegionHandle}, TimeDilation={TimeDilation}";
            int nObjects = buf[o++];
            for (int i = 0; i < nObjects; i++)
            {
                UInt32 len;
                ObjectUpdateMessage.ObjectData data = new ObjectUpdateMessage.ObjectData();
                Objects.Add(data);

                data.UpdateFlags = (ObjectUpdateFlags)BinarySerializer.DeSerializeUInt32_Le(buf, ref o, length);

                int compressedLength = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);
                byte[] compressedData = new byte[compressedLength];
                Array.Copy(buf, o, compressedData, 0, compressedLength);
                o += compressedLength;
                int compressedOffset = 0;

                logMessage += $"\n  Object {i}: UpdateFlags={data.UpdateFlags}, Data({compressedData.Length})={BitConverter.ToString(compressedData)}";
                data.FullId      = BinarySerializer.DeSerializeGuid      (compressedData, ref compressedOffset, compressedLength);
                data.LocalId     = BinarySerializer.DeSerializeUInt32_Le (compressedData, ref compressedOffset, compressedLength);
                data.PCode       = (PCode)compressedData[compressedOffset++];
                data.State       = compressedData[compressedOffset++];
                data.Crc         = BinarySerializer.DeSerializeUInt32_Le(compressedData, ref compressedOffset, compressedLength);
                data.Material    = (MaterialType)compressedData[compressedOffset++];
                data.ClickAction = (ClickAction)compressedData[compressedOffset++];
                data.Scale       = BinarySerializer.DeSerializeVector3    (compressedData, ref compressedOffset, compressedLength);
                data.Position    = BinarySerializer.DeSerializeVector3    (compressedData, ref compressedOffset, compressedLength);
                data.Rotation    = BinarySerializer.DeSerializeQuaternion (compressedData, ref compressedOffset, compressedLength);
                CompressedFlags compressedFlags = (CompressedFlags)BinarySerializer.DeSerializeUInt32_Le(compressedData, ref compressedOffset, compressedLength);

                data.OwnerId     = BinarySerializer.DeSerializeGuid(compressedData, ref compressedOffset, compressedLength);

                logMessage += $"\n    FullId={data.FullId}, LocalId={data.LocalId}, PCode={data.PCode}, State={data.State}, Crc={data.Crc}, Material={data.Material}, ClickAction={data.ClickAction}, Scale={data.Scale}, Position={data.Position}, Rotation={data.Rotation}, CompressedFlags=({compressedFlags})";

                if ((compressedFlags & CompressedFlags.HasAngularVelocity) != 0)
                {
                    data.AngularVelocity = BinarySerializer.DeSerializeVector3(compressedData, ref compressedOffset, compressedLength);
                    logMessage += $", AngularVelocity={data.AngularVelocity}";
                }

                data.ParentId = (compressedFlags & CompressedFlags.HasParent) != 0 ? BinarySerializer.DeSerializeUInt32_Le (compressedData, ref compressedOffset, compressedLength) : (uint) 0;
                logMessage += $", ParentId={data.ParentId}";

                if ((compressedFlags & CompressedFlags.Tree) != 0)
                {
                    byte treeSpecies = compressedData[compressedOffset++];
                    logMessage += $", TreeSpecies={treeSpecies}";
                }

                if ((compressedFlags & CompressedFlags.ScratchPad) != 0)
                {
                    len = compressedData[compressedOffset++];
                    compressedOffset += (int)len; // TODO: These offsets and length should all be UInt32
                    logMessage += $", Scratchpad({len})";
                }

                if ((compressedFlags & CompressedFlags.HasText) != 0)
                {
                    data.Text       = BinarySerializer.DeSerializeString(compressedData, ref compressedOffset, compressedLength, 0);
                    data.TextColour = BinarySerializer.DeSerializeColor(compressedData, ref compressedOffset, compressedLength);
                    logMessage += $", Text={data.Text}, TextColour={data.TextColour}";
                }

                if ((compressedFlags & CompressedFlags.MediaURL) != 0)
                {
                    data.MediaUrl = BinarySerializer.DeSerializeString(compressedData, ref compressedOffset, compressedLength, 0);
                    logMessage += $", MediaUrl={data.MediaUrl}";
                }

                if ((compressedFlags & CompressedFlags.HasParticles) != 0)
                {
                    // TODO: Parse the particle system data. OpenMetaverse says that this is a BitPack of 86 bytes.
                    len = 86;
                    compressedOffset += (int)len;
                    logMessage += $", ParticleSystem({len})";
                }

                data.ExtraParameters = BinarySerializer.DeSerializeExtraParameters(compressedData, ref compressedOffset, compressedOffset + compressedLength);

                if ((compressedFlags & CompressedFlags.HasSound) != 0)
                {
                    data.SoundId = BinarySerializer.DeSerializeGuid(compressedData, ref compressedOffset, compressedLength);
                    data.Gain    = BinarySerializer.DeSerializeUInt32_Le(compressedData, ref compressedOffset, compressedLength);
                    data.SoundFlags = (SoundFlags) compressedData[compressedOffset++];
                    data.Radius = BinarySerializer.DeSerializeFloat_Le(compressedData, ref compressedOffset, compressedLength);
                    logMessage += $", SoundId={data.SoundId}, Gain={data.Gain}, SoundFlags={data.SoundFlags}, Radius={data.Radius}";
                }

                if ((compressedFlags & CompressedFlags.HasNameValues) != 0)
                {
                    data.NameValue = BinarySerializer.DeSerializeString(compressedData, ref compressedOffset, compressedLength, 0);
                    logMessage += $", NameValue={data.NameValue}";
                }

                data.PathCurve        = (PathType)compressedData[compressedOffset++];
                data.PathBegin        = BinarySerializer.DeSerializeUInt16_Le(compressedData, ref compressedOffset, length) * CUT_QUANTA;
                data.PathEnd          = BinarySerializer.DeSerializeUInt16_Le(compressedData, ref compressedOffset, length) * CUT_QUANTA;
                data.PathScaleX       = compressedData[compressedOffset++]                                                  * SCALE_QUANTA;
                data.PathScaleY       = compressedData[compressedOffset++]                                                  * SCALE_QUANTA;
                data.PathShearX       = compressedData[compressedOffset++]                                                  * SHEAR_QUANTA;
                data.PathShearY       = compressedData[compressedOffset++]                                                  * SHEAR_QUANTA;
                data.PathTwist        = (sbyte)compressedData[compressedOffset++]                                           * SCALE_QUANTA;
                data.PathTwistBegin   = (sbyte)compressedData[compressedOffset++]                                           * SCALE_QUANTA;
                data.PathRadiusOffset = (sbyte)compressedData[compressedOffset++]                                           * SCALE_QUANTA;
                data.PathTaperX       = (sbyte)compressedData[compressedOffset++]                                           * TAPER_QUANTA;
                data.PathTaperY       = (sbyte)compressedData[compressedOffset++]                                           * TAPER_QUANTA;
                data.PathRevolutions  = compressedData[compressedOffset++]                                                  * REV_QUANTA;
                data.PathSkew         = (sbyte)compressedData[compressedOffset++]                                           * SCALE_QUANTA;

                data.ProfileCurve     = (ProfileType)compressedData[compressedOffset++];
                data.ProfileBegin     = BinarySerializer.DeSerializeUInt16_Le(compressedData, ref compressedOffset, length) * CUT_QUANTA;
                data.ProfileEnd       = BinarySerializer.DeSerializeUInt16_Le(compressedData, ref compressedOffset, length) * CUT_QUANTA;
                data.ProfileHollow    = BinarySerializer.DeSerializeUInt16_Le(compressedData, ref compressedOffset, length) * HOLLOW_QUANTA;

                data.TextureEntry = BinarySerializer.DeSerializeTextureEntry(compressedData, ref compressedOffset, compressedLength, true);
                logMessage += $", TextureEntry={data.TextureEntry}";

                if ((compressedFlags & CompressedFlags.TextureAnimation) != 0)
                {
                    data.TextureAnimation = BinarySerializer.DeSerializeTextureAnimation(compressedData, ref compressedOffset, compressedLength);
                    logMessage += ", TextureAnimation";
                }

                data.IsAttachment = (compressedFlags & CompressedFlags.HasNameValues) != 0 && data.ParentId != 0;
            }
            //Logger.LogDebug("ObjectUpdateCompressedMessage.DeSerialise", logMessage);
        }
        #endregion DeSerialise

    }
}