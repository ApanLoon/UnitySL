using System;

[Flags]
public enum RegionProtocols : UInt64
{
    None = 0,
    AgentAppearanceService = (1 << 0)
}
