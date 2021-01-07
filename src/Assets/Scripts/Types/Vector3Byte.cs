public struct Vector3Byte
{
    public byte x { get; set; }
    public byte y { get; set; }
    public byte z { get; set; }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}
