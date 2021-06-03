using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Audio;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.Extensions.MathExtensions;
using Assets.Scripts.Primitives;
using UnityEngine;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Objects
{
    public enum ObjectUpdateType
    {
        OUT_FULL,
        OUT_TERSE_IMPROVED,
        OUT_FULL_COMPRESSED,
        OUT_FULL_CACHED,
        OUT_UNKNOWN,
    }

    [Flags]
    public enum ObjectUpdateFlags : UInt32
    {
        USE_PHYSICS          = 0x00000001,
        CREATE_SELECTED      = 0x00000002,
        OBJECT_MODIFY        = 0x00000004,
        OBJECT_COPY          = 0x00000008,
        OBJECT_ANY_OWNER     = 0x00000010,
        OBJECT_YOU_OWNER     = 0x00000020,
        SCRIPTED             = 0x00000040,
        HANDLE_TOUCH         = 0x00000080,
        OBJECT_MOVE          = 0x00000100,
        TAKES_MONEY          = 0x00000200,
        PHANTOM              = 0x00000400,
        INVENTORY_EMPTY      = 0x00000800,

        AFFECTS_NAVMESH      = 0x00001000,
        CHARACTER            = 0x00002000,
        VOLUME_DETECT        = 0x00004000,
        INCLUDE_IN_SEARCH    = 0x00008000,

        ALLOW_INVENTORY_DROP = 0x00010000,
        OBJECT_TRANSFER      = 0x00020000,
        OBJECT_GROUP_OWNED   = 0x00040000,
        //UNUSED_000         = 0x00080000, // was OBJECT_YOU_OFFICER

        CAMERA_DECOUPLED     = 0x00100000,
        ANIM_SOURCE          = 0x00200000,
        CAMERA_SOURCE        = 0x00400000,

        //UNUSED_001         = 0x00800000, // was CAST_SHADOWS

        //UNUSED_002         = 0x01000000,
        //UNUSED_003         = 0x02000000,
        //UNUSED_004         = 0x04000000,
        //UNUSED_005         = 0x08000000,

        OBJECT_OWNER_MODIFY  = 0x10000000,

        TEMPORARY_ON_REZ     = 0x20000000,
        //UNUSED_006         = 0x40000000, // was TEMPORARY
        //UNUSED_007         = 0x80000000, // was ZLIB_COMPRESSED

        LOCAL = ANIM_SOURCE | CAMERA_SOURCE,
        WORLD = USE_PHYSICS | PHANTOM | TEMPORARY_ON_REZ
    }

    [Flags]
    public enum PCode : byte
    {
        // useful masks
        HOLLOW_MASK         = 0x80,		// has a thickness
        SEGMENT_MASK        = 0x40,		// segments (1 angle)
        PATCH_MASK          = 0x20,		// segmented segments (2 angles)
        HEMI_MASK           = 0x10,		// half-primitives get their own type per PR's dictum
        BASE_MASK           = 0x0F,

        // primitive shapes
        CUBE                = 1,
        PRISM               = 2,
        TETRAHEDRON         = 3,
        PYRAMID             = 4,
        CYLINDER            = 5,
        CONE                = 6,
        SPHERE              = 7,
        TORUS               = 8,
        VOLUME              = 9,

        // surfaces
        //SURFACE_TRIANGLE 	= 10,
        //SURFACE_SQUARE 	= 11,
        //SURFACE_DISC 		= 12,

        APP                 = 14, // App specific pcode (for viewer/sim side only objects)
        LEGACY              = 15,

        // Pcodes for legacy objects
        //LEGACY_ATOR       = 0x10 | LEGACY, // ATOR
        LEGACY_AVATAR       = 0x20 | LEGACY, // PLAYER
        //LEGACY_BIRD       = 0x30 | LEGACY, // BIRD
        //LEGACY_DEMON      = 0x40 | LEGACY, // DEMON
        LEGACY_GRASS        = 0x50 | LEGACY, // GRASS
        TREE_NEW            = 0x60 | LEGACY, // new trees
        //LEGACY_ORACLE     = 0x70 | LEGACY, // ORACLE
        LEGACY_PART_SYS     = 0x80 | LEGACY, // PART_SYS
        LEGACY_ROCK         = 0x90 | LEGACY, // ROCK
        //LEGACY_SHOT       = 0xA0 | LEGACY, // BASIC_SHOT
        //LEGACY_SHOT_BIG   = 0xB0 | LEGACY,
        //LEGACY_SMOKE      = 0xC0 | LEGACY, // SMOKE
        //LEGACY_SPARK      = 0xD0 | LEGACY, // SPARK
        LEGACY_TEXT_BUBBLE  = 0xE0 | LEGACY, // TEXTBUBBLE
        LEGACY_TREE         = 0xF0 | LEGACY, // TREE

        // hemis
        CYLINDER_HEMI       = CYLINDER | HEMI_MASK,
        CONE_HEMI           = CONE     | HEMI_MASK,
        SPHERE_HEMI         = SPHERE   | HEMI_MASK,
        TORUS_HEMI          = TORUS    | HEMI_MASK,
    }


    /// <summary>
    /// Material type for a primitive
    /// </summary>
    public enum MaterialType : byte
    {
        Stone = 0,
        Metal,
        Glass,
        Wood,
        Flesh,
        Plastic,
        Rubber,
        Light
    }

    public enum JointType : byte
    {
        /// <summary></summary>
        Invalid = 0,
        /// <summary></summary>
        Hinge = 1,
        /// <summary></summary>
        Point = 2,
        // <summary></summary>
        //[Obsolete]
        //LPoint = 3,
        //[Obsolete]
        //Wheel = 4
    }

    /// <summary>
    /// This message can arrive on the circuit as is, but it is also used as the event data for other object update messages.
    /// </summary>
    public class ObjectUpdateMessage : Message
    {
        // Used for packing and unpacking parameters
        protected const float CUT_QUANTA = 0.00002f;
        protected const float SCALE_QUANTA = 0.01f;
        protected const float SHEAR_QUANTA = 0.01f;
        protected const float TAPER_QUANTA = 0.01f;
        protected const float REV_QUANTA = 0.015f;
        protected const float HOLLOW_QUANTA = 0.00002f;

        public ObjectUpdateType UpdateType { get; set; } = ObjectUpdateType.OUT_FULL; // Not in the message itself, set depending on how the data arrived (Full, Terse, Compressed, Cached etc)
        public RegionHandle RegionHandle { get; set; }
        public UInt16 TimeDilation { get; set; }

        public List<ObjectData> Objects { get; set; } = new List<ObjectData>();
        public class ObjectData
        {
            public UInt32 LocalId { get; set; }
            public byte State { get; set; } // TODO: Create enum

            public Guid FullId { get; set; }
            public UInt32 Crc { get; set; }

            public PCode PCode { get; set; }
            public MaterialType Material { get; set; }
            public ClickAction ClickAction { get; set; }
            public Vector3 Scale { get; set; }
            public MovementUpdate MovementUpdate { get; set; }

            public UInt32 ParentId { get; set; }
            public ObjectUpdateFlags UpdateFlags { get; set; }

            public PathType PathCurve { get; set; }
            public ProfileType ProfileCurve { get; set; }
            public float PathBegin { get; set; }
            public float PathEnd { get; set; }
            public float PathScaleX { get; set; }
            public float PathScaleY { get; set; }
            public float PathShearX { get; set; }
            public float PathShearY { get; set; }
            public float PathTwist { get; set; }
            public float PathTwistBegin { get; set; }
            public float PathRadiusOffset { get; set; }
            public float PathTaperX { get; set; }
            public float PathTaperY { get; set; }
            public float PathRevolutions { get; set; }
            public float PathSkew { get; set; }
            public float ProfileBegin { get; set; }
            public float ProfileEnd { get; set; }
            public float ProfileHollow { get; set; }

            public TextureEntry TextureEntry { get; set; }
            public TextureAnimation TextureAnimation { get; set; }

            public string NameValue { get; set; }
            public byte[] Data2 { get; set; } // TODO: What is this?
            public string Text { get; set; } // llSetText hovering text
            public Color TextColour { get; set; }
            public string MediaUrl { get; set; }

            public byte[] ParticleSystemData { get; set; }

            public ExtraParameters ExtraParameters { get; set; }

            public Guid SoundId { get; set; }
            public Guid OwnerId { get; set; }
            public float Gain { get; set; }
            public SoundFlags SoundFlags { get; set; }
            public float Radius { get; set; }

            public JointType JointType { get; set; }
            public Vector3 JointPivot { get; set; }
            public Vector3 JointAxisOrAnchor { get; set; }
        
            public bool IsAttachment { get; set; }

            public override string ToString()
            {
                return     $"                     ObjectId={LocalId}, State={State}"
                       + $"\n                     FullId={FullId}, Crc={Crc}, PCode={PCode}, Material={Material}, ClickAction={ClickAction}, Scale={Scale}"
                       + $"\n                     MovementUpdate={MovementUpdate}"
                       + $"\n                     ParentId={ParentId}, UpdateFlags={UpdateFlags}"
                       + $"\n                     PathCurve={PathCurve}, ProfileCurve={ProfileCurve}, Path=({PathBegin}-{PathEnd}), PathScale=({PathScaleX}, {PathScaleY}), PathShear=({PathShearX}, {PathShearY}), PathTwist={PathTwist}, PathTwistBegin={PathTwistBegin}, PathRadiusOffset={PathRadiusOffset}, PathTaper=({PathTaperX}, {PathTaperY}), PathRevolutions={PathRevolutions}, PathSkew={PathSkew}, Profile=({ProfileBegin}-{ProfileEnd}), Hollow={ProfileHollow}"
                       + $"\n                     TextureEntry({TextureEntry})"
                       + $"\n                     TextureAnim={TextureAnimation}"
                       + $"\n                     NameValue={(NameValue != null ? NameValue.Replace("\n", "\\n"): "null")}"
                       + $"\n                     Data2({Data2?.Length.ToString()}), Text={Text}, TextColour={TextColour}, MediaUrl={MediaUrl}"
                       + $"\n                     ParticleSystemData({ParticleSystemData?.Length.ToString()})"
                       + $"\n                     ExtraParams({ExtraParameters})"
                       + $"\n                     SoundId={SoundId}, OwnerId={OwnerId}, Gain={Gain}, Flags={SoundFlags}, Radius={Radius}"
                       + $"\n                     JointType={JointType}, JointPivot={JointPivot}, JointAxisOrAnchor={JointAxisOrAnchor}"
                       ;
            }
        }

        public class MovementUpdate
        {
            public Vector4 FootPlane { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Velocity { get; set; }
            public Vector3 Acceleration { get; set; }
            public Quaternion Rotation { get; set; }
            public Vector3 AngularVelocity { get; set; }

            public override string ToString()
            {
                return $"{{FootPlane={FootPlane}, Position={Position}, Velocity={Velocity}, Acceleration={Acceleration}, Rotation= {Rotation}, AngularVelocity={AngularVelocity}}}";
            }
        }

        public ObjectUpdateMessage()
        {
            MessageId = MessageId.ObjectUpdate;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            RegionHandle                = new RegionHandle(BinarySerializer.DeSerializeUInt64_Le(buf, ref o, length));
            TimeDilation                = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);

            int nObjects = buf[o++];
            for (int i = 0; i < nObjects; i++)
            {
                int len;
                ObjectUpdateMessage.ObjectData data = new ObjectUpdateMessage.ObjectData();
                Objects.Add(data);

                data.LocalId           = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
                data.State              = buf[o++];

                data.FullId             = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
                data.Crc                = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
                data.PCode              = (PCode)buf[o++];
                data.Material           = (MaterialType)buf[o++];
                data.ClickAction        = (ClickAction)buf[o++];
                data.Scale              = BinarySerializer.DeSerializeVector3   (buf, ref o, buf.Length);
                data.MovementUpdate     = DeSerializeMovementUpdate(buf, ref o, buf.Length);

                data.ParentId           = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
                data.UpdateFlags        = (ObjectUpdateFlags)BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);

                data.PathCurve          = (PathType)buf[o++];
                data.ProfileCurve       = (ProfileType)buf[o++];
                data.PathBegin          = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length) * CUT_QUANTA;
                data.PathEnd            = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length) * CUT_QUANTA;
                data.PathScaleX         = buf[o++]        * SCALE_QUANTA;
                data.PathScaleY         = buf[o++]        * SCALE_QUANTA;
                data.PathShearX         = buf[o++]        * SHEAR_QUANTA;
                data.PathShearY         = buf[o++]        * SHEAR_QUANTA;
                data.PathTwist          = (sbyte)buf[o++] * SCALE_QUANTA;
                data.PathTwistBegin     = (sbyte)buf[o++] * SCALE_QUANTA;
                data.PathRadiusOffset   = (sbyte)buf[o++] * SCALE_QUANTA;
                data.PathTaperX         = (sbyte)buf[o++] * TAPER_QUANTA;
                data.PathTaperY         = (sbyte)buf[o++] * TAPER_QUANTA;
                data.PathRevolutions    = buf[o++]        * REV_QUANTA;
                data.PathSkew           = (sbyte)buf[o++] * SCALE_QUANTA;
                data.ProfileBegin       = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length) * CUT_QUANTA;
                data.ProfileEnd         = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length) * CUT_QUANTA;
                data.ProfileHollow      = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length) * HOLLOW_QUANTA;

                data.TextureEntry       = BinarySerializer.DeSerializeTextureEntry(buf, ref o, length);
                data.TextureAnimation   = BinarySerializer.DeSerializeTextureAnimation(buf, ref o, length);

                data.NameValue          = BinarySerializer.DeSerializeString    (buf, ref o, length, 2);
                len                     = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);
                data.Data2 = new byte[len];
                Array.Copy(buf, o, data.Data2, 0, len);
                o += len;
                data.Text               = BinarySerializer.DeSerializeString    (buf, ref o, length, 1);
                data.TextColour         = BinarySerializer.DeSerializeColor     (buf, ref o, length);
                data.MediaUrl           = BinarySerializer.DeSerializeString    (buf, ref o, length, 1);

                len = buf[o++];
                data.ParticleSystemData = new byte[len];
                Array.Copy(buf, o, data.ParticleSystemData, 0, len);
                o += len;

                len = buf[o++];
                data.ExtraParameters = BinarySerializer.DeSerializeExtraParameters(buf, ref o, o + len);

                data.SoundId            = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
                data.OwnerId            = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
                data.Gain               = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, buf.Length);
                data.SoundFlags         = (SoundFlags)buf[o++];
                data.Radius             = BinarySerializer.DeSerializeFloat_Le  (buf, ref o, length);

                data.JointType          = (JointType)buf[o++];
                data.JointPivot         = BinarySerializer.DeSerializeVector3   (buf, ref o, buf.Length);
                data.JointAxisOrAnchor  = BinarySerializer.DeSerializeVector3   (buf, ref o, buf.Length);

                //Logger.LogDebug("ObjectUpdateMessage.DeSerialise", ToString());
            }
        }

        public MovementUpdate DeSerializeMovementUpdate(byte[] buffer, ref int offset, int length, int forcedLen = 0)
        {
            float size = 256; // TODO: This should be fetched from the Region.WidthInMetres of the region this message is for.
            float minHeight = -256; // TODO: This should be fetched from the surface of the region this message is for.
            float maxHeight = 3 * 256; // TODO: This should be fetched from the surface of the region this message is for.

            MovementUpdate update = new MovementUpdate();

            int len = forcedLen == 0 ? buffer[offset++] : forcedLen;
            int limit = offset + len;
            switch (len)
            {
                case 60 + 16:
                    // pull out collision normal for avatar
                    update.FootPlane       = BinarySerializer.DeSerializeVector4(buffer, ref offset, limit);
                    goto case 60;

                case 60:
                    update.Position        = BinarySerializer.DeSerializeVector3(buffer, ref offset, limit);
                    update.Velocity        = BinarySerializer.DeSerializeVector3(buffer, ref offset, limit);
                    update.Acceleration    = BinarySerializer.DeSerializeVector3(buffer, ref offset, limit);
                    update.Rotation        = BinarySerializer.DeSerializeQuaternion(buffer, ref offset, limit); // Theta
                    update.AngularVelocity = BinarySerializer.DeSerializeVector3(buffer, ref offset, limit); // Omega
                    break;

                case 32 + 16:
                    // pull out collision normal for avatar
                    update.FootPlane = BinarySerializer.DeSerializeVector4(buffer, ref offset, limit);

                    goto case 32;

                case 32: // Values are UInt16 and needs to be quantized to floats
                    update.Position = new Vector3(
                        x:BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-0.5f * size, 1.5f * size),
                        z:BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(minHeight, maxHeight), // Handedness
                        y:BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-0.5f * size, 1.5f * size));
                    update.Velocity = new Vector3(
                        x:BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size),
                        z:BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size), // Handedness
                        y:BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size));
                    update.Acceleration = new Vector3(
                        x: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size),
                        z: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size), // Handedness
                        y: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size));
                    update.Rotation = new Quaternion(
                        x: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-1f, 1f),
                        z: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-1f, 1f), // Handedness
                        y: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-1f, 1f),
                        w: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-1f, 1f));
                    update.AngularVelocity = new Vector3(
                        x: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size),
                        z: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size), // Handedness
                        y: BinarySerializer.DeSerializeUInt16_Le(buffer, ref offset, limit).ToFloat(-size, size));
                    break;

                case 16:// Values are UInt8 and needs to be quantized to floats
                    update.Position = new Vector3(
                        x: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-0.5f * size, 1.5f * size),
                        z: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(minHeight, maxHeight), // Handedness
                        y: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-0.5f * size, 1.5f * size));
                    update.Velocity = new Vector3(
                        x: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size),
                        z: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size), // Handedness
                        y: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size));
                    update.Acceleration = new Vector3(
                        x: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size),
                        z: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size), // Handedness
                        y: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size));
                    update.Rotation = new Quaternion(
                        x: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-1f, 1f),
                        z: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-1f, 1f), // Handedness
                        y: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-1f, 1f),
                        w: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-1f, 1f));
                    update.AngularVelocity = new Vector3(
                        x: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size),
                        z: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size), // Handedness
                        y: BinarySerializer.DeSerializeUInt8(buffer, ref offset, limit).ToFloat(-size, size));
                    break;
            }
            return update;
        }
        #endregion DeSerialise

        public override string ToString()
        {
            string s = $"{base.ToString()}: UpdateType={UpdateType}, RegionHandle={RegionHandle}, TimeDilation={TimeDilation}";
            foreach (ObjectUpdateMessage.ObjectData data in Objects)
            {
                s += $"{data}";
            }
            return s;
        }
    }
}