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
    public bool SurfaceTextureUpdate; // Does the surface texture need to be updated?

    protected SurfacePatch[] NeighborPatches = new SurfacePatch[8]; // Adjacent patches
    protected bool[] NormalsInvalid = new bool[9]; // Which normals are invalid

    protected bool Dirty;
    protected bool DirtyZStats;
    protected bool HeightsGenerated;

    protected UInt32 DataOffset;

    /// <summary>
    /// Contains the Z values for the entire surface, make sure that you use the DataZStart offset every time you access this.
    /// </summary>
    protected float[] DataZ;
    public UInt32 DataZStart { get; set; }

    /// <summary>
    /// Contains the Normal vectors for the entire surface, make sure that you use the DataNormStart offset every time you access this.
    /// </summary>
    protected Vector3[] DataNorm;
    public UInt32 DataNormStart { get; protected set; }

    // Pointer to the LLVOSurfacePatch object which is used in the new renderer.
    //protected LLPointer<LLVOSurfacePatch> mVObjp;

    // All of the camera-dependent stuff should be in its own class...
    //protected LLPatchVisibilityInfo mVisInfo;

    // pointers to beginnings of patch data fields
    public Vector3Double OriginGlobal { get; set; }
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

    public Surface Surface; // Pointer to "parent" surface

    public SurfacePatch()
    {
        HasReceivedData = false;
        SurfaceTextureUpdate = false;
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
            SetNeighbourPatch((DirectionIndex)i, null);
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

    public void SetNeighbourPatch (DirectionIndex direction, SurfacePatch neighbor)
    {
        NeighborPatches[(UInt32)direction] = neighbor;
        NormalsInvalid[(UInt32)direction] = true;
        if ((UInt32)direction < 4)
        {
            NormalsInvalid[(UInt32)World.DirAdjacent[(UInt32)direction, 0]] = true;
            NormalsInvalid[(UInt32)World.DirAdjacent[(UInt32)direction, 1]] = true;
        }
    }


    // Called when a patch has changed its height field
    // data.
    public void UpdateVerticalStats()
    {
        if (DirtyZStats == false)
        {
            return;
        }

        UInt32 grids_per_patch_edge = Surface.GridsPerPatchEdge;
        UInt32 grids_per_edge = Surface.GridsPerEdge;
        float meters_per_grid = Surface.MetersPerGrid;

        UInt32 i;
        UInt32 j;
        UInt32 k;
        float z;
        float total;

//        llassert(mDataZ);
        z = DataZ[0];

        MinZ = z;
        MaxZ = z;

        k = 0;
        total = 0.0f;

        // Iterate to +1 because we need to do the edges correctly.
        for (j = 0; j < (grids_per_patch_edge + 1); j++)
        {
            for (i = 0; i < (grids_per_patch_edge + 1); i++)
            {
                z = DataZ[i + j * grids_per_edge];

                MinZ = Mathf.Min(z, MinZ);
                MaxZ = Mathf.Max(z, MaxZ);
                total += z;
                k++;
            }
        }
        MeanZ = total / (float)k;
        CenterRegion.y = 0.5f * (MinZ + MaxZ);

        Vector3 diam_vec = new Vector3 (meters_per_grid * grids_per_patch_edge,
                                        meters_per_grid* grids_per_patch_edge,
                                        MaxZ - MinZ);
        Radius = diam_vec.magnitude * 0.5f;

        Surface.MaxZ = Mathf.Max (MaxZ, Surface.MaxZ);
        Surface.MinZ = Mathf.Min (MinZ, Surface.MinZ);
        Surface.HasZData = true;
        // TODO: Surface.Region.CalculateCenterGlobal();

        //if (mVObjp)
        //{
        //    mVObjp->dirtyPatch();
        //}
        DirtyZStats = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>true if we did NOT update the texture</returns>
    public bool UpdateTexture()
    {
        if (SurfaceTextureUpdate == false)
        {
            return true;
        }

        float metersPerGrid = Surface.MetersPerGrid;
        float gridsPerPatchEdge = (float)Surface.GridsPerPatchEdge;

        if (   GetNeighbourPatch(DirectionIndex.East)  == null && !GetNeighbourPatch (DirectionIndex.East).HasReceivedData
            || GetNeighbourPatch(DirectionIndex.West)  == null && !GetNeighbourPatch (DirectionIndex.West).HasReceivedData
            || GetNeighbourPatch(DirectionIndex.South) == null && !GetNeighbourPatch (DirectionIndex.South).HasReceivedData
            || GetNeighbourPatch(DirectionIndex.North) == null && !GetNeighbourPatch (DirectionIndex.North).HasReceivedData)
        {
            return false;
        }

        Region region = Surface.Region;
        Vector3Double originRegion = OriginGlobal.Subtract (Surface.OriginGlobal);

        // Have to figure out a better way to deal with these edge conditions...

        //if (HeightsGenerated == true)
        //{
        //    return false;
        //}

        float patchSize = metersPerGrid * (gridsPerPatchEdge + 1);
        //TODO: VLComposition comp = region.Composition;
        // TODO: if (comp.GenerateHeights ((float) originRegion.x, (float) originRegion.y, patchSize, patchSize) == false)  // TODO: Should y be z?
        //{
        //    return false;
        //}

        HeightsGenerated = true;

        // TODO: if (comp.GenerateComposition())
        //{
        //    if (VObjp)
        //    {
        //        VObjp->dirtyGeom();
        //        Pipeline.markGLRebuild(mVObjp);
        //        return true;
        //    }
        //}

        return false;
    }


    public void DirtyZ()
    {
        SurfaceTextureUpdate = true;

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

        // TODO: if (VObjp)
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

        SetNeighbourPatch(direction, neighbour);
        neighbour.SetNeighbourPatch (World.DirOpposite[(int)direction], this);

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
        UInt32 southSurfaceIndex = gridsPerPatchEdge *gridsPerEdge + DataZStart;
        UInt32 northSurfaceIndex;
        float[] northSource = null;

        if (GetNeighbourPatch (DirectionIndex.North) != null)
        {
            northSource = DataZ;
            northSurfaceIndex = (gridsPerPatchEdge - 1) * gridsPerEdge + DataZStart;
        }
        else if ((ConnectedEdge & EdgeType.North) != 0)
        {
            SurfacePatch neighbour = GetNeighbourPatch(DirectionIndex.North);
            northSource       =     neighbour.DataZ;
            northSurfaceIndex = 0 + neighbour.DataZStart;
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
        UInt32 westSurfaceIndex = gridsPerPatchEdge + DataZStart;
        UInt32 eastSurfaceIndex;
        float[] eastSource = DataZ;

        if (GetNeighbourPatch (DirectionIndex.East) != null)
        {
            eastSurfaceIndex = gridsPerPatchEdge - 1 + DataZStart;
        }
        else if ((ConnectedEdge & EdgeType.East) != 0)
        {
            SurfacePatch neighbour = GetNeighbourPatch(DirectionIndex.East);
            eastSource       =     neighbour.DataZ;
            eastSurfaceIndex = 0 + neighbour.DataZStart;
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

    public void SetDataZ(float[] dataZ, UInt32 dataZStart)
    {
        DataZ = dataZ;
        DataZStart = dataZStart;
    }

    public void SetDataNorm(Vector3[] dataNorm, UInt32 dataNormStart)
    {
        DataNorm = dataNorm;
        DataNormStart = dataNormStart;
    }

    public void ClearDirty()
    {
        Dirty = false;
    }

}
