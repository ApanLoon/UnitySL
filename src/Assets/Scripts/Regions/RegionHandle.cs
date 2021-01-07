using System;

public class RegionHandle
{
    public UInt32 X { get; set; }
    public UInt32 Y { get; set; }

    public UInt64 Handle => ((UInt64)X << 32) | Y;

    public RegionHandle(UInt32 x, UInt32 y)
    {
        X = x;
        Y = y;
    }

    public RegionHandle(UInt64 handle)
    {
        X = (UInt32)(handle >> 32);
        Y = (UInt32)handle;
    }

    public override string ToString()
    {
        return $"0x{Handle:x8} ({X}, {Y})";
    }
}
