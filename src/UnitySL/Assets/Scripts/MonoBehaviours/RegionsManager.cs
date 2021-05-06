using System.Collections.Generic;
using Assets.Scripts.Regions;
using UnityEngine;

namespace Assets.Scripts.MonoBehaviours
{
    public class RegionsManager : MonoBehaviour
    {
        [SerializeField] protected GameObject RegionPrefab;

        protected class RegionGo
        {
            public GameObject Root;
            public Terrain Terrain;
            public Water Water;
        }

        protected Dictionary<Region, RegionGo> RegionGoByRegion = new Dictionary<Region, RegionGo>();

        protected RegionGo CurrentRegionGo;

        private void OnEnable()
        {
            if (Agent.CurrentPlayer != null)
            {
                Region region = Agent.CurrentPlayer.Region;
                if (region != null)
                {
                    OnRegionDataChanged(region);
                    OnHeightsDecoded(region, region.Land);
                }
            }

            EventManager.Instance.OnRegionDataChanged += OnRegionDataChanged;
            EventManager.Instance.OnHeightsDecoded += OnHeightsDecoded;
            EventManager.Instance.OnLogout += OnLogout;
        }

        private void OnDisable()
        {
            EventManager.Instance.OnRegionDataChanged -= OnRegionDataChanged;
            EventManager.Instance.OnHeightsDecoded -= OnHeightsDecoded;
            EventManager.Instance.OnLogout -= OnLogout;
        }

        protected void OnRegionDataChanged(Region region)
        {
            RegionGo rgo;
            if (RegionGoByRegion.ContainsKey(region) == false)
            {
                GameObject go = Instantiate(RegionPrefab, transform);

                // TODO: Set position

                rgo = new RegionGo()
                {
                    Root = go,
                    Terrain = go.GetComponentInChildren<Terrain>(),
                    Water = go.GetComponentInChildren<Water>()
                };
                RegionGoByRegion[region] = rgo;
            }

            rgo = RegionGoByRegion[region];

            Vector3 pos = rgo.Water.transform.position;
            pos.y = region.WaterHeight;
            rgo.Water.transform.position = pos;
        }

        protected void OnHeightsDecoded (Region region, Surface surface)
        {
            if (RegionGoByRegion.ContainsKey(region) == false)
            {
                return;
            }

            float[,] newHeights = new float[surface.GridsPerEdge - 1, surface.GridsPerEdge - 1];
            for (int y = 0; y < surface.GridsPerEdge - 1; y++)
            {
                for (int x = 0; x < surface.GridsPerEdge - 1; x++)
                {
                    newHeights[y, x] = Mathf.InverseLerp(surface.MinZ, surface.MaxZ, surface.SurfaceZ[y * surface.GridsPerEdge + x]);
                }
            }

            RegionGo rgo = RegionGoByRegion[region];
            Terrain terrain = rgo.Terrain;

            Vector3 pos = terrain.transform.position;
            terrain.transform.position = new Vector3(pos.x, surface.MinZ + 0.5f, pos.z); // TODO: What is the correct y-value here?

            TerrainData data = new TerrainData
            {
                heightmapResolution = (int)surface.GridsPerEdge,
                size = new Vector3(256f, surface.MaxZ - surface.MinZ, 256f)
            };

            data.SetDetailResolution ((int)256, 16);
            data.SetHeights          (0, 0, newHeights);
            data.SyncHeightmap();
            terrain.terrainData = data;
        }

        private void OnLogout()
        {
            foreach (RegionGo regionGo in RegionGoByRegion.Values)
            {
                Destroy(regionGo.Root);
            }
            RegionGoByRegion.Clear();
        }
    }
}
