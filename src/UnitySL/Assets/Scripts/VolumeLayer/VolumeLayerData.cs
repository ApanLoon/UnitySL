using Assets.Scripts.Regions;

public class VolumeLayerData
{
    public LayerType LayerType { get; set; }
    public int Size { get; set; }
    public byte[] Data { get; set; }
    public Region Region { get; set; }
}
