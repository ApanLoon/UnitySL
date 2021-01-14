using System;
using System.Collections.Generic;
using UnityEngine;

public enum ViewerEffectType : byte
{
    Text,
    Icon,
    Connector,
    FlexibleObject,
    AnimalControls,
    LocalAnimationObject,
    Cloth,
    EffectBeam,
    EffectGlow,
    EffectPoint,
    EffectTrail,
    EffectSphere,
    EffectSpiral,
    EffectEdit,
    EffectLookAt,
    EffectPointAt,
    EffectVoiceViaualizer,
    NameTag,
    EffectBlob
}

public class ViewerEffect
{
    public Guid Id { get; set; }
    public Guid AgentId { get; set; }
    public ViewerEffectType EffectType { get; set; }
    public float Duration { get; set; }
    public Color Color { get; set; }
}

public class ViewerEffectSpiral : ViewerEffect
{
    public Guid SourceObjectId { get; set; }
    public Guid TargetObjectId { get; set; }

    public Vector3Double PositionGlobal { get; set; }
}

public enum ViewerEffectLookAtType : byte
{
    None,
    Idle,
    AutoListen,
    FreeLook,
    Respond,
    Hover,
    Conversation,
    Select,
    Focus,
    MouseLook,
    Clear
}

public class ViewerEffectLookAt : ViewerEffect
{
    public Guid SourceAvatarId { get; set; }
    public Guid TargetObjectId { get; set; }
    public Vector3Double TargetPosition { get; set; }
    public ViewerEffectLookAtType LookAtType { get; set; }
}

public class ViewerEffectMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }

    public List<ViewerEffect> Effects { get; protected set; } = new List<ViewerEffect>();

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public ViewerEffectMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }
}
