using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Audio
{
    public class PreloadSoundMessage : Message
    {
        public class SoundInfo
        {
            public Guid ObjectId { get; set; }
            public Guid OwnerId { get; set; }
            public Guid SoundId { get; set; }
        }

        public List<SoundInfo> Sounds { get; set; } = new List<SoundInfo>();

        public PreloadSoundMessage()
        {
            MessageId = MessageId.PreloadSound;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            byte nSounds = buf[o++];
            for (byte i = 0; i < nSounds; i++)
            {
                PreloadSoundMessage.SoundInfo si = new PreloadSoundMessage.SoundInfo
                {
                    ObjectId = BinarySerializer.DeSerializeGuid (buf, ref o, length),
                    OwnerId  = BinarySerializer.DeSerializeGuid (buf, ref o, length),
                    SoundId  = BinarySerializer.DeSerializeGuid (buf, ref o, length)
                };
                Sounds.Add(si);
            }
        }
        #endregion DeSerialise

        public override string ToString()
        {
            string s = $"{base.ToString()}:";
            foreach (SoundInfo sound in Sounds)
            {
                s += $"ObjectId={sound.ObjectId}, OwnerId={sound.OwnerId}, SoundId={sound.SoundId}";
            }
            return s;
        }
    }
}
