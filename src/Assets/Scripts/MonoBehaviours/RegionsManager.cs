using UnityEngine;

public class RegionsManager : MonoBehaviour
{
    [SerializeField] protected GameObject CurrentRegion;

    protected class RegionGo
    {
        public Terrain Terrain;
        public Water Water;
    }

    protected RegionGo CurrentRegionGo;

    private void Start()
    {
        CurrentRegionGo = new RegionGo()
        {
            Terrain = GetComponentInChildren<Terrain>(),
            Water = GetComponentInChildren<Water>()
        };

        EventManager.Instance.OnRegionDataChanged += OnRegionDataChanged;
    }

    protected void OnRegionDataChanged(Region region)
    {
        if (region == Region.CurrentRegion)
        {
            Vector3 pos = CurrentRegionGo.Water.transform.position;
            pos.y = region.WaterHeight;
            CurrentRegionGo.Water.transform.position = pos;
        }
    }
}
