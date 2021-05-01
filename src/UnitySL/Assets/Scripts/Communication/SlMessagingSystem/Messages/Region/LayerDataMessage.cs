using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Region
{
    public class LayerDataMessage : Message
    {
        public LayerType LayerType { get; set; }
        public byte[] Data { get; set; }

        public LayerDataMessage()
        {
            MessageId = MessageId.LayerData;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            LayerType = (LayerType)buf[o++];
            UInt16 len = BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length);
            Data = new byte[len];
            Array.Copy(buf, o, Data, 0, len);
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: LayerType={LayerType}, Data({Data.Length})";
        }
    }
}
