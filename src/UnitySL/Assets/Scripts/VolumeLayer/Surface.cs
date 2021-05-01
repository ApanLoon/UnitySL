using System;
using System.Collections.Generic;
using Assets.Scripts.Regions;
using UnityEngine;

//        .   __.
//     Z /|\   /| Y                                 North
//        |   / 
//        |  /             |<----------------- mGridsPerSurfaceEdge --------------->|
//        | /              __________________________________________________________
//        |/______\ X     /_______________________________________________________  /
//                /      /      /      /      /      /      /      /M*M-2 /M*M-1 / /  
//                      /______/______/______/______/______/______/______/______/ /  
//                     /      /      /      /      /      /      /      /      / /  
//                    /______/______/______/______/______/______/______/______/ /  
//                   /      /      /      /      /      /      /      /      / /  
//                  /______/______/______/______/______/______/______/______/ /  
//      West       /      /      /      /      /      /      /      /      / /  
//                /______/______/______/______/______/______/______/______/ /     East
//               /...   /      /      /      /      /      /      /      / /  
//              /______/______/______/______/______/______/______/______/ /  
//       _.    / 2M   /      /      /      /      /      /      /      / /  
//       /|   /______/______/______/______/______/______/______/______/ /  
//      /    / M    / M+1  / M+2  / ...  /      /      /      / 2M-1 / /   
//     j    /______/______/______/______/______/______/______/______/ /   
//         / 0    / 1    / 2    / ...  /      /      /      / M-1  / /   
//        /______/______/______/______/______/______/______/______/_/   
//                                South             |<-L->|
//             i -->
//
// where M = mSurfPatchWidth
// and L = mPatchGridWidth
// 
// Notice that mGridsPerSurfaceEdge = a power of two + 1
// This provides a buffer on the east and north edges that will allow us to 
// fill the cracks between adjacent surfaces when rendering.

public enum SurfaceType : UInt32
{
    Land = 0x6c // 'l'
}

[Flags]
public enum EdgeType : byte
{
    None  = 0x00,
    East  = 0x01,
    North = 0x02,
    West  = 0x04,
    South = 0x08
}

//static const S32 ONE_MORE_THAN_NEIGHBOR = 1;
//static const S32 EQUAL_TO_NEIGHBOR = 0;
//static const S32 ONE_LESS_THAN_NEIGHBOR = -1;

public class Surface
{
    public static readonly int ABOVE_WATERLINE_ALPHA = 32;  // The alpha of water when the land elevation is above the waterline.

    // Number of grid points on one side of a region, including +1 buffer for
    // north and east edge.
    public UInt32 GridsPerEdge;

    public float OOGridsPerEdge;            // Inverse of grids per edge

    public UInt32 PatchesPerEdge;            // Number of patches on one side of a region
    public UInt32 NumberOfPatches;           // Total number of patches


    // Each surface points at 8 neighbors (or NULL)
    // +---+---+---+
    // |NW | N | NE|
    // +---+---+---+
    // | W | 0 | E |
    // +---+---+---+
    // |SW | S | SE|
    // +---+---+---+
    public Surface[] Neighbours = new Surface[8]; // Adjacent patches

    public SurfaceType SurfaceType;              // Useful for identifying derived classes

    public float DetailTextureScale;    //  Number of times to repeat detail texture across this surface 


    public Vector3Double OriginGlobal { get; protected set; }       // In absolute frame
    protected SurfacePatch[] PatchList;     // Array of all patches

    // Array of grid data, mGridsPerEdge * mGridsPerEdge
    public float[] SurfaceZ { get; protected set; } //TODO: In Indra this is protected and there are getters for individual values. I made this public so that I can access it when creating the dummy height map image.

    // Array of grid normals, mGridsPerEdge * mGridsPerEdge
    protected Vector3[] Norm;

    protected HashSet<SurfacePatch> DirtyPatchList = new HashSet<SurfacePatch>();

    // The textures should never be directly initialized - use the setter methods!
    //protected LLPointer<LLViewerTexture> SurfaceTexture;      // Texture for surface
    //protected LLPointer<LLViewerTexture> mWaterTexturep;  // Water texture

    //protected LLPointer<LLVOWater> mWaterObjp;

    // When we want multiple cameras we'll need one of each these for each camera
    protected UInt32 VisiblePatchCount;

