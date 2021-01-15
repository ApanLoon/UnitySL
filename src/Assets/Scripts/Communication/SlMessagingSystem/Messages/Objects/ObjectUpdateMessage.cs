using System;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectUpdateType
{
    OUT_FULL,
    OUT_TERSE_IMPROVED,
    OUT_FULL_COMPRESSED,
    OUT_FULL_CACHED,
    OUT_UNKNOWN,
}

/// <summary>
/// This message can arrive on the circuit as is, but it is also used as the event data for other object update messages.
/// </summary>
public class ObjectUpdateMessage : Message
{
    public ObjectUpdateType UpdateType { get; set; } = ObjectUpdateType.OUT_FULL; // Not in the message itself, set depending on how the data arrived (Full, Terse, Compressed, Cached etc)
    public RegionHandle RegionHandle { get; set; }
    public UInt16 TimeDilation { get; set; }

    public List<ObjectData> Objects { get; set; } = new List<ObjectData>();
    public class ObjectData
    {
        public UInt32 ObjectId { get; set; }
        public byte State { get; set; } // TODO: Create enum

        public Guid FullId { get; set; }
        public UInt32 Crc { get; set; }

        public byte PCode { get; set; } // TODO: Create enum
        public byte Material { get; set; }
        public byte ClickAction { get; set; } // TODO: Create enum
        public Vector3 Scale { get; set; }
        public byte[] Data1 { get; set; } // TODO: What is this?

        public UInt32 ParentId { get; set; }
        public UInt32 UpdateFlags { get; set; } // TODO: Turn this into an enum

        public byte PathCurve { get; set; }
        public byte ProfileCurve { get; set; }
        public UInt16 PathBegin { get; set; }
        public UInt16 PathEnd { get; set; }
        public byte PathScaleX { get; set; }
        public byte PathScaleY { get; set; }
        public byte PathShearX { get; set; }
        public byte PathShearY { get; set; }
        public sbyte PathTwist { get; set; }
        public sbyte PathTwistBegin { get; set; }
        public sbyte PathRadiusOffset { get; set; }
        public sbyte PathTaperX { get; set; }
        public sbyte PathTaperY { get; set; }
        public byte PathRevolutions { get; set; }
        public sbyte PathSkew { get; set; }
        public UInt16 ProfileBegin { get; set; }
        public UInt16 ProfileEnd { get; set; }
        public UInt16 ProfileHollow { get; set; }

        public List<byte> TextureEntries { get; set; } = new List<byte>();
        public List<byte> TextureAnims { get; set; } = new List<byte>();

        public string NameValue { get; set; }
        public byte[] Data2 { get; set; } // TODO: What is this?
        public string Text { get; set; } // llSetText hovering text
        public Color TextColour { get; set; }
        public string MediaUrl { get; set; }

        public byte[] ParticleSystemData { get; set; }

        public byte[] ExtraParameters { get; set; }

        public Guid SoundId { get; set; }
        public Guid OwnerId { get; set; }
        public float Gain { get; set; }
        public SoundFlags SoundFlags { get; set; }
        public float Radius { get; set; }

        public byte JointType { get; set; } // Todo: Create enum
        public Vector3 JointPivot { get; set; }
        public Vector3 JointAxisOrAnchor { get; set; }
    }

    public ObjectUpdateMessage()
    {
        Id = MessageId.ObjectUpdate;
        Flags = 0;
        Frequency = MessageFrequency.High;
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

            data.ObjectId           = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
            data.State              = buf[o++];

            data.FullId             = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
            data.Crc                = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
            data.PCode              = buf[o++];
            data.Material           = buf[o++];
            data.ClickAction        = buf[o++];
            data.Scale              = BinarySerializer.DeSerializeVector3   (buf, ref o, buf.Length);
            len = buf[o++];
            data.Data1              = new byte[len];
            Array.Copy(buf, o, data.Data1, 0, len);
            o += len;

            data.ParentId           = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
            data.UpdateFlags        = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);

