
using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.Extensions.MathExtensions;
using Assets.Scripts.Primitives;
using UnityEngine;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Objects
{
    public class ImprovedTerseObjectUpdateMessage : Message
    {
        public RegionHandle RegionHandle { get; set; }
        public UInt16 TimeDilation { get; set; }

        public List<ObjectData> Objects { get; set; } = new List<ObjectData>();
        public class ObjectData
        {
            public UInt32 LocalId { get; set; }
            public byte State { get; set; } // TODO: Create enum
            public ObjectUpdateMessage.MovementUpdate MovementUpdate { get; set; }
            public TextureEntry TextureEntry { get; set; }
        }

        public ImprovedTerseObjectUpdateMessage()
        {
            MessageId = MessageId.ImprovedTerseObjectUpdate;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            RegionHandle = new RegionHandle(BinarySerializer.DeSerializeUInt64_Le(buf, ref o, length));
            TimeDilation = BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length);

            // TODO: Get the region for this handle

            float size = 256; // TODO: This should be fetched from the Region.WidthInMetres of the region this message is for.

            int nObjects = buf[o++];
            for (int i = 0; i < nObjects; i++)
            {
                ObjectData data = new ObjectData();
                Objects.Add(data);
                int len = buf[o++]; // TODO: This length is NOT correct in the messages! We ignore it for now.

                data.LocalId = BinarySerializer.DeSerializeUInt32_Le(buf, ref o, length);
                data.State = buf[o++];

                bool isAvatar = BinarySerializer.DeSerializeBool(buf, ref o, length);

                ObjectUpdateMessage.MovementUpdate update = new ObjectUpdateMessage.MovementUpdate();
                if (isAvatar)
                {
                    update.FootPlane = BinarySerializer.DeSerializeVector4(buf, ref o, length);
                }

                update.Position = BinarySerializer.DeSerializeVector3(buf, ref o, length);
                update.Velocity = new Vector3(
                    x: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size),
                    z: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size), // Handedness
                    y: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size));
                update.Acceleration = new Vector3(
                    x: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size),
                    z: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size), // Handedness
                    y: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size));
                update.Rotation = new Quaternion(
                    x: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-1f, 1f),
                    z: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-1f, 1f), // Handedness
                    y: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-1f, 1f),
                    w: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-1f, 1f));
                update.AngularVelocity = new Vector3(
                    x: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size),
                    z: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size), // Handedness
                    y: BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length).ToFloat(-size, size));

                data.MovementUpdate = update;

                len = BinarySerializer.DeSerializeInt16_Le(buf, ref o, length);
                if (len != 0)
                {
                    data.TextureEntry = BinarySerializer.DeSerializeTextureEntry(buf, ref o, length, true);
                }
            }
        }
        #endregion DeSerialise

        public override string ToString()
        {
            string s = $"{base.ToString()}: UpdateType={ObjectUpdateType.OUT_TERSE_IMPROVED}, RegionHandle={RegionHandle}, TimeDilation={TimeDilation}";
            foreach (ObjectData data in Objects)
            {
                s += $"\n                     ObjectId={data.LocalId}, State={data.State}"
                     + $"\n                     MovementUpdate={data.MovementUpdate}"
                     + $"\n                     TextureEntry({data.TextureEntry})"
                    ;
            }
            return s;
        }

    }
}
