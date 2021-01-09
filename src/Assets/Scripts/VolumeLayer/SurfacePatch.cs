using System;
using UnityEngine;

public class SurfacePatch
{
    // connectivity -- each LLPatch points at 5 neighbors (or NULL)
    // +---+---+---+
    // |   | 2 | 5 |
    // +---+---+---+
    // | 3 | 0 | 1 |
    // +---+---+---+
    // | 6 | 4 |   |
    // +---+---+---+

    public bool HasReceivedData; // has the patch EVER received height data?
    public bool STexUpdate; // Does the surface texture need to be updated?

    protected SurfacePatch[] NeighborPatches = new SurfacePatch[8]; // Adjacent patches
    protected bool[] NormalsInvalid = new bool[9]; // Which normals are invalid

    protected bool Dirty;
    protected bool DirtyZStats;
    protected bool HeightsGenerated;

    protected UInt32 DataOffset;
    protected float[] DataZ;
    protected Vector3[] DataNorm;

    // Pointer to the LLVOSurfacePatch object which is used in the new renderer.
    //protected LLPointer<LLVOSurfacePatch> mVObjp;

    // All of the camera-dependent stuff should be in its own class...
    //protected LLPatchVisibilityInfo mVisInfo;

    // pointers to beginnings of patch data fields
    protected Vector3Double OriginGlobal;
    protected Vector3 OriginRegion;


    // height field stats
    protected Vector3 CenterRegion; // Center in region-local coords
    protected float MinZ, MaxZ, MeanZ;
    protected float Radius;

    protected float MinComposition;
    protected float MaxComposition;
    protected float MeanComposition;

    protected EdgeType ConnectedEdge; // This flag is non-zero iff patch is on at least one edge 

    // of LLSurface that is "connected" to another LLSurface
    protected float LastUpdateTime; // Time patch was last updated

    protected Surface Surface; // Pointer to "parent" surface

    public SurfacePatch()
    {
        HasReceivedData = false;
        STexUpdate = false;
        Dirty = false;
        DirtyZStats = true;
        HeightsGenerated = false;
        DataOffset = 0;
        DataZ = null;
        DataNorm = null;
        //VObjp = null;
        OriginRegion = new Vector3(0f, 0f, 0f);
        CenterRegion = new Vector3(0f, 0f, 0f);
        MinZ = 0f;
        MaxZ = 0f;
        MeanZ = 0f;
        Radius = 0f;
        MinComposition = 0f;
        MaxComposition = 0f;
        MeanComposition = 0f;
        // This flag is used to communicate between adjacent surfaces and is
        // set to non-zero values by higher classes.  
        ConnectedEdge = EdgeType.None;
        LastUpdateTime = 0;
        Surface = null;
        int i;
        for (i = 0; i < 8; i++)
        {
            SetNeighborPatch((DirectionIndex)i, null);
        }

        for (i = 0; i < 9; i++)
        {
            NormalsInvalid[i] = true;
        }
    }

    public SurfacePatch GetNeighbourPatch (DirectionIndex direction)
    {
        return NeighborPatches[(int)direction];
    }

    public void SetNeighborPatch (DirectionIndex direction, SurfacePatch neighbor)
    {
        NeighborPatches[(UInt32)direction] = neighbor;
        NormalsInvalid[(UInt32)direction] = true;
        if ((UInt32)direction < 4)
        {
            NormalsInvalid[(UInt32)World.DirAdjacent[(UInt32)direction, 0]] = true;
            NormalsInvalid[(UInt32)World.DirAdjacent[(UInt32)direction, 1]] = true;
        }
    }

    public void DirtyZ()
    {
        STexUpdate = true;

        // Invalidate all normals in this patch
        UInt32 i;
        for (i = 0; i < 9; i++)
        {
            NormalsInvalid[i] = true;
        }

        // Invalidate normals in this and neighbouring patches
        for (i = 0; i < 8; i++)
        {
            DirectionIndex dir = (DirectionIndex) i;
            if (GetNeighbourPatch (dir) != null)
            {
                GetNeighbourPatch (dir).NormalsInvalid[(int)World.DirOpposite[i]] = true;
                GetNeighbourPatch (dir).SetDirty();
                if (i < 4)
                {
                    GetNeighbourPatch (dir).NormalsInvalid[(int)World.DirAdjacent[(int)World.DirOpposite[i], 0]] = true;
                    GetNeighbourPatch (dir).NormalsInvalid[(int)World.DirAdjacent[(int)World.DirOpposite[i], 1]] = true;
                }
            }
        }

        SetDirty();
        LastUpdateTime = Time.time;
    }

