
public enum CapabilityType
{
    Unknown,
    Http,
    MessageSystem
}

public class Capability
{
    public string Name { get; set; }
    
    public bool IsGranted { get; set; }

    public CapabilityType CapabilityType { get; set; }

    public string Url { get; set; }
    public int Throttle { get; set; }
    public bool UseSsl { get; set; }
    public bool ViaCache { get; set; }
    public bool ViewerUsesBenefits { get; set; }

    public Capability(string name)
    {
        Name = name;
        IsGranted = false;
        CapabilityType = CapabilityType.Unknown;
    }
}
