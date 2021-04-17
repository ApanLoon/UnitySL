using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Region
{
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

        public RegionHandshakeMessage()
        {
            MessageId = MessageId.RegionHandshake;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise (byte[] buf, ref int o, int length)
        {
            RegionFlags          = (RegionFlags)BinarySerializer.DeSerializeUInt32_Le(buf, ref o, length);
            SimAccess            = (SimAccess)buf[o++];
            SimName              = BinarySerializer.DeSerializeString   (buf, ref o, length, 1);
            SimOwner             = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            IsEstateManager      = BinarySerializer.DeSerializeBool     (buf, ref o, length);
            WaterHeight          = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            BillableFactor       = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            CacheId              = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainBase0         = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainBase1         = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainBase2         = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainBase3         = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainDetail0       = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainDetail1       = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainDetail2       = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainDetail3       = BinarySerializer.DeSerializeGuid     (buf, ref o, length);
            TerrainStartHeight00 = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            TerrainStartHeight01 = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            TerrainStartHeight10 = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            TerrainStartHeight11 = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            TerrainHeightRange00 = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            TerrainHeightRange01 = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            TerrainHeightRange10 = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
            TerrainHeightRange11 = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);

            RegionId             = BinarySerializer.DeSerializeGuid     (buf, ref o, length);

            CpuClassId           = BinarySerializer.DeSerializeInt32_Le (buf, ref o, length);
            CpuRatio             = BinarySerializer.DeSerializeInt32_Le (buf, ref o, length);
            ColoName             = BinarySerializer.DeSerializeString   (buf, ref o, length, 1);
            ProductSku           = BinarySerializer.DeSerializeString   (buf, ref o, length, 1);
            ProductName          = BinarySerializer.DeSerializeString   (buf, ref o, length, 1);

            int n = buf[o++];
            for (int i = 0; i < n; i++)
            {
                RegionInfo4 info = new RegionInfo4
                {
                    RegionFlagsExtended = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length),
                    RegionProtocols     = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length)
                };
                RegionInfo4.Add (info);
            }
        }
        #endregion DeSerialise
        public override string ToString()
        {
            return $"{base.ToString()}: SimName={SimName}";
        }
    }
}