            data.PathCurve          = buf[o++];
            data.ProfileCurve       = buf[o++];
            data.PathBegin          = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);
            data.PathEnd            = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);
            data.PathScaleX         = buf[o++];
            data.PathScaleY         = buf[o++];
            data.PathShearX         = buf[o++];
            data.PathShearY         = buf[o++];
            data.PathTwist          = (sbyte)buf[o++];
            data.PathTwistBegin     = (sbyte)buf[o++];
            data.PathRadiusOffset   = (sbyte)buf[o++];
            data.PathTaperX         = (sbyte)buf[o++];
            data.PathTaperY         = (sbyte)buf[o++];
            data.PathRevolutions    = buf[o++];
            data.PathSkew           = (sbyte)buf[o++];
            data.ProfileBegin       = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);
            data.ProfileEnd         = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);
            data.ProfileHollow      = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);

            len                     = BinarySerializer.DeSerializeUInt16_Le (buf, ref o, length);
            for (int j = 0; j < len; j++)
            {
                data.TextureEntries.Add(buf[o++]);
            }

            len = buf[o++];
            for (int j = 0; j < len; j++)
            {
                data.TextureAnims.Add(buf[o++]);
            }

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
            data.ExtraParameters    = new byte[len];
            Array.Copy(buf, o, data.ExtraParameters, 0, len);
            o += len;

            data.SoundId            = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
            data.OwnerId            = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
            data.Gain               = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, buf.Length);
            data.SoundFlags         = (SoundFlags)buf[o++];
            data.Radius             = BinarySerializer.DeSerializeFloat_Le  (buf, ref o, length);

            data.JointType          = buf[o++];
            data.JointPivot         = BinarySerializer.DeSerializeVector3   (buf, ref o, buf.Length);
            data.JointAxisOrAnchor  = BinarySerializer.DeSerializeVector3   (buf, ref o, buf.Length);
        }
    }
    #endregion DeSerialise

    public override string ToString()
    {
        string s = $"ObjectUpdateMessage: UpdateType={UpdateType}, RegionHandle={RegionHandle}, TimeDilation={TimeDilation}";
        foreach (ObjectUpdateMessage.ObjectData data in Objects)
        {
            s += $"\n                     ObjectId={data.ObjectId}, State={data.State}"
               + $"\n                     FullId={data.FullId}, Crc={data.Crc}, PCode={data.PCode}, Material={data.Material}, ClickAction={data.ClickAction}, Scale={data.Scale}, Data1({data.Data1.Length})"
               + $"\n                     ParentId={data.ParentId}, UpdateFlags={data.UpdateFlags}"
               + $"\n                     PathCurve={data.PathCurve}, ProfileCurve={data.ProfileCurve}, Path=({data.PathBegin}-{data.PathEnd}), PathScale=({data.PathScaleX}, {data.PathScaleY}), PathShear=({data.PathShearX}, {data.PathShearY}), PathTwist={data.PathTwist}, PathTwistBegin={data.PathTwistBegin}, PathRadiusOffset={data.PathRadiusOffset}, PathTaper=({data.PathTaperX}, {data.PathTaperY}), PathRevolutions={data.PathRevolutions}, PathSkew={data.PathSkew}, Profile=({data.ProfileBegin}-{data.ProfileEnd}), Hollow={data.ProfileHollow}"
               + $"\n                     TextureEntries({data.TextureEntries.Count})"
               + $"\n                     TextureAnims({data.TextureAnims.Count})"
               + $"\n                     NameValue={data.NameValue.Replace("\n", "\\n")}, Data2({data.Data2.Length}), Text={data.Text}, TextColour={data.TextColour}, MediaUrl={data.MediaUrl}"
               + $"\n                     ParticleSystemData({data.ParticleSystemData.Length})"
               + $"\n                     ExtraParams({data.ExtraParameters.Length})"
               + $"\n                     SoundId={data.SoundId}, OwnerId={data.OwnerId}, Gain={data.Gain}, Flags={data.SoundFlags}, Radius={data.Radius}"
               + $"\n                     JointType={data.JointType}, JointPivot={data.JointPivot}, JointAxisOrAnchor={data.JointAxisOrAnchor}"
            ;
        }
        return s;
    }
}
