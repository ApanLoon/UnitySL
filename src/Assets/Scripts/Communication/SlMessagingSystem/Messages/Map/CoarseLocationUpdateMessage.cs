using System;
using System.Collections.Generic;

public class CoarseLocation
{
    public Guid AgentId { get; set; }
    public Vector3Byte Position { get; set; }
    public bool IsYou { get; set; }

    /// <summary>
    /// True if you are tracking this agent
    /// </summary>
    public bool IsPrey { get; set; }
}

public class CoarseLocationUpdateMessage : Message
{
    public List<CoarseLocation> Locations = new List<CoarseLocation>();

    public CoarseLocationUpdateMessage()
    {
        Id = MessageId.CoarseLocationUpdate;
        Flags = 0;
        Frequency = MessageFrequency.Medium;
    }

    #region DeSerialise
    protected override void DeSerialise(byte[] buf, ref int o, int length)
    {
        byte nLocations = buf[o++];
        for (byte i = 0; i < nLocations; i++)
        {
            Locations.Add (new CoarseLocation
            {
                Position = BinarySerializer.DeSerializeVector3Byte (buf, ref o, buf.Length)
            });
        }
        int youIndex     = BinarySerializer.DeSerializeInt16_Le    (buf, ref o, buf.Length);
        int preyIndex    = BinarySerializer.DeSerializeInt16_Le    (buf, ref o, buf.Length);

        byte nAgents = buf[o++];
        for (byte i = 0; i < nAgents; i++)
        {
            Guid guid    = BinarySerializer.DeSerializeGuid        (buf, ref o, length);
            if (i < nLocations)
            {
                Locations[i].AgentId = guid;
            }
        }

        if (youIndex > 0 && youIndex < nLocations - 1)
        {
            Locations[youIndex].IsYou = true;
        }

        if (preyIndex > 0 && preyIndex < nLocations - 1)
        {
            Locations[preyIndex].IsPrey = true;
        }
    }
    #endregion DeSerialise

    public override string ToString()
    {
        string s = $"{base.ToString()}:";
        foreach (CoarseLocation location in Locations)
        {
            s += $"\n    AgentId={location.AgentId}, Positon={location.Position}, IsYou={location.IsYou}, IsPrey={location.IsPrey}";
        }
        return s;
    }
}
