using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.Region;
using Assets.Scripts.Regions.Parcels;
using UnityEngine;

namespace Assets.Scripts.Regions
{
    public enum RegionMaturityLevel
    {
        A,
        M,
        PG
    }

    public class Region : IDisposable
    {

        /// <summary>
        /// The server only keeps our pending agent info for 60 seconds.
        /// We want to allow for seed cap retry, but its not useful after that 60 seconds.
        /// Give it 3 chances, each at 18 seconds to give ourselves a few seconds to connect anyways if we give up.
        /// </summary>
        public static readonly int MAX_SEED_CAP_ATTEMPTS_BEFORE_LOGIN = 3;

        /// <summary>
        /// Even though we gave up on login, keep trying for caps after we are logged in:
        /// </summary>
        public static readonly int MAX_CAP_REQUEST_ATTEMPTS = 30;


        public static readonly UInt32 DEFAULT_MAX_REGION_WIDE_PRIM_COUNT = 15000;


        public static void Initialise()
        {
            EventManager.Instance.OnRegionHandshakeMessage += OnRegionHandshakeMessage; // TODO: Perhaps split this up so that I only get triggered when this comes from the "login" circuit
        }

        public Guid Id { get; set; }

        /// <summary>
        /// The surfaces and other layers
        /// </summary>
        public Surface Land { get; set; }

        ///// <summary>
        ///// Composition layer for the surface
        ///// </summary>
        //public VolumeLayerComposition Composition { get; set; }


        #region RegionGeometryData
        /// <summary>
        /// Location of southwest corner of region (meters)
        /// </summary>
        public Vector3Double OriginGlobal { get; set; }

        /// <summary>
        /// Location of center in world space (meters)
        /// </summary>
        public Vector3Double CenterGlobal { get; set; }

        /// <summary>
        /// Width of region on a side (meters)
        /// </summary>
        public float Width { get; set; }
        #endregion RegionGeometryData

        public Host Host { get; set; }
        public RegionHandle Handle { get; set; }
    
        public string Name { get; set; }
        public string Zoning { get; set; }

        #region NetworkStatistics
        public float BitsReceived { get; set; } // F32Bits
        public float PacketsReceived { get; set; }
        public UInt32 PacketsIn { get; set; }
        public UInt32 BitsIn { get; set; } // U32Bits 
        public UInt32 LastBitsIn { get; set; } // U32Bits
        public UInt32 LastPacketsIn { get; set; }
        public UInt32 PacketsOut { get; set; }
        public UInt32 LastPacketsOut { get; set; }
        public int PacketsLost { get; set; }
        public int LastPacketsLost { get; set; }

        /// <summary>
        /// In milliseconds
        /// </summary>
        public UInt32 PingDelay { get; set; } // U32Milliseconds

        /// <summary>
        /// Time since last measurement of lastPackets, Bits, etc
        /// </summary>
        float DeltaTime { get; set; }
        #endregion NetworkStatistics

        public RegionFlags RegionFlags { get; set; }
        public RegionProtocols RegionProtocols { get; set; }
        public SimAccess SimAccess { get; set; }
        public float BillableFactor { get; set; }

        /// <summary>
        /// Max prim count
        /// </summary>
        public UInt32 MaxTasks { get; set; }

        public byte CentralBakeVersion { get; set; }

        //LLVOCacheEntry* mLastVisitedEntry;
        private UInt32 InvisibilityCheckHistory { get; set; }


        // Information for Homestead / CR-53
        public int CpuClassId { get; set; }
        public int CpuRatio { get; set; }

        public string ColoName { get; set; }
        public string ProductSku { get; set; }
        public string ProductName { get; set; }
        public string ViewerAssetUrl { get; set; }

        #region Cache
        public bool CacheLoaded { get; set; }
        public bool CacheDirty { get; set; }

        // Maps local ids to cache entries.
        // Regions can have order 10,000 objects, so assume
        // a structure of size 2^14 = 16,000
        // TODO: Cache stuff

        public UInt64 RegionCacheHitCount { get; set; }
        public UInt64 RegionCacheMissCount { get; set; }

        #endregion Cache

        #region Status
        public bool IsCurrentPlayerEstateOwner { get; set; }

