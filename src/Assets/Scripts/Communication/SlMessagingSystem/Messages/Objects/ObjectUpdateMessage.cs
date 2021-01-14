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

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public ObjectUpdateMessage (PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
