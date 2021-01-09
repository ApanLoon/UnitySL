public struct Vector3Byte
{
    public byte x { get; set; }
    public byte y { get; set; }
    public byte z { get; set; }

    public Vector3Byte (byte x, byte y, byte z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}
