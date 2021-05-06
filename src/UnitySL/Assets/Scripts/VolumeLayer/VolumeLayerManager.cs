
using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts;
using Assets.Scripts.Regions;

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
        //Logger.LogDebug("VolumeLayerManager.AddLayerData", "");

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

    public static void Clear()
    {
        LayerData.Clear();
    }

    public static string BytesToC(byte[] data, string name = "buffer", string type = "U8")
    {
        string s = $"    {type} {name}[{data.Length}] =\n    {{\n";
        for (int i = 0; i < data.Length; i++)
        {
            if (i % 16 == 0)
            {
                if (i > 0)
                {
                    s += "\n";
                }

                s += "        ";
            }
            else if (i % 8 == 0)
            {
                s += " ";
            }

            s += $"0x{data[i]:x2}, ";
        }

        s += "\n    };\n";
        return s;
    }

    public static void UnpackLayerData()
    {
        Logger.LogDebug("VolumeLayerManager.UnpackLayerData", "");
        for (int i = 0; i < LayerData.Count; i++)
        {
            VolumeLayerData vlData = LayerData[i];
            BitPack bitPack = new BitPack(vlData.Data);
            GroupHeader gh = new GroupHeader (bitPack);

            switch (vlData.LayerType)
            {
                case LayerType.Land:
                    Agent.CurrentPlayer.Region.Land.DecompressPatches (bitPack, gh, false);
                    break;
                case LayerType.Wind:
                    break;
                case LayerType.Cloud:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        //TODO: Bogus event for debug purpose (generate a texture):
        Region region = Agent.CurrentPlayer.Region;
        Surface surface = region.Land;
        surface.IdleUpdate(0f);
        EventManager.Instance.RaiseOnHeightsDecoded(region, surface.SurfaceZ, surface.GridsPerEdge, surface.MinZ, surface.MaxZ);

    }
}
