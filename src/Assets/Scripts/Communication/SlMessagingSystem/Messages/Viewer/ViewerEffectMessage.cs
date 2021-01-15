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

    public override string ToString()
    {
        return $"Id={Id}, AgentId={AgentId}, Type={EffectType}, Duration={Duration}, Colour={Color}";
    }
}

public class ViewerEffectSpiral : ViewerEffect
{
    public Guid SourceObjectId { get; set; }
    public Guid TargetObjectId { get; set; }

    public Vector3Double PositionGlobal { get; set; }
    public override string ToString()
    {
        return $"Spiral: {base.ToString()}, SourceObjectId={SourceObjectId}, TargetObjectId={TargetObjectId}, PositionGlobal={PositionGlobal}";
    }
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
    public override string ToString()
    {
        return $"LookAt: {base.ToString()}, SourceAvatarId={SourceAvatarId}, TargetObjectId={TargetObjectId}, TargetPosition={TargetPosition}, LookAtType={LookAtType}";
    }

}

public class ViewerEffectMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }

    public List<ViewerEffect> Effects { get; protected set; } = new List<ViewerEffect>();

    public ViewerEffectMessage()
    {
        MessageId = MessageId.ViewerEffect;
        Flags = 0;
    }

    #region DeSerialise
    protected override void DeSerialise(byte[] buf, ref int o, int length)
    {
        AgentId                   = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
        SessionId                 = BinarySerializer.DeSerializeGuid     (buf, ref o, length);

        byte nEffects             = buf[o++];
        for (byte i = 0; i < nEffects; i++)
        {
            Guid effectId         = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            Guid agentId          = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            ViewerEffectType type = (ViewerEffectType)buf[o++];
            float duration        = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            Color color           = BinarySerializer.DeSerializeColor    (buf, ref o, length);

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
                    spiralEffect.SourceObjectId = BinarySerializer.DeSerializeGuid(buf, ref o, length);
                    spiralEffect.TargetObjectId = BinarySerializer.DeSerializeGuid(buf, ref o, length);
                    spiralEffect.PositionGlobal = BinarySerializer.DeSerializeVector3Double(buf, ref o, length);
                    effect = spiralEffect;
                    break;

                case ViewerEffectType.EffectLookAt:
                    ViewerEffectLookAt lookAtEffect = new ViewerEffectLookAt();
                    lookAtEffect.SourceAvatarId = BinarySerializer.DeSerializeGuid(buf, ref o, length);
                    lookAtEffect.TargetObjectId = BinarySerializer.DeSerializeGuid(buf, ref o, length);
                    lookAtEffect.TargetPosition = BinarySerializer.DeSerializeVector3Double(buf, ref o, length);
                    lookAtEffect.LookAtType = (ViewerEffectLookAtType)buf[o++];
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
            Effects.Add(effect);
        }
    }
    #endregion DeSerialise

    public override string ToString()
    {
        string s = $"{base.ToString()}:";
        foreach (ViewerEffect effect in Effects)
        {
            s += $"\n    {effect}";
        }
        return s;
    }
}