    public void SetDirty()
    {
        // These are outside of the loop in case we're still waiting for a dirty from the
        // texture being updated...
        // TODO: 
        //if (VObjp)
        //{
        //    VObjp.DirtyGeom();
        //}
        //else
        //{
        //    Logger.LogWarning("Terrain: No GameObject for this surface patch!");
        //}

        DirtyZStats = true;
        HeightsGenerated = false;

        if (!Dirty)
        {
            Dirty = true;
            Surface.DirtySurfacePatch (this);
        }
    }

    public void ConnectNeighbour (SurfacePatch neighbour, DirectionIndex direction)
    {
        if (neighbour == null)
        {
            // TODO: Should I throw an exception?
            return;
        }
        
        NormalsInvalid[(int)direction] = true;
        neighbour.NormalsInvalid[(int)World.DirOpposite[(int)direction]] = true;

        SetNeighborPatch(direction, neighbour);
        neighbour.SetNeighborPatch (World.DirOpposite[(int)direction], this);

        switch (direction)
        {
            case DirectionIndex.East:
                ConnectedEdge |= EdgeType.East;
                neighbour.ConnectedEdge |= EdgeType.West;
                break;

            case DirectionIndex.North:
                ConnectedEdge |= EdgeType.North;
                neighbour.ConnectedEdge |= EdgeType.South;
                break;

            case DirectionIndex.West:
                ConnectedEdge |= EdgeType.West;
                neighbour.ConnectedEdge |= EdgeType.East;
                break;

            case DirectionIndex.South:
                ConnectedEdge |= EdgeType.South;
                neighbour.ConnectedEdge |= EdgeType.North;
                break;
        }
    }

    public void UpdateNorthEdge()
    {
        UInt32 gridsPerPatchEdge = Surface.GridsPerPatchEdge;
        UInt32 gridsPerEdge = Surface.GridsPerEdge;

        UInt32 i;
        UInt32 southSurfaceIndex;
        UInt32 northSurfaceIndex;
        float[] northSource = DataZ;

        if (GetNeighbourPatch (DirectionIndex.North) != null)
        {
            southSurfaceIndex = gridsPerPatchEdge * gridsPerEdge;
            northSurfaceIndex = (gridsPerPatchEdge - 1) * gridsPerEdge;
        }
        else if ((ConnectedEdge & EdgeType.North) != 0)
        {
            southSurfaceIndex = gridsPerPatchEdge * gridsPerEdge;
            northSurfaceIndex = 0;
            northSource = GetNeighbourPatch (DirectionIndex.North).DataZ;
        }
        else
        {
            return;
        }

        // Update patch's north edge ...
        for (i = 0; i < gridsPerPatchEdge; i++)
        {
            DataZ[southSurfaceIndex + i] = northSource[northSurfaceIndex + i];    // update buffer Z
        }
    }

    public void UpdateEastEdge()
    {
        UInt32 gridsPerPatchEdge = Surface.GridsPerPatchEdge;
        UInt32 gridsPerEdge = Surface.GridsPerEdge;

        UInt32 j;
        UInt32 k;
        UInt32 westSurfaceIndex;
        UInt32 eastSurfaceIndex;
        float[] eastSource = DataZ;

        if (GetNeighbourPatch (DirectionIndex.East) != null)
        {
            westSurfaceIndex = gridsPerPatchEdge;
            eastSurfaceIndex = gridsPerPatchEdge - 1;
        }
        else if ((ConnectedEdge & EdgeType.East) != 0)
        {
            westSurfaceIndex = gridsPerPatchEdge;
            eastSurfaceIndex = 0;
            eastSource = GetNeighbourPatch (DirectionIndex.East).DataZ;
        }
        else
        {
            return;
        }

        // If patch is on the east edge of its surface, then we update the east
        // side buffer
        for (j = 0; j < gridsPerPatchEdge; j++)
        {
            k = j * gridsPerEdge;
            DataZ[westSurfaceIndex + k] = eastSource[eastSurfaceIndex + k];  // update buffer Z
        }
    }
}