        /// <summary>
        /// Can become false if circuit disconnects
        /// </summary>
        public bool Alive { get; set; }
        public bool CapabilitiesReceived { get; set; }
        public bool SimulatorFeaturesReceived { get; set; }
        public bool ReleaseNotesRequested { get; set; }

        /// <summary>
        /// If true, this region is in the process of deleting.
        /// </summary>
        public bool Dead { get; set; }

        /// <summary>
        /// Pause processing the objects in the region
        /// </summary>
        public bool Paused { get; set; }
        #endregion Status


        public Guid Owner { get; set; }
        public float WaterHeight { get; set; }
        public Guid CacheId { get; set; }
        public Guid[] TerrainBase { get; protected set; } = new Guid[4];
        public Guid[] TerrainDetail { get; protected set; } = new Guid[4];
        public float[] TerrainStartHeight { get; protected set; } = new float[4];
        public float[] TerrainHeightRange { get; protected set; } = new float[4];

        // TODO: Add RegionInfo4 when I know what it is

        public Circuit Circuit { get; set; }
    
        protected float TimeDilation;   // time dilation of physics simulation on simulator
        protected Dictionary<string, Capability> CapabilityByName { get; set; } = new Dictionary<string, Capability>();

        /// <summary>
        /// Creates a new region
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="host"></param>
        /// <param name="gridsPerRegionEdge"></param>
        /// <param name="gridsPerPatchEdge"></param>
        /// <param name="regionWidth"> in metres</param>
        public Region (RegionHandle handle, Host host, UInt32 gridsPerRegionEdge, UInt32 gridsPerPatchEdge, float regionWidth)
        {
            Host = host;
            Handle = handle;
            TimeDilation = 1.0f;

            Name   = "";
            Zoning = "";

            IsCurrentPlayerEstateOwner = false;

            RegionFlags     = RegionFlags.Default;
            RegionProtocols = RegionProtocols.None;
            SimAccess       = SimAccess.Min;

            BillableFactor = 1.0f;

            MaxTasks = DEFAULT_MAX_REGION_WIDE_PRIM_COUNT;

            CentralBakeVersion = 1;

            CpuClassId = 0;
            CpuRatio   = 0;

            ColoName       = "unknown";
            ProductSku     = "unknown";
            ProductName    = "unknown";
            ViewerAssetUrl = "";

            CacheLoaded = false;
            CacheDirty = false;

            ReleaseNotesRequested = false;
            CapabilitiesReceived = false;
            SimulatorFeaturesReceived = false;

            BitsReceived = 0f;
            PacketsReceived = 0f;

            Dead = false;
            // TODO: LastVisitedEntry = null;
            InvisibilityCheckHistory = 0xffffffff;
            Paused = false;
            RegionCacheHitCount = 0;
            RegionCacheMissCount = 0;

            Width = regionWidth;
            OriginGlobal = handle.ToVector3Double();
            //TODO: updateRenderMatrix();

            Land = new Surface (SurfaceType.Land, null); //TODO: Why not set the region right away?

            // Create the composition layer for the surface
            //Composition = new VolumeLayerComposition (Land, gridsPerRegionEdge, regionWidth / gridsPerRegionEdge);
            //Composition.SetSurface (Land);

            // Create the surfaces
            Land.SetRegion (this);
            Land.Create (gridsPerRegionEdge, gridsPerPatchEdge, OriginGlobal, Width);

            ParcelOverlay = new ParcelOverlay(this, regionWidth);
            EventManager.Instance.OnParcelOverlayMessage += OnParcelOverlayChangedMessage;

            //TODO: CalculateCenterGlobal();

            // Create the object lists
            // TODO: InitStats();

            //TODO: create object partitions
            //MUST MATCH declaration of eObjectPartitions
            //ObjectPartition.Add (new LLHUDPartition(this));        //PARTITION_HUD
            //ObjectPartition.Add (new LLTerrainPartition(this));    //PARTITION_TERRAIN
            //ObjectPartition.Add (new LLVoidWaterPartition(this));  //PARTITION_VOIDWATER
            //ObjectPartition.Add (new LLWaterPartition(this));      //PARTITION_WATER
            //ObjectPartition.Add (new LLTreePartition(this));       //PARTITION_TREE
            //ObjectPartition.Add (new LLParticlePartition(this));   //PARTITION_PARTICLE
            //ObjectPartition.Add (new LLGrassPartition(this));      //PARTITION_GRASS
            //ObjectPartition.Add (new LLVolumePartition(this)); //PARTITION_VOLUME
            //ObjectPartition.Add (new LLBridgePartition(this)); //PARTITION_BRIDGE
            //ObjectPartition.Add (new LLAvatarPartition(this)); //PARTITION_AVATAR
            //ObjectPartition.Add (new LLControlAVPartition(this));  //PARTITION_CONTROL_AV
            //ObjectPartition.Add (new LLHUDParticlePartition(this));//PARTITION_HUD_PARTICLE
            //ObjectPartition.Add (new LLVOCachePartition(this)); //PARTITION_VO_CACHE
            //ObjectPartition.Add (null);                    //PARTITION_NONE
            //VOCachePartition = getVOCachePartition();

            // TODO: setCapabilitiesReceivedCallback(boost::bind(&LLAvatarRenderInfoAccountant::scanNewRegion, _1));
        }

