public struct Vector3Double
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
}
