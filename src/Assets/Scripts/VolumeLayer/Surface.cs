using System;
using System.Collections.Generic;
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


    protected Vector3Double OriginGlobal;       // In absolute frame
    protected SurfacePatch[] PatchList;     // Array of all patches

    // Array of grid data, mGridsPerEdge * mGridsPerEdge
    protected float[] SurfaceZ;

    // Array of grid normals, mGridsPerEdge * mGridsPerEdge
    protected Vector3[] Norm;

    protected HashSet<SurfacePatch> DirtyPatchList = new HashSet<SurfacePatch>();

    // The textures should never be directly initialized - use the setter methods!
    //protected LLPointer<LLViewerTexture> mSTexturep;      // Texture for surface
    //protected LLPointer<LLViewerTexture> mWaterTexturep;  // Water texture

    //protected LLPointer<LLVOWater> mWaterObjp;

    // When we want multiple cameras we'll need one of each these for each camera
    protected int VisiblePatchCount;

    public UInt32 GridsPerPatchEdge { get; protected set; }         // Number of grid points on a side of a patch
    protected float MetersPerGrid;             // Converts (i,j) indecies to distance
    protected float MetersPerEdge;             // = mMetersPerGrid * (mGridsPerEdge-1)

    //protected LLPatchVertexArray mPVArray;

    protected bool HasZData;             // We've received any patch data for this surface.
    protected float MinZ;                  // min z for this region (during the session)
    protected float MaxZ;                  // max z for this region (during the session)

    protected int SurfacePatchUpdateCount;					// Number of frames since last update.




    protected Region Region;  // Patch whose coordinate system this surface is using.

    public Surface(SurfaceType surfaceType, Region region)
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

    public void SetRegion (Region region)
    {
        Region = region;
        //WaterObj = null; // depends on region, needs recreating
    }

    public void DecompressDCTPatch(BitPack bitPack, PatchDct.PatchGroupHeader groupHeader, bool isLargePatch)
    {
        //int o = bitPack.Offset;
        //bitPack.Offset = o + 8 + 32 + 16;
        //byte b0 = bitPack.GetUInt8();
        //byte b1 = bitPack.GetUInt8();
        //bitPack.Offset = o + 8 + 32 + 16;
        //byte[] b = bitPack.GetBytes(10, false);
        //bitPack.Offset = o;
        //Logger.LogDebug($"b0={b0:x2} b1={b1:x2} b[0]={b[0]:x2} b[1]={b[1]:x2}");

        int j;
        int i;
        int[] patch = new int[PatchDct.LARGE_PATCH_SIZE * PatchDct.LARGE_PATCH_SIZE];

//        init_patch_decompressor(gopp->patch_size);
//        gopp->stride = mGridsPerEdge;
//        set_group_of_patch_header(gopp);

        while (true)
        {
            PatchDct.PatchHeader ph = PatchDct.PatchHeader.Create(bitPack);
            Logger.LogDebug($"Surface.DecompressDCTPatch: {ph} w={ph.PatchIds >> 5} h={ph.PatchIds & 0x1f}");
            break;
            if (ph.QuantWBits == PatchDct.END_OF_PATCHES)
            {
                break;
            }

            i = ph.PatchIds >> 5;
            j = ph.PatchIds & 0x1f;

            //if ((i >= mPatchesPerEdge) || (j >= mPatchesPerEdge))
            //{
            //    LL_WARNS() << "Received invalid terrain packet - patch header patch ID incorrect!"
            //               << " patches per edge " << mPatchesPerEdge
            //               << " i " << i
            //               << " j " << j
            //               << " dc_offset " << ph.dc_offset
            //               << " range " << (S32)ph.range
            //               << " quant_wbits " << (S32)ph.quant_wbits
            //               << " patchids " << (S32)ph.patchids
            //        << LL_ENDL;
            //    return;
            //}

            //patchp = &mPatchList[j * mPatchesPerEdge + i];


            //decode_patch(bitpack, patch);
            //decompress_patch(patchp->getDataZ(), patch, &ph);

            //// Update edges for neighbors.  Need to guarantee that this gets done before we generate vertical stats.
            //patchp->updateNorthEdge();
            //patchp->updateEastEdge();
            //if (patchp->getNeighborPatch(WEST))
            //{
            //    patchp->getNeighborPatch(WEST)->updateEastEdge();
            //}
            //if (patchp->getNeighborPatch(SOUTHWEST))
            //{
            //    patchp->getNeighborPatch(SOUTHWEST)->updateEastEdge();
            //    patchp->getNeighborPatch(SOUTHWEST)->updateNorthEdge();
            //}
            //if (patchp->getNeighborPatch(SOUTH))
            //{
            //    patchp->getNeighborPatch(SOUTH)->updateNorthEdge();
            //}

            //// Dirty patch statistics, and flag that the patch has data.
            //patchp->dirtyZ();
            //patchp->setHasReceivedData();
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
        
        //TODO: InitTextures();

        // Has to be done after texture initialization
        // TODO: CreatePatchData();
    }

    public void ConnectNeighbour(Surface neighbour, DirectionIndex direction)
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

    public SurfacePatch GetPatch(UInt32 x, UInt32 y)
    {
        // Note: If "below zero" it will hopefully by larger than PatchesPerEdge
        if (x < PatchesPerEdge && y < PatchesPerEdge)
        {
            return PatchList[x + y * PatchesPerEdge];
        }

        Logger.LogError("Surface.GetPatch: Asking for patch out of bounds");
        return null;
    }

    public void DirtySurfacePatch (SurfacePatch patch)
    {
        // Put surface patch on dirty surface patch list
        DirtyPatchList.Add (patch);
    }
}
