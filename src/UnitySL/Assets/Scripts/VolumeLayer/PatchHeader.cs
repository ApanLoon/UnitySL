using System;

public class PatchHeader
{
    public static readonly byte END_OF_PATCHES = 97;

    public bool IsEnd => QuantWBits == END_OF_PATCHES;

    public PatchHeader (BitPack bitPack)
    {
        QuantWBits = bitPack.GetUInt8();
        if (QuantWBits == END_OF_PATCHES)
        {
            // End of data, blitz the rest.
            DcOffset = 0;
            Range = 0;
            PatchIds = 0;
            return;
        }

        DcOffset = bitPack.GetFloat_Le();
        Range    = bitPack.GetUInt16_Le();
        PatchIds = bitPack.GetUInt16_Le(10);
    }

    public float DcOffset;      // 4 bytes
    public UInt16 Range;        // 2 = 7 ((S16) FP range (breaks if we need > 32K meters in 1 patch)
    public byte QuantWBits;     // 1 = 8 (upper 4 bits is quant - 2, lower 4 bits is word bits - 2)
    public UInt16 PatchIds;     // 2 = 10 (actually only uses 10 bits, 5 for each)

    public override string ToString()
    {
        return $"PatchHeader: QuantWBits=0x{QuantWBits:x2}, DcOffset={DcOffset}, Range=0x{Range:x4}, PatchIds=0x{PatchIds:x4}";
    }
}

