
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
}
