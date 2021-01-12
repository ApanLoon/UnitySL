using System;

public class GroupHeader
{
    public UInt16 Stride;
    public byte PatchSize;
    public LayerType LayerType;

    public GroupHeader (BitPack bitPack)
    {
        Stride = bitPack.GetUInt16_Le();
        PatchSize = bitPack.GetUInt8();
        LayerType = (LayerType) bitPack.GetUInt8();
    }
    
    public override string ToString()
    {
        return $"GroupHeader: Stride=0x{Stride:x4}, PatchSize=0x{PatchSize:x2}, LayerType={LayerType}";
    }
}
