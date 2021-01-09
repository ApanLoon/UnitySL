using System;

public class PatchDct
{
    public static readonly int NORMAL_PATCH_SIZE = 16;
    public static readonly int LARGE_PATCH_SIZE = 32;

    public static readonly byte END_OF_PATCHES = 97;

    public class PatchGroupHeader
    {
        public static PatchGroupHeader Create(BitPack bitPack)
        {
            PatchGroupHeader gh = new PatchGroupHeader
            {
                Stride = bitPack.GetUInt16_Le(),
                PatchSize = bitPack.GetUInt8(),
                LayerType = (LayerType)bitPack.GetUInt8()
            };
            return gh;
        }

        public UInt16 Stride;
        public byte PatchSize;
        public LayerType LayerType;

        public override string ToString()
        {
            return $"GroupHeader: Stride={Stride}, PatchSize={PatchSize}, LayerType={LayerType}";
        }
    }

    public class PatchHeader
    {
        public static PatchHeader Create(BitPack bitPack)
        {
            PatchHeader ph = new PatchHeader();
            ph.QuantWBits = bitPack.GetUInt8();
            if (ph.QuantWBits == END_OF_PATCHES)
            {
                // End of data, blitz the rest.
                ph.DcOffset = 0;
                ph.Range = 0;
                ph.PatchIds = 0;
                return ph;
            }

            ph.DcOffset = bitPack.GetFloat_Le();
            ph.Range = bitPack.GetUInt16_Le();
            ph.PatchIds = bitPack.GetUInt16_Le(10);
            return ph;
        }

        public float DcOffset;      // 4 bytes
        public UInt16 Range;        // 2 = 7 ((S16) FP range (breaks if we need > 32K meters in 1 patch)
        public byte QuantWBits;     // 1 = 8 (upper 4 bits is quant - 2, lower 4 bits is word bits - 2)
        public UInt16 PatchIds;     // 2 = 10 (actually only uses 10 bits, 5 for each)

        public override string ToString()
        {
            return $"PatchHeader: DcOffset={DcOffset}, Range={Range}, QuantWBits={QuantWBits}, PatchIds={PatchIds}";
        }
    }

}
