
using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Objects
{
    public class ImprovedTerseObjectUpdateMessage : ObjectUpdateMessage
    {
        public ImprovedTerseObjectUpdateMessage()
        {
            MessageId = MessageId.ImprovedTerseObjectUpdate;
            Flags = 0;

            UpdateType = ObjectUpdateType.OUT_TERSE_IMPROVED;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            RegionHandle = new RegionHandle(BinarySerializer.DeSerializeUInt64_Le(buf, ref o, length));
            TimeDilation = BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length);

            int nObjects = buf[o++];
            for (int i = 0; i < nObjects; i++)
            {
                ObjectUpdateMessage.ObjectData data = new ObjectUpdateMessage.ObjectData();
                Objects.Add(data);

                data.LocalId = BinarySerializer.DeSerializeUInt32_Le(buf, ref o, length);
                data.State = buf[o++];

                bool isAvatar = BinarySerializer.DeSerializeBool(buf, ref o, length);
                var len = 32;
                if (isAvatar)
                {
                    len += 16;
                }

                data.MovementUpdate = DeSerializeMovementUpdate(buf, ref o, length, len);

                data.TextureEntry = BinarySerializer.DeSerializeTextureEntry(buf, ref o, length);
            }

            #endregion DeSerialise

        }
    }
}