        #region Capabilities
        public async Task SetSeedCapability(string url)
        {
            SetCapability(new Capability(Capability.SEED_CAPABILITY_NAME)
            {
                CapabilityType = CapabilityType.Http,
                Url = url
            });

            foreach (Capability capability in await SeedCapabilities.RequestCapabilities(url))
            {
                SetCapability(capability);
            }
        }

        public Capability GetCapability(string name)
        {
            return CapabilityByName.ContainsKey(name) ? CapabilityByName[name] : null;
        }

        public void SetCapability (Capability capability)
        {
            CapabilityByName[capability.Name] = capability;
        }
        #endregion Capabilities

        public Vector3 GetLocalPosition(Vector3Double globalPosition)
        {
            return new Vector3 (
                (float)(globalPosition.x - Handle.X),
                (float)(globalPosition.y),
                (float)(globalPosition.z - Handle.Y));
        }


        #region Terrain
        /// <summary>
        ///  This gets called when the height field changes.
        /// </summary>
        public void DirtyHeights()
        {
            // Property lines need to be reconstructed when the land changes.
            //if (ParcelOverlay)
            //{
            //    ParcelOverlay.SetDirty();
            //}
        }

        protected static async void OnRegionHandshakeMessage(RegionHandshakeMessage message)
        {
            Region region = Agent.CurrentPlayer?.Region;
            if (region == null || region.Id != Guid.Empty && region.Id != message.RegionId)
            {
                return;
            }

            region.Id = message.RegionId;

            region.RegionFlags = message.RegionFlags;
            region.SimAccess = message.SimAccess;
            region.Name = message.SimName;
            region.Owner = message.SimOwner;
            region.IsCurrentPlayerEstateOwner = message.IsEstateManager;
            region.WaterHeight = message.WaterHeight;
            region.BillableFactor = message.BillableFactor;
            region.CacheId = message.CacheId;
            region.TerrainBase[0] = message.TerrainBase0;
            region.TerrainBase[1] = message.TerrainBase1;
            region.TerrainBase[2] = message.TerrainBase2;
            region.TerrainBase[3] = message.TerrainBase3;
            region.TerrainDetail[0] = message.TerrainDetail0;
            region.TerrainDetail[1] = message.TerrainDetail1;
            region.TerrainDetail[2] = message.TerrainDetail2;
            region.TerrainDetail[3] = message.TerrainDetail3;
            region.TerrainStartHeight[0] = message.TerrainStartHeight00;
            region.TerrainStartHeight[1] = message.TerrainStartHeight01;
            region.TerrainStartHeight[2] = message.TerrainStartHeight10;
            region.TerrainStartHeight[3] = message.TerrainStartHeight11;
            region.TerrainHeightRange[0] = message.TerrainHeightRange00;
            region.TerrainHeightRange[1] = message.TerrainHeightRange01;
            region.TerrainHeightRange[2] = message.TerrainHeightRange10;
            region.TerrainHeightRange[3] = message.TerrainHeightRange11;

            region.CpuClassId  = message.CpuClassId;
            region.CpuRatio    = message.CpuRatio;
            region.ColoName    = message.ColoName;
            region.ProductSku  = message.ProductSku;
            region.ProductName = message.ProductName;

            //TODO: Add RegionInfo4 when I know what it is

            //string s = "";
            //for (int i = 0; i < 4; i++)
            //{
            //    s += $"\nTerrainBase{i}:   http://asset-cdn.glb.agni.lindenlab.com/?texture_id={region.TerrainBase[i]}";
            //    s += $"\nTerrainDetail{i}: http://asset-cdn.glb.agni.lindenlab.com/?texture_id={region.TerrainDetail[i]}";
            //}
            //Logger.LogDebug("Region.OnRegionHandshakeMessage", s);

            EventManager.Instance.RaiseOnRegionDataChanged(region);

            // TODO: Load cache for the region, but should it be here?

            RegionHandshakeReplyFlags flags = 0
                                              | RegionHandshakeReplyFlags.SendAllCacheableObjects
                                              | RegionHandshakeReplyFlags.CacheFileIsEmpty 
                                              | RegionHandshakeReplyFlags.SupportsSelfAppearance;

            await region.Circuit.SendRegionHandshakeReply(Session.Instance.AgentId, Session.Instance.SessionId, flags);
        }

