using System;

public class RegionHandle
{
    public static UInt64 Create(UInt32 x, UInt32 y)
    {
        return ((UInt64)x << 32) | y;
    }
}
