using System;
using System.Collections.Generic;

public class RegionInfo4
{
    public UInt64 RegionFlagsExtended { get; set; }
    public UInt64 RegionProtocols { get; set; }
}

public class RegionHandshakeMessage : Message
{
    public RegionFlags RegionFlags { get; set; }
    public SimAccess SimAccess { get; set; }
    public string SimName { get; set; }
    public Guid SimOwner { get; set; }
    public bool IsEstateManager { get; set; }
    public float WaterHeight { get; set; }
    public float BillableFactor { get; set; }
    public Guid CacheId { get; set; }
    public Guid TerrainBase0{ get; set; }
    public Guid TerrainBase1 { get; set; }
    public Guid TerrainBase2 { get; set; }
    public Guid TerrainBase3 { get; set; }
    public Guid TerrainDetail0 { get; set; }
    public Guid TerrainDetail1 { get; set; }
    public Guid TerrainDetail2 { get; set; }
    public Guid TerrainDetail3 { get; set; }
    public float TerrainStartHeight00 { get; set; }
    public float TerrainStartHeight01 { get; set; }
    public float TerrainStartHeight10 { get; set; }
    public float TerrainStartHeight11 { get; set; }
    public float TerrainHeightRange00 { get; set; }
    public float TerrainHeightRange01 { get; set; }
    public float TerrainHeightRange10 { get; set; }
    public float TerrainHeightRange11 { get; set; }

    public Guid RegionId { get; set; }

    public int CpuClassId { get; set; } // TODO: Make an enum
    public int CpuRatio { get; set; }
    public string ColoName { get; set; }
    public string ProductSku { get; set; }
    public string ProductName { get; set; }

    public List<RegionInfo4> RegionInfo4 { get; protected set; } = new List<RegionInfo4>();

    /// <summary>
    /// Use this when de-serializing.
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="sequenceNumber"></param>
    /// <param name="extraHeader"></param>
    /// <param name="frequency"></param>
    /// <param name="id"></param>
    public RegionHandshakeMessage(PacketFlags flags, UInt32 sequenceNumber, byte[] extraHeader, MessageFrequency frequency, MessageId id)
    {
        Flags = flags;
        SequenceNumber = sequenceNumber;
        ExtraHeader = extraHeader;
        Frequency = frequency;
        Id = id;
    }

    public override string ToString()
    {
        return $"{base.ToString()}: SimName={SimName}";
    }

}