        public static sbyte NORMAL_PATCH_SIZE = 16;
        public static sbyte LARGE_PATCH_SIZE = 32;

        public void DecodeLandData(byte[] landData)
        {
            //LLPatchHeader ph;
            int j, i;
            Int32[] patch = new Int32[LARGE_PATCH_SIZE * LARGE_PATCH_SIZE];
            //LLSurfacePatch* patchp;

            //init_patch_decompressor(gopp->patch_size);
            //gopp->stride = mGridsPerEdge;
            //set_group_of_patch_header(gopp);

            //while (1)
            //{
            //    decode_patch_header(bitpack, &ph);
            //    if (ph.quant_wbits == END_OF_PATCHES)
            //    {
            //        break;
            //    }

            //    i = ph.patchids >> 5;
            //    j = ph.patchids & 0x1F;

            //    if ((i >= mPatchesPerEdge) || (j >= mPatchesPerEdge))
            //    {
            //        LL_WARNS() << "Received invalid terrain packet - patch header patch ID incorrect!"
            //                   << " patches per edge " << mPatchesPerEdge
            //                   << " i " << i
            //                   << " j " << j
            //                   << " dc_offset " << ph.dc_offset
            //                   << " range " << (S32)ph.range
            //                   << " quant_wbits " << (S32)ph.quant_wbits
            //                   << " patchids " << (S32)ph.patchids
            //            << LL_ENDL;
            //        return;
            //    }

            //    patchp = &mPatchList[j * mPatchesPerEdge + i];


            //    decode_patch(bitpack, patch);
            //    decompress_patch(patchp->getDataZ(), patch, &ph);

            //    // Update edges for neighbors.  Need to guarantee that this gets done before we generate vertical stats.
            //    patchp->updateNorthEdge();
            //    patchp->updateEastEdge();
            //    if (patchp->getNeighborPatch(WEST))
            //    {
            //        patchp->getNeighborPatch(WEST)->updateEastEdge();
            //    }
            //    if (patchp->getNeighborPatch(SOUTHWEST))
            //    {
            //        patchp->getNeighborPatch(SOUTHWEST)->updateEastEdge();
            //        patchp->getNeighborPatch(SOUTHWEST)->updateNorthEdge();
            //    }
            //    if (patchp->getNeighborPatch(SOUTH))
            //    {
            //        patchp->getNeighborPatch(SOUTH)->updateNorthEdge();
            //    }

            //    // Dirty patch statistics, and flag that the patch has data.
            //    patchp->dirtyZ();
            //    patchp->setHasReceivedData();
            //}

        }
        #endregion Terrain

        #region ParcelOverlay
        public ParcelOverlay ParcelOverlay { get; protected set; }
        private void OnParcelOverlayChangedMessage(ParcelOverlayMessage message)
        {
            ParcelOverlay.UpdateData(message);
            EventManager.Instance.RaiseOnParcelOverlayChanged(this);
        }
        #endregion ParcelOverlay

        public void ConnectNeighbour (Region neighbour, DirectionIndex direction)
        {
            Land.ConnectNeighbour (neighbour.Land, direction);
        }

        public void SaveObjectCache()
        {
            // TODO: Save object cache
        }

        public void Dispose()
        {
            EventManager.Instance.OnRegionHandshakeMessage -= OnRegionHandshakeMessage;
            Circuit?.Dispose();
        }
    }
}