    public UInt32 GridsPerPatchEdge { get; protected set; }         // Number of grid points on a side of a patch
    public float MetersPerGrid { get; protected set; }             // Converts (i,j) indecies to distance
    public float MetersPerEdge { get; protected set; }             // = mMetersPerGrid * (mGridsPerEdge-1)

    //protected LLPatchVertexArray mPVArray;

    public bool HasZData { get; set; }              // We've received any patch data for this surface.
    public float MinZ { get; set; }                 // min z for this region (during the session)
    public float MaxZ { get; set; }                 // max z for this region (during the session)

    public float GetZ(UInt32 k) => SurfaceZ[k];
    public float GetZ(int i, int j) => SurfaceZ[i + j * GridsPerEdge];

    protected int SurfacePatchUpdateCount;          // Number of frames since last update.


    /// <summary>
    /// Use ONLY for unit testing.
    /// </summary>
    public float[] _SurfaceZ => SurfaceZ;


    public Region Region { get; protected set; }  // Patch whose coordinate system this surface is using.

    public Surface (SurfaceType surfaceType, Region region)
    {
        SurfaceType = surfaceType;
        Region = region;

        GridsPerEdge = 0;
        OOGridsPerEdge = 0f;
        PatchesPerEdge = 0;
        NumberOfPatches = 0;
        DetailTextureScale = 0f;
        OriginGlobal = new Vector3Double (0.0, 0.0, 0.0);
        //STexturep(NULL),
        //WaterTexturep(NULL),
        GridsPerPatchEdge = 0;
        MetersPerGrid = 1.0f;
        MetersPerEdge = 1.0f;

        // Surface data
        SurfaceZ = null;
        Norm = null;

        // Patch data
        PatchList = null;

        // One of each for each camera
        VisiblePatchCount = 0;

        HasZData = false;
        // "uninitialized" min/max z
        MinZ = 10000f;
        MaxZ = -10000f;

        //WaterObj = NULL;

        // In here temporarily.
        SurfacePatchUpdateCount = 0;

        for (int i = 0; i < 8; i++)
        {
            Neighbours[i] = null;
        }
    }

    /// <summary>
    /// Assumes that arguments are powers of 2, and that
    /// gridsPerEdge / gridsPerPatchEdge = power of 2 
    /// </summary>
    /// <param name="gridsPerRegionEdge"></param>
    /// <param name="gridsPerPatchEdge"></param>
    /// <param name="originGlobal"></param>
    /// <param name="width">in metres</param>
    public void Create (UInt32 gridsPerEdge, UInt32 gridsPerPatchEdge, Vector3Double originGlobal, float width)
    {
        // Initialize various constants for the surface
        GridsPerEdge = gridsPerEdge + 1;  // Add 1 for the east and north buffer
        OOGridsPerEdge = 1f / GridsPerEdge;
        GridsPerPatchEdge = gridsPerPatchEdge;
        PatchesPerEdge = ((GridsPerEdge - 1) / GridsPerPatchEdge);
        NumberOfPatches = PatchesPerEdge * PatchesPerEdge;
        MetersPerGrid = width / (GridsPerEdge - 1);
        MetersPerEdge = MetersPerGrid * (GridsPerEdge - 1);

        OriginGlobal = originGlobal;

        //TODO: PVArray.create(GridsPerEdge, GridsPerPatchEdge, LLWorld::getInstance()->getRegionScale());

        UInt32 nGrids = GridsPerEdge * GridsPerEdge;

        // Initialize data arrays for surface
        SurfaceZ = new float[nGrids];
        Norm = new Vector3[nGrids];

        // Reset the surface to be a flat square grid
        for (int i = 0; i < nGrids; i++)
        {
            // Surface is flat and zero
            // Normals all point up
            SurfaceZ[i] = 0.0f;
            Norm[i] = new Vector3 (0f, 0f, 1f);
        }
        
        VisiblePatchCount = 0;
        
        InitTextures();

        // Has to be done after texture initialization
        CreatePatchData();
    }

    #region Textures
    protected void InitTextures()
    {
        CreateSurfaceTexture();

        // TODO: CreateWaterTexture();
        // TODO: Create water GameObject
    }

