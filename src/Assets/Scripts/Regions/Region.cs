
using System;
using System.Collections.Generic;
using UnityEngine;

public enum RegionMaturityLevel
{
    A,
    M,
    PG
}

public class Region
{
    public static Region CurrentRegion;

    public static void Initialise()
    {
        EventManager.Instance.OnRegionHandshakeMessage += OnRegionHandshakeMessage; // TODO: Perhaps split this up so that I only get triggered when this comes from the "login" circuit
    }

    public static void SetCurrentRegion(Region region)
    {
        CurrentRegion = region;
    }

    public Guid Id { get; set; }
    public string Name => SimName; // TODO: Might return the parcel name?
    public RegionHandle Handle { get; set; }
    public UInt64 RegionFlags { get; set; } // TODO: Make an enum
    public byte SimAccess { get; set; }
    public string SimName { get; set; }
    public Guid Owner { get; set; }
    public bool IsCurrentPlayerEstateOwner { get; set; }
    public float WaterHeight { get; set; }
    public float BillableFactor { get; set; }
    public Guid CacheId { get; set; }
    public Guid[] TerrainBase { get; protected set; } = new Guid[4];
    public Guid[] TerrainDetail { get; protected set; } = new Guid[4];
    public float[] TerrainStartHeight { get; protected set; } = new float[4];
    public float[] TerrainHeightRange { get; protected set; } = new float[4];

    public int CpuClassId { get; set; }
    public int CpuRatio { get; set; }
    public string ColoName { get; set; }
    public string ProductSku { get; set; }
    public string ProductName { get; set; }

    // TODO: Add RegionInfo4 when I know what it is

    public Circuit Circuit { get; set; }
    public string SeedCapability { get; set; }
    public Dictionary<string, Capability> Capabilities { get; set; }

    public Vector3 GetLocalPosition(Vector3Double globalPosition)
    {
        return new Vector3 (
            (float)(globalPosition.x - Region.CurrentRegion.Handle.X),
            (float)(globalPosition.y),
            (float)(globalPosition.z - Region.CurrentRegion.Handle.Y));
    }

    protected static async void OnRegionHandshakeMessage(RegionHandshakeMessage message)
    {
        if (CurrentRegion == null)
        {
            return;
        }

        if (CurrentRegion.Id != Guid.Empty && CurrentRegion.Id != message.RegionId)
        {
            return;
        }

        CurrentRegion.Id = message.RegionId;

        CurrentRegion.RegionFlags = message.RegionFlags;
        CurrentRegion.SimAccess = message.SimAccess;
        CurrentRegion.SimName = message.SimName;
        CurrentRegion.Owner = message.SimOwner;
        CurrentRegion.IsCurrentPlayerEstateOwner = message.IsEstateManager;
        CurrentRegion.WaterHeight = message.WaterHeight;
        CurrentRegion.BillableFactor = message.BillableFactor;
        CurrentRegion.CacheId = message.CacheId;
        CurrentRegion.TerrainBase[0] = message.TerrainBase0;
        CurrentRegion.TerrainBase[1] = message.TerrainBase1;
        CurrentRegion.TerrainBase[2] = message.TerrainBase2;
        CurrentRegion.TerrainBase[3] = message.TerrainBase3;
        CurrentRegion.TerrainDetail[0] = message.TerrainDetail0;
        CurrentRegion.TerrainDetail[1] = message.TerrainDetail1;
        CurrentRegion.TerrainDetail[2] = message.TerrainDetail2;
        CurrentRegion.TerrainDetail[3] = message.TerrainDetail3;
        CurrentRegion.TerrainStartHeight[0] = message.TerrainStartHeight00;
        CurrentRegion.TerrainStartHeight[1] = message.TerrainStartHeight01;
        CurrentRegion.TerrainStartHeight[2] = message.TerrainStartHeight10;
        CurrentRegion.TerrainStartHeight[3] = message.TerrainStartHeight11;
        CurrentRegion.TerrainHeightRange[0] = message.TerrainHeightRange00;
        CurrentRegion.TerrainHeightRange[1] = message.TerrainHeightRange01;
        CurrentRegion.TerrainHeightRange[2] = message.TerrainHeightRange10;
        CurrentRegion.TerrainHeightRange[3] = message.TerrainHeightRange11;

        CurrentRegion.CpuClassId = message.CpuClassId;
        CurrentRegion.CpuRatio = message.CpuRatio;
        CurrentRegion.ColoName = message.ColoName;
        CurrentRegion.ProductSku = message.ProductSku;
        CurrentRegion.ProductName = message.ProductName;

        //TODO: Add RegionInfo4 when I know what it is

        string s = "";
        for (int i = 0; i < 4; i++)
        {
            s += $"\nTerrainBase{i}:   http://asset-cdn.glb.agni.lindenlab.com/?texture_id={CurrentRegion.TerrainBase[i]}";
            s += $"\nTerrainDetail{i}: http://asset-cdn.glb.agni.lindenlab.com/?texture_id={CurrentRegion.TerrainDetail[i]}";
        }
        Logger.LogDebug(s);

        EventManager.Instance.RaiseOnRegionDataChanged(CurrentRegion);

        // TODO: Load cache for the region, but should it be here?

        RegionHandshakeReplyFlags flags = 0
                                          | RegionHandshakeReplyFlags.SendAllCacheableObjects
                                          | RegionHandshakeReplyFlags.CacheFileIsEmpty 
                                          | RegionHandshakeReplyFlags.SupportsSelfAppearance;

        await CurrentRegion.Circuit.SendRegionHandshakeReply(Session.Instance.AgentId, Session.Instance.SessionId, flags);
        await CurrentRegion.Circuit.SendAgentThrottle();
        await CurrentRegion.Circuit.SendAgentHeightWidth(1080, 1920); // TODO: This should not be called from here and the dimensions should take the title and status bars into account.
    }
}
