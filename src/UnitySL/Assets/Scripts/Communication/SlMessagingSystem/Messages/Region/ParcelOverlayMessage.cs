
using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.Regions.Parcels;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Region
{
    public class ParcelOverlayMessage : Message
    {
        /// <summary>
        /// Identifier in the sequence of ParcelOverlay messages. (0..3)
        /// </summary>
        public Int32 SequenceId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OwnershipFlags[] Data { get; set; }

        public ParcelOverlayMessage()
        {
            MessageId = MessageId.ParcelOverlay;
            Flags = 0;
        }

        #region DeSerialise

        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            SequenceId = BinarySerializer.DeSerializeInt32_Le(buf, ref o, length);
            UInt16 len = BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length);
            Data = new OwnershipFlags[len];
            Array.Copy(buf, o, Data, 0, len);

            Logger.LogDebug("ParcelOverlayMessage.DeSerialise", ToString());
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: SequenceId={SequenceId}, Data({Data.Length})";
        }
    }
}