    protected void CreateSurfaceTexture()
    {
        // TODO: Generate dummy grey texture or probably use a pre-made one?
        //if (!mSTexturep)
        //{
        //    // Fill with dummy gray data.	
        //    // GL NOT ACTIVE HERE
        //    LLPointer<LLImageRaw> raw = new LLImageRaw(sTextureSize, sTextureSize, 3);
        //    U8* default_texture = raw->getData();
        //    for (S32 i = 0; i < sTextureSize; i++)
        //    {
        //        for (S32 j = 0; j < sTextureSize; j++)
        //        {
        //            *(default_texture + (i * sTextureSize + j) * 3) = 128;
        //            *(default_texture + (i * sTextureSize + j) * 3 + 1) = 128;
        //            *(default_texture + (i * sTextureSize + j) * 3 + 2) = 128;
        //        }
        //    }

        //    mSTexturep = LLViewerTextureManager::getLocalTexture(raw.get(), FALSE);
        //    mSTexturep->dontDiscard();
        //    gGL.getTexUnit(0)->bind(mSTexturep);
        //    mSTexturep->setAddressMode(LLTexUnit::TAM_CLAMP);
        //}
    }

    protected void CreatePatchData()
    {
        // Assumes GridsPerEdge, GridsPerPatchEdge, and PatchesPerEdge have been properly set
        // TODO -- check for create() called when surface is not empty
        UInt32 i;
        UInt32 j;
        SurfacePatch patch;

        // Allocate memory
        PatchList = new SurfacePatch[NumberOfPatches];
        for (i = 0; i < NumberOfPatches; i++)
        {
            PatchList[i] = new SurfacePatch();
        }

        // One of each for each camera
        VisiblePatchCount = NumberOfPatches;

        for (j = 0; j < PatchesPerEdge; j++)
        {
            for (i = 0; i < PatchesPerEdge; i++)
            {
                patch = GetPatch (i, j);
                patch.Surface = this;
            }
        }

        for (j = 0; j < PatchesPerEdge; j++)
        {
            for (i = 0; i < PatchesPerEdge; i++)
            {
                patch = GetPatch (i, j);
                patch.HasReceivedData = false;
                patch.SurfaceTextureUpdate = true;

                UInt32 dataOffset = i * GridsPerPatchEdge + j * GridsPerPatchEdge * GridsPerEdge;

                patch.SetDataZ    (SurfaceZ, dataOffset);
                patch.SetDataNorm (Norm,     dataOffset);
                
                // We make each patch point to its neighbours so we can do resolution checking 
                // when butting up different resolutions.  Patches that don't have neighbours
                // somewhere will point to NULL on that side.
                patch.SetNeighbourPatch (DirectionIndex.East,      i < PatchesPerEdge - 1                           ? GetPatch (i + 1, j    ) : null);
                patch.SetNeighbourPatch (DirectionIndex.North,                               j < PatchesPerEdge - 1 ? GetPatch (i,     j + 1) : null);
                patch.SetNeighbourPatch (DirectionIndex.West,      i > 0                                            ? GetPatch (i - 1, j    ) : null);
                patch.SetNeighbourPatch (DirectionIndex.South,                               j > 0                  ? GetPatch (i,     j - 1) : null);
                patch.SetNeighbourPatch (DirectionIndex.NorthEast, i < PatchesPerEdge - 1 && j < PatchesPerEdge - 1 ? GetPatch (i + 1, j + 1) : null);
                patch.SetNeighbourPatch (DirectionIndex.NorthWest, i > 0                  && j < PatchesPerEdge - 1 ? GetPatch (i - 1, j + 1) : null);
                patch.SetNeighbourPatch (DirectionIndex.SouthWest, i > 0                  && j > 0                  ? GetPatch (i - 1, j - 1) : null);
                patch.SetNeighbourPatch (DirectionIndex.SouthEast, i < PatchesPerEdge - 1 && j > 0                  ? GetPatch (i + 1, j - 1) : null);


                patch.OriginGlobal = new Vector3Double (               // NOTE: y and z are swapped compared to Indra because of handedness
                    OriginGlobal.x + i * MetersPerGrid * GridsPerPatchEdge,
                    0f,
                    OriginGlobal.x + j * MetersPerGrid * GridsPerPatchEdge); // TODO: Is it really correct to use .x here?
            }
        }
    }
    #endregion Textures

