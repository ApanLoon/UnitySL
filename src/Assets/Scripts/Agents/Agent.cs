
using System;
using System.Collections.Generic;
using UnityEngine;

public enum AgentState : byte
{
    Typing = 0x04,
    Editing = 0x10
}

[Flags]
public enum AgentControlFlags : UInt32
{
    AtPos                   = 0x00000001,
    AtNeg                   = 0x00000002,
    LeftPos                 = 0x00000004,
    LeftNeg                 = 0x00000008,
    UpPos                   = 0x00000010,
    UpNeg                   = 0x00000020,
    PitchPos                = 0x00000040,
    PitchNeg                = 0x00000080,
    YawPos                  = 0x00000100,
    YawNeg                  = 0x00000200,

    FastAt                  = 0x00000400,
    FastLeft                = 0x00000800,
    FastUp                  = 0x00001000,

    Fly                     = 0x00002000,
    Stop                    = 0x00004000,
    FinishAnim              = 0x00008000,
    StandUp                 = 0x00010000,
    SitOnGround             = 0x00020000,
    MouseLook               = 0x00040000,

    NudgeAtPos              = 0x00080000,
    NudgeAtNeg              = 0x00100000,
    NudgeLeftPos            = 0x00200000,
    NudgeLeftNeg            = 0x00400000,
    NudgeUpPos              = 0x00800000,
    NudgeUpNeg              = 0x01000000,
    TurnLeft                = 0x02000000,
    TurnRight               = 0x04000000,

    Away                    = 0x08000000,

    LeftButtonDown          = 0x10000000,
    LeftButtonUp            = 0x20000000,
    MouseLookLeftButtonDown = 0x40000000,
    MouseLookLeftButtonUp   = 0x80000000
}

public class Agent :IDisposable
{
    public static Agent CurrentPlayer;

    public static void SetCurrentPlayer(Agent agent)
    {
        AddAgent(agent.Id, agent);
        CurrentPlayer = agent;
    }

    protected static Dictionary<Guid, Agent> AgentById = new Dictionary<Guid, Agent>();

    public static void AddAgent(Guid id, Agent agent)
    {
        AgentById[id] = agent;
    }
    public static void RemoveAgent(Guid id)
    {
        if (AgentById.ContainsKey(id) == true)
        {
            AgentById.Remove(id);
        }
    }

    public Agent GetAgentById(Guid id)
    {
        return AgentById.ContainsKey(id) == false ? null : AgentById[id];
    }

    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }

    public Guid ActiveGroupId { get; set; }
    public string GroupName { get; set; }
    public string GroupTitle { get; set; }
    public UInt64 GroupPowers { get; set; }
    public Region CurrentRegion { get; set; }


    public Vector3 Position { get; set; }
    public Vector3 LookAt { get; set; }
    public Quaternion BodyRotation { get; set; } = Quaternion.identity;
    public Quaternion HeadRotation { get; set; } = Quaternion.identity;

    public Vector3 CameraCentre   { get; set; }
    public Vector3 CameraAtAxis   { get; set; }
    public Vector3 CameraLeftAxis { get; set; }
    public Vector3 CameraUpAxis   { get; set; }
    public float FarClipPlane { get; set; }

    public float             Health { get; set; }
    public AgentState        AgentState { get; set; }
    public AgentControlFlags ControlFlags { get; set; }



    public Agent(Guid id)
    {
        Id = id;
        EventManager.Instance.OnHealthMessage += OnHealthMessage;
        EventManager.Instance.OnAgentDataUpdateMessage += OnAgentDataUpdateMessage;
        EventManager.Instance.OnAgentMovementCompleteMessage += OnAgentMovementCompleteMessage;
    }

    protected void OnHealthMessage(HealthMessage message)
    {
        // TODO: There shouldn't be a health message listener for every agent!
        CurrentPlayer.Health = message.Health;
        EventManager.Instance.RaiseOnHealthChanged (CurrentPlayer);
    }

    protected async void OnAgentMovementCompleteMessage(AgentMovementCompleteMessage message) // TODO: Should this even be here?
    {
        // TODO: Check if AgentId and SessionId makles sense
        if (message.AgentId != Id)
        {
            return;
        }

        // TODO: Check timestamp to make sure the movement compleation makes sense.

        Position = message.Position;
        LookAt = message.LookAt;

        // TODO: Check that the avatar is valid

        // TODO: Check if the region is valid, disconnect if not

        // TODO: Update current region and host

        await Region.CurrentRegion.Circuit.SendAgentThrottle();

        await Region.CurrentRegion.Circuit.SendAgentHeightWidth(1080, 1920); // TODO: This should take the title and status bars into account.

        // if (Teleport)
        // {
        // TODO: Force camera to new location and look at
        // TODO: Force agent position
        // }
        // else if (initial login)
        // {
        // // This is initial log-in, not a region crossing:
        // Set the camera looking ahead of the AV so send_agent_update() below 
        // will report the correct location to the server.
        // }
        // 
        EventManager.Instance.RaiseOnAgentMoved(this);

        // TODO: Check beacon and remove if we are close

        Region.CurrentRegion.Circuit.SendAgentUpdate (Id,
                                                            Session.Instance.SessionId,
                                                            BodyRotation,
                                                            HeadRotation,
                                                            AgentState,
                                                            CameraCentre,
                                                            CameraAtAxis,
                                                            CameraLeftAxis,
                                                            CameraUpAxis,
                                                            FarClipPlane,
                                                            ControlFlags,
                                                            AgentUpdateFlags.None);

        // TODO: Force flying depending on region permissions

        // TODO: Force simulator to recognise do not disturb mode

        // TODO: Send current walk/run animation
    }

    protected void OnAgentDataUpdateMessage(AgentDataUpdateMessage message)
    {
        if (message.AgentId != Id)
        {
            return;
        }

        FirstName = message.FirstName;
        LastName = message.LastName;
        GroupTitle = message.GroupTitle;
        ActiveGroupId = message.ActiveGroupId;
        GroupPowers = message.GroupPowers;
        GroupName = message.GroupName;

        EventManager.Instance.RaiseOnAgentDataChanged(this);
    }

    public void Dispose()
    {
        EventManager.Instance.OnAgentDataUpdateMessage -= OnAgentDataUpdateMessage;
    }
}
