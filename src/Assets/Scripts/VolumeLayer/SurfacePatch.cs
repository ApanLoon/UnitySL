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
    protected UInt64 LastUpdateTime; // Time patch was last updated

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

    public void SetNeighborPatch (DirectionIndex direction, SurfacePatch neighbor)
    {
        NeighborPatches[(UInt32)direction] = neighbor;
        NormalsInvalid[(UInt32)direction] = true;
        if ((UInt32)direction < 4)
        {
            NormalsInvalid[(UInt32)Surface.DirAdjacent[(UInt32)direction, 0]] = true;
            NormalsInvalid[(UInt32)Surface.DirAdjacent[(UInt32)direction, 1]] = true;
        }
    }

}
