
using System;
using System.Collections.Generic;

public enum LayerType : byte
{
    Land = 0x4c, // 'L'
    Wind = 0x37, // '7'
    Cloud = 0x38  // '8'
}

public class VolumeLayerManager
{
    public static int LandByteCount = 0;
    public static int WindByteCount = 0;
    public static int CloudByteCount = 0;

    public static List<VolumeLayerData> LayerData = new List<VolumeLayerData>();

    public static void AddLayerData(VolumeLayerData vlData)
    {
        //Logger.LogDebug("VolumeLayerManager.AddLayerData");

        switch (vlData.LayerType)
        {
            case LayerType.Land:
                LandByteCount += vlData.Size;
                break;
            case LayerType.Wind:
                WindByteCount += vlData.Size;
                break;
            case LayerType.Cloud:
                CloudByteCount += vlData.Size;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        LayerData.Add(vlData);
    }


    public static void UnpackLayerData()
    {
        Logger.LogDebug("VolumeLayerManager.UnpackLayerData");
        for (int i = 0; i < LayerData.Count; i++)
        {
            VolumeLayerData vlData = LayerData[i];
            Logger.LogDebug(BitConverter.ToString(vlData.Data));
            BitPack bitPack = new BitPack(vlData.Data);

            PatchDct.PatchGroupHeader gh = PatchDct.PatchGroupHeader.Create(bitPack);

            Logger.LogDebug($"VolumeLayerManager.UnpackLayerData: {gh}");

            switch (vlData.LayerType)
            {
                case LayerType.Land:
                    //Region.CurrentRegion.Surface.DecompressDCTPatch(bitPack, gh, false);
                    break;
                case LayerType.Wind:
                    break;
                case LayerType.Cloud:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
