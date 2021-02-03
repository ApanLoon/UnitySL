using System.Collections.Generic;
using UnityEngine;

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

    private void Start()
    {
        EventManager.Instance.OnRegionDataChanged += OnRegionDataChanged;
        EventManager.Instance.OnHeightsDecoded += OnHeightsDecoded;
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

    protected void OnHeightsDecoded (Region region, float[] heights, uint width, float minHeight, float maxHeight)
    {
        if (RegionGoByRegion.ContainsKey(region) == false)
        {
            return;
        }

        float[,] newHeights = new float[width - 1, width - 1];
        for (int y = 0; y < width - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                newHeights[y, x] = Mathf.InverseLerp(minHeight, maxHeight, heights[y * width + x]);
            }
        }

        RegionGo rgo = RegionGoByRegion[region];
        Terrain terrain = rgo.Terrain;

        Vector3 pos = terrain.transform.position;
        terrain.transform.position = new Vector3(pos.x, minHeight + 0.5f, pos.z); // TODO: What is the correct y-value here?

        TerrainData data = new TerrainData
        {
            heightmapResolution = (int) width,
            size = new Vector3(256f, maxHeight - minHeight, 256f)
        };

        data.SetDetailResolution ((int)256, 16);
        data.SetHeights          (0, 0, newHeights);
        data.SyncHeightmap();
        terrain.terrainData = data;
    }
}
