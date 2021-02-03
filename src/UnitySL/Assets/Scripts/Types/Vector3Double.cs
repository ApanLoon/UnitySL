public struct Vector3Double
{
    public double x { get; set; }
    public double y { get; set; }
    public double z { get; set; }

    public Vector3Double(double x, double y, double z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }

    public Vector3Double Subtract (Vector3Double value)
    {
        return new Vector3Double(x - value.x, y - value.y, z - value.z);
    }
}