    public void SetRegion (Region region)
    {
        Region = region;
        //WaterObj = null; // depends on region, needs recreating
    }

    public void ConnectNeighbour (Surface neighbour, DirectionIndex direction)
    {
        UInt32 i;
        SurfacePatch patch;
        SurfacePatch neighbor_patch;

        Neighbours[(int)direction] = neighbour;
        neighbour.Neighbours[(int)World.DirOpposite[(int)direction]] = this;

        // Connect patches
        DirectionIndex oppositeDir = World.DirOpposite[(int)direction];
        switch (direction)
        {
            case DirectionIndex.NorthEast:
                patch = GetPatch (PatchesPerEdge - 1, PatchesPerEdge - 1);
                neighbor_patch = neighbour.GetPatch (0, 0);

                patch.ConnectNeighbour (neighbor_patch, direction);
                neighbor_patch.ConnectNeighbour (patch, oppositeDir);

                patch.UpdateNorthEdge(); // Only update one of north or east.
                patch.DirtyZ();
                break;

            case DirectionIndex.NorthWest:
                patch = GetPatch (0, PatchesPerEdge - 1);
                neighbor_patch = neighbour.GetPatch (PatchesPerEdge - 1, 0);

                patch.ConnectNeighbour (neighbor_patch, direction);
                neighbor_patch.ConnectNeighbour (patch, oppositeDir);
                break;

            case DirectionIndex.SouthWest:
                patch = GetPatch (0, 0);
                neighbor_patch = neighbour.GetPatch (PatchesPerEdge - 1, PatchesPerEdge - 1);

                patch.ConnectNeighbour (neighbor_patch, direction);
                neighbor_patch.ConnectNeighbour (patch, oppositeDir);

                neighbor_patch.UpdateNorthEdge(); // Only update one of north or east.
                neighbor_patch.DirtyZ();
                break;

            case DirectionIndex.SouthEast:
                patch = GetPatch (PatchesPerEdge - 1, 0);
                neighbor_patch = neighbour.GetPatch (0, PatchesPerEdge - 1);

                patch.ConnectNeighbour (neighbor_patch, direction);
                neighbor_patch.ConnectNeighbour (patch, oppositeDir);
                break;

            case DirectionIndex.East:
                // Do east/west connections, first
                for (i = 0; i < (int)PatchesPerEdge; i++)
                {
                    patch = GetPatch (PatchesPerEdge - 1, i);
                    neighbor_patch = neighbour.GetPatch (0, i);

                    patch.ConnectNeighbour (neighbor_patch, direction);
                    neighbor_patch.ConnectNeighbour (patch, oppositeDir);

                    patch.UpdateEastEdge();
                    patch.DirtyZ();
                }

                // Now do northeast/southwest connections
                for (i = 0; i < (int)PatchesPerEdge - 1; i++)
                {
                    patch = GetPatch (PatchesPerEdge - 1, i);
                    neighbor_patch = neighbour.GetPatch (0, i + 1);

                    patch.ConnectNeighbour (neighbor_patch, DirectionIndex.NorthEast);
                    neighbor_patch.ConnectNeighbour (patch, DirectionIndex.SouthWest);
                }
                // Now do southeast/northwest connections
                for (i = 1; i < (int)PatchesPerEdge; i++)
                {
                    patch = GetPatch (PatchesPerEdge - 1, i);
                    neighbor_patch = neighbour.GetPatch (0, i - 1);

                    patch.ConnectNeighbour (neighbor_patch, DirectionIndex.SouthEast);
                    neighbor_patch.ConnectNeighbour (patch, DirectionIndex.NorthWest);
                }
                break;

            case DirectionIndex.North:
                // Do north/south connections, first
                for (i = 0; i < (int)PatchesPerEdge; i++)
                {
                    patch = GetPatch (i, PatchesPerEdge - 1);
                    neighbor_patch = neighbour.GetPatch (i, 0);

                    patch.ConnectNeighbour (neighbor_patch, direction);
                    neighbor_patch.ConnectNeighbour (patch, oppositeDir);

                    patch.UpdateNorthEdge();
                    patch.DirtyZ();
                }

                // Do northeast/southwest connections
                for (i = 0; i < (int)PatchesPerEdge - 1; i++)
                {
                    patch = GetPatch (i, PatchesPerEdge - 1);
                    neighbor_patch = neighbour.GetPatch (i + 1, 0);

                    patch.ConnectNeighbour (neighbor_patch, DirectionIndex.NorthEast);
                    neighbor_patch.ConnectNeighbour (patch, DirectionIndex.SouthWest);
                }
                // Do southeast/northwest connections
                for (i = 1; i < (int)PatchesPerEdge; i++)
                {
                    patch = GetPatch (i, PatchesPerEdge - 1);
                    neighbor_patch = neighbour.GetPatch (i - 1, 0);

                    patch.ConnectNeighbour (neighbor_patch, DirectionIndex.NorthWest);
                    neighbor_patch.ConnectNeighbour (patch, DirectionIndex.SouthEast);
                }
                break;

            case DirectionIndex.West:
                // Do east/west connections, first
                for (i = 0; i < PatchesPerEdge; i++)
                {
                    patch = GetPatch (0, i);
                    neighbor_patch = neighbour.GetPatch (PatchesPerEdge - 1, i);

                    patch.ConnectNeighbour (neighbor_patch, direction);
                    neighbor_patch.ConnectNeighbour (patch, oppositeDir);

                    neighbor_patch.UpdateEastEdge();
                    neighbor_patch.DirtyZ();
                }

                // Now do northeast/southwest connections
                for (i = 1; i < PatchesPerEdge; i++)
                {
                    patch = GetPatch (0, i);
                    neighbor_patch = neighbour.GetPatch (PatchesPerEdge - 1, i - 1);

                    patch.ConnectNeighbour (neighbor_patch, DirectionIndex.SouthWest);
                    neighbor_patch.ConnectNeighbour (patch, DirectionIndex.NorthEast);
                }

                // Now do northwest/southeast connections
                for (i = 0; i < PatchesPerEdge - 1; i++)
                {
                    patch = GetPatch (0, i);
                    neighbor_patch = neighbour.GetPatch (PatchesPerEdge - 1, i + 1);

                    patch.ConnectNeighbour (neighbor_patch, DirectionIndex.NorthWest);
                    neighbor_patch.ConnectNeighbour(patch, DirectionIndex.SouthEast);
                }
                break;

            case DirectionIndex.South:
                // Do north/south connections, first
                for (i = 0; i < PatchesPerEdge; i++)
                {
                    patch = GetPatch (i, 0);
                    neighbor_patch = neighbour.GetPatch (i, PatchesPerEdge - 1);

                    patch.ConnectNeighbour(neighbor_patch, direction);
                    neighbor_patch.ConnectNeighbour(patch, oppositeDir);

                    neighbor_patch.UpdateNorthEdge();
                    neighbor_patch.DirtyZ();
                }

                // Now do northeast/southwest connections
                for (i = 1; i < PatchesPerEdge; i++)
                {
                    patch = GetPatch (i, 0);
                    neighbor_patch = neighbour.GetPatch (i - 1, PatchesPerEdge - 1);

                    patch.ConnectNeighbour (neighbor_patch, DirectionIndex.SouthWest);
                    neighbor_patch.ConnectNeighbour (patch, DirectionIndex.NorthEast);
                }
                // Now do northeast/southwest connections
                for (i = 0; i < PatchesPerEdge - 1; i++)
                {
                    patch = GetPatch (i, 0);
                    neighbor_patch = neighbour.GetPatch (i + 1, PatchesPerEdge - 1);

                    patch.ConnectNeighbour (neighbor_patch, DirectionIndex.SouthEast);
                    neighbor_patch.ConnectNeighbour(patch, DirectionIndex.NorthWest);
                }
                break;
        }
    }

