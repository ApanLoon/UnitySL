using System;
using UnityEngine;

public enum AgentUpdateFlags : byte
{
    None            = 0x00,
    HideTitle       = 0x01,
    ClientAutoPilot = 0x02
}

public class AgentUpdateMessage : Message
{
    public Guid AgentId { get; set; }
    public Guid SessionId { get; set; }
    public Quaternion BodyRotation { get; set; }
    public Quaternion HeadRotation { get; set; }
    public AgentState AgentState { get; set; }
    public Vector3 CameraCentre { get; set; }
    public Vector3 CameraAtAxis { get; set; }
    public Vector3 CameraLeftAxis { get; set; }
    public Vector3 CameraUpAxis { get; set; }
    public float FarClipPlane { get; set; }
    public AgentControlFlags ControlFlags { get; set; }
    public AgentUpdateFlags UpdateFlags { get; set; }

    public AgentUpdateMessage (Guid              agentId,
                               Guid              sessionId,
                               Quaternion        bodyRotation,
                               Quaternion        headRotation,
                               AgentState        agentState,
                               Vector3           cameraCentre,
                               Vector3           cameraAtAxis,
                               Vector3           cameraLeftAxis,
                               Vector3           cameraUpAxis,
                               float             farClipPlane,
                               AgentControlFlags controlFlags,
                               AgentUpdateFlags  updateFlags)
    {
        MessageId = MessageId.AgentUpdate;
        Flags = PacketFlags.Reliable; // TODO: Should be zero-coded

        AgentId        = agentId;
        SessionId      = sessionId;
        BodyRotation   = bodyRotation;
        HeadRotation   = headRotation;
        AgentState     = agentState;
        CameraCentre   = cameraCentre;
        CameraAtAxis   = cameraAtAxis;
        CameraLeftAxis = cameraLeftAxis;
        CameraUpAxis   = cameraUpAxis;
        FarClipPlane   = farClipPlane;
        ControlFlags   = controlFlags;
        UpdateFlags    = updateFlags;
    }

    #region Serialise
    public override int GetSerializedLength()
    {
        return base.GetSerializedLength()
               + 16  // AgentId
               + 16  // SessionId
               + 12  // BodyRotation
               + 12  // HeadRotation
               +  1  // AgentState
               + 12  // CameraCentre
               + 12  // CameraAtAxis
               + 12  // CameraLeftAxis
               + 12  // CameraUpAxis
               +  4  // FarClipPlane
               +  4  // ControlFlags
               +  1; // UpdateFlags
    }
    public override int Serialize(byte[] buffer, int offset, int length)
    {
        int o = offset;
        o += base.Serialize(buffer, offset, length);

        o = BinarySerializer.Serialize    (AgentId,              buffer, o, length);
        o = BinarySerializer.Serialize    (SessionId,            buffer, o, length);
        o = BinarySerializer.Serialize_Le (BodyRotation,         buffer, o, length);
        o = BinarySerializer.Serialize_Le (HeadRotation,         buffer, o, length);
        buffer[o++] = (byte) AgentState;
        o = BinarySerializer.Serialize_Le (CameraCentre,         buffer, o, length);
        o = BinarySerializer.Serialize_Le (CameraAtAxis,         buffer, o, length);
        o = BinarySerializer.Serialize_Le (CameraLeftAxis,       buffer, o, length);
        o = BinarySerializer.Serialize_Le (CameraUpAxis,         buffer, o, length);
        o = BinarySerializer.Serialize_Le (FarClipPlane,         buffer, o, length);
        o = BinarySerializer.Serialize_Le ((UInt32)ControlFlags, buffer, o, length);
        buffer[o++] = (byte)UpdateFlags;

        return o - offset;
    }
    #endregion Serialise
}
