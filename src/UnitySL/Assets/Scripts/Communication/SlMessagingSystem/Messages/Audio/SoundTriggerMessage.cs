using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using UnityEngine;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Audio
{
    public class SoundTriggerMessage : Message
    {
        public Guid SoundId { get; set; }
        public Guid OwnerId { get; set; }
        public Guid ObjectId { get; set; }
        public Guid ParentId { get; set; }
        public RegionHandle Handle { get; set; }
        public Vector3 Position { get; set; }
        public float Gain { get; set; }

        public SoundTriggerMessage()
        {
            MessageId = MessageId.SoundTrigger;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            SoundId  = BinarySerializer.DeSerializeGuid    (buf, ref o, length);
            OwnerId  = BinarySerializer.DeSerializeGuid    (buf, ref o, length);
            ObjectId = BinarySerializer.DeSerializeGuid    (buf, ref o, length);
            ParentId = BinarySerializer.DeSerializeGuid    (buf, ref o, length);
            Handle   = new RegionHandle(BinarySerializer.DeSerializeUInt64_Le(buf, ref o, length));
            Position = BinarySerializer.DeSerializeVector3 (buf, ref o, buf.Length);
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: SoundId={SoundId}, OwnerId={OwnerId}, ObjectId={ObjectId}, ParentId={ParentId}, RegionHandle={Handle}, Position={Position}, Gain={Gain}";
        }
    }
}