    public SurfacePatch GetPatch (UInt32 x, UInt32 y)
    {
        // Note: If "below zero" it will hopefully by larger than PatchesPerEdge
        if (x < PatchesPerEdge && y < PatchesPerEdge)
        {
            return PatchList[x + y * PatchesPerEdge];
        }

        Logger.LogError("Surface.GetPatch", "Asking for patch out of bounds");
        return null;
    }

    public void DirtySurfacePatch (SurfacePatch patch)
    {
        // Put surface patch on dirty surface patch list
        DirtyPatchList.Add (patch);
    }


    public bool IdleUpdate (float maxUpdateTime)
    {
        //if (!gPipeline.hasRenderType(LLPipeline::RENDER_TYPE_TERRAIN))
        //{
        //    return FALSE;
        //}

        // Perform idle time update of non-critical stuff.
        // In this case, texture and normal updates.
        //LLTimer update_timer;
        bool didUpdate = false;

        // If the Z height data has changed, we need to rebuild our
        // property line vertex arrays.
        if (DirtyPatchList.Count > 0)
        {
            Region.DirtyHeights();
        }

        // Always call updateNormals() / updateVerticalStats()
        //  every frame to avoid artifacts

        HashSet<SurfacePatch> patchesToKeep = new HashSet<SurfacePatch>();
        foreach (SurfacePatch surfacePatch in DirtyPatchList)
        {
            // TODO: surfacePatch.UpdateNormals();
            surfacePatch.UpdateVerticalStats();
            if (maxUpdateTime > 0f
//                && update_timer.getElapsedTimeF32() < max_update_time
                && surfacePatch.UpdateTexture() == false)
            {
                patchesToKeep.Add(surfacePatch);
                continue;
            }

            didUpdate = true;
            surfacePatch.ClearDirty();
        }
        DirtyPatchList = patchesToKeep;

        return didUpdate;
    }



    /// <summary>
    /// Decompresses all the patches in the given BitPack and assigns heights and normals to the surface.
    /// </summary>
    /// <param name="bitPack"></param>
    /// <param name="groupHeader"></param>
    /// <param name="isLargePatch"></param>
    public void DecompressPatches (BitPack bitPack, GroupHeader groupHeader, bool isLargePatch)
    {
        int j;
        int i;
        int[] patchData = new int[Patch.LARGE_PATCH_SIZE * Patch.LARGE_PATCH_SIZE]; // Large enough for a maximum sized patch

        Patch.InitPatchDecompressor (groupHeader.PatchSize);
        groupHeader.Stride = (UInt16)GridsPerEdge;
        Patch.SetGroupHeader (groupHeader);

        while (true)
        {
            PatchHeader patchHeader = new PatchHeader (bitPack);
            //Logger.LogDebug("Surface.DecompressPatches", $"{patchHeader} w={patchHeader.PatchIds >> 5} h={patchHeader.PatchIds & 0x1f} (PatchesPerEdge={PatchesPerEdge})");

            if (patchHeader.IsEnd)
            {
                break;
            }

            i = patchHeader.PatchIds >> 5;
            j = patchHeader.PatchIds & 0x1f;

            if ((i >= PatchesPerEdge) || (j >= PatchesPerEdge))
            {
                //Logger.LogWarning("Surface.DecompressPatches", $"Received invalid terrain packet - patch header patch ID incorrect! {i}x{j} DcOffset={patchHeader.DcOffset} Range={patchHeader.Range} QuantWBits={patchHeader.QuantWBits} PatchIds={patchHeader.PatchIds}");
                return;
            }

            SurfacePatch surfacePatch = PatchList[j * PatchesPerEdge + i];
            Patch.Decode (bitPack, groupHeader.PatchSize, (patchHeader.QuantWBits & 0xf) + 2, patchData);

            Patch.DeCompress (SurfaceZ, surfacePatch.DataZStart, patchData, patchHeader);

            // Update edges for neighbours.  Need to guarantee that this gets done before we generate vertical stats.
            surfacePatch.UpdateNorthEdge();
            surfacePatch.UpdateEastEdge();
            if (surfacePatch.GetNeighbourPatch(DirectionIndex.West) != null)
            {
                surfacePatch.GetNeighbourPatch(DirectionIndex.West).UpdateEastEdge();
            }
            if (surfacePatch.GetNeighbourPatch(DirectionIndex.SouthWest) != null)
            {
                surfacePatch.GetNeighbourPatch(DirectionIndex.SouthWest).UpdateEastEdge();
                surfacePatch.GetNeighbourPatch(DirectionIndex.SouthWest).UpdateNorthEdge();
            }
            if (surfacePatch.GetNeighbourPatch(DirectionIndex.South) != null)
            {
                surfacePatch.GetNeighbourPatch(DirectionIndex.South).UpdateNorthEdge();
            }

            //// Dirty patch statistics, and flag that the patch has data.
            surfacePatch.DirtyZ();
            surfacePatch.HasReceivedData = true;
            //break; //TODO: Only do the first patch for testing
        }
    }
}
