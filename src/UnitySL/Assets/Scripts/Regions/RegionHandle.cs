using System;

public class RegionHandle : IEquatable<RegionHandle>
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

    public Vector3Double ToVector3Double()
    {
        return new Vector3Double (X, 0f, Y); // NOTE: y and z are swapped compared to Indra because of handedness
    }

    public override string ToString()
    {
        return $"0x{Handle:x8} ({X}, {Y})";
    }

    public bool Equals(RegionHandle other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RegionHandle) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)X * 397) ^ (int)Y;
        }
    }
}
