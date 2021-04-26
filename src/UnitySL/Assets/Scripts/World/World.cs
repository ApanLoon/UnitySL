using System;
using System.Collections.Generic;
using Assets.Scripts.Regions;

public enum DirectionIndex : UInt32
{
    East  = 0,
    North = 1,
    West  = 2,
    South = 3,

    NorthEast = 4,
    NorthWest = 5,
    SouthWest = 6,
    SouthEast = 7,
    Middle    = 8,
    Invalid   = 0xffffffff
}

[Flags]
public enum DirectionFlag : byte
{
    East  = 1 << (int)DirectionIndex.East,
    North = 1 << (int)DirectionIndex.North,
    West  = 1 << (int)DirectionIndex.West,
    South = 1 << (int)DirectionIndex.South,

    NorthEast = North | East,
    NorthWest = North | West,
    SouthWest = South | West,
    SouthEast = South | East
}

public class World
{
    public static readonly UInt32 WORLD_PATCH_SIZE = 16;
    public static readonly UInt32 WIDTH = 256;

    /// <summary>
    /// Metres/point, therefore mWidth * mScale = meters per edge
    /// </summary>
    public static readonly float SCALE = 1f;

    public static readonly float WIDTH_IN_METRES = WIDTH * SCALE;


    public static readonly DirectionIndex[] DirOpposite =
    {
        DirectionIndex.West,      DirectionIndex.South,     DirectionIndex.East,      DirectionIndex.North,
        DirectionIndex.SouthWest, DirectionIndex.SouthEast, DirectionIndex.NorthEast, DirectionIndex.NorthWest
    };

    public static readonly DirectionIndex[,] DirAdjacent =
    {
        {DirectionIndex.NorthEast, DirectionIndex.SouthEast},
        {DirectionIndex.NorthEast, DirectionIndex.NorthWest},
        {DirectionIndex.NorthWest, DirectionIndex.SouthWest},
        {DirectionIndex.SouthWest, DirectionIndex.SouthEast},
        {DirectionIndex.East,      DirectionIndex.North},
        {DirectionIndex.North,     DirectionIndex.West},
        {DirectionIndex.West,      DirectionIndex.South},
        {DirectionIndex.East,      DirectionIndex.South}
    };

    /// <summary>
    /// Magnitude along the x and y axis 
    /// </summary>
    public static readonly int[,] DirectionAxes =
    {
        { 1, 0}, // east
        { 0, 1}, // north
        {-1, 0}, // west
        { 0,-1}, // south
        { 1, 1}, // ne
        {-1, 1}, // nw
        {-1,-1}, // sw
        { 1,-1}, // se
    };

    public static DirectionFlag[] DirectionMasks = 
    {
        DirectionFlag.East,
        DirectionFlag.North,
        DirectionFlag.West,
        DirectionFlag.South,
        DirectionFlag.NorthEast,
        DirectionFlag.NorthWest,
        DirectionFlag.SouthWest,
        DirectionFlag.SouthEast
    };


    public static World Instance = new World();

    protected Dictionary<RegionHandle, Region> RegionByHandle    = new Dictionary<RegionHandle, Region>();
    protected Dictionary<Host, Region>         RegionByHost      = new Dictionary<Host, Region>();
    protected List<Region>                     RegionList        = new List<Region>();
    protected List<Region>                     ActiveRegionList  = new List<Region>();
    protected List<Region>                     CulledRegionList  = new List<Region>();
    protected List<Region>                     VisibleRegionList = new List<Region>();

    public Region AddRegion(RegionHandle handle, Host host)
    {
        Region region;
        Capability seedCapability = null;

        if (RegionByHandle.ContainsKey(handle))
        {
            // Region already exists
            region = RegionByHandle[handle];
            Host oldHost = region.Host;
            if (host == oldHost && region.Alive)
            {
                Logger.LogInfo($"Region with handle {handle} already exists and is alive, using existing region.");
                return region;
            }

            if (host != oldHost)
            {
                Logger.LogWarning($"Region with handle {handle} already exists but with a different host. Removing and creating new.");
            }
            if (region.Alive == false)
            {
                Logger.LogWarning($"Region with handle {handle} already exists but it isn't alive. Removing and creating new.");
            }

            // Save capabilities seed URL
            seedCapability = region.GetCapability (Capability.SEED_CAPABILITY_NAME);

            // Kill the old host, and then we can continue on and add the new host.  We have to kill even if the host
            // matches, because all the agent state for the new camera is completely different.
            RemoveRegion (oldHost);
        }
        else
        {
            Logger.LogInfo($"Region with handle {handle} does not exist, creating a new one.");
        }

        UInt32 iindex = handle.X;
        UInt32 jindex = handle.Y;

        int x = (int)(iindex / WIDTH);
        int y = (int)(jindex / WIDTH);

        Logger.LogInfo($"Adding new region {handle} on {host}.");

        Vector3Double origin_global = handle.ToVector3Double();

        region = new Region (handle, host, WIDTH, WORLD_PATCH_SIZE, WIDTH_IN_METRES);

        if (seedCapability != null)
        {
            region.SetCapability (seedCapability);
        }

        RegionList.Add       (region);
        ActiveRegionList.Add (region);
        CulledRegionList.Add (region);
        RegionByHandle[handle] = region;
        RegionByHost[host]     = region;

        // Find all the adjacent regions, and attach them.
        // Generate handles for all of the adjacent regions, and attach them in the correct way.
        // connect the edges
        float adj_x = 0f;
        float adj_y = 0f;
        float region_x = handle.X;
        float region_y = handle.Y;
        RegionHandle adj_handle = new RegionHandle(0);

        float width = WIDTH_IN_METRES;

        // Iterate through all directions, and connect neighbors if there.
        for (int dir = 0; dir < 8; dir++)
        {
            adj_x = region_x + width * DirectionAxes[dir, 0];
            adj_y = region_y + width * DirectionAxes[dir, 1];
            if (adj_x >= 0)
            {
                adj_handle.X = (UInt32)adj_x;
            }
            if (adj_y >= 0)
            {
                adj_handle.Y = (UInt32)adj_y;
            }

            if (RegionByHandle.ContainsKey(adj_handle))
            {
                region.ConnectNeighbour (RegionByHandle[adj_handle], (DirectionIndex)dir);
            }
        }

        // TODO: UpdateWaterObjects();

        return region;
	}

    public void RemoveRegion (Host host)
    {
        Region region = GetRegion (host);
        if (region == null)
        {
            Logger.LogWarning("Trying to remove region that doesn't exist!");
            return;
        }

        Agent agent = Agent.CurrentPlayer;
        if (region == agent.Region)
        {
            foreach (Region r in RegionList)
            {
                Logger.LogWarning($"RegionDump: {r.Host} {r.OriginGlobal}");
            }

            Logger.LogWarning($"Agent position global {agent.OriginGlobal} agent {agent.Position}");
            // TODO: Logger.LogWarning($"Regions visited {agent.RegionsVisited}");
            // TODO: Logger.LogWarning($"FrameTimeSeconds {FrameTimeSeconds}");
            Logger.LogWarning($"Disabling region {region.Name} that agent is in!");
            
            // TODO: await Session.Instance.Stop ("You have been disconnected.");

            region.SaveObjectCache(); //force to save objects here in case that the object cache is about to be destroyed.
            return;
        }

        float x = region.Handle.X;
        float y = region.Handle.Y;

        Logger.LogInfo($"Removing region {x}:{y}");

        RegionByHandle.Remove    (region.Handle);
        RegionByHost.Remove      (host);
        RegionList.Remove        (region);
        ActiveRegionList.Remove  (region);
        CulledRegionList.Remove  (region);
        VisibleRegionList.Remove (region);

        EventManager.Instance.RaiseOnRegionRemoved (region);

        region.Dispose();

        // TODO: UpdateWaterObjects();

        // Make sure that all objects of this region are removed:
        //TODO: ObjectList.ClearAllMapObjectsInRegion (region);
    }

    public Region GetRegion(Host host)
    {
        return RegionByHost.ContainsKey(host) ? RegionByHost[host] : null;
    }
}
