using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.Primitives;
using UnityEngine;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Agent
{
    public class AppearanceData
    {
        public byte AppearanceVersion { get; set; }
        public Int32 CofVersion { get; set; }
        public UInt32 Flags { get; set; }
    }

    public class AvatarAppearanceMessage : Message
    {
        public Guid SenderId { get; set; }
        public bool SenderIsTrial { get; set; }
        public TextureEntry TextureEntry { get; set; }
        public byte[] VisualParam { get; set; }
        public List<AppearanceData> AppearanceDatas { get; set; } = new List<AppearanceData>();
        public List<Vector3> HoverHeights { get; set; } = new List<Vector3>();
        
        public AvatarAppearanceMessage()
        {
            MessageId = MessageId.AvatarAppearance;
            Flags = 0;
        }

        #region DeSerialise

        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            SenderId      = BinarySerializer.DeSerializeGuid(buf, ref o, length);
            SenderIsTrial = BinarySerializer.DeSerializeBool(buf, ref o, length);

            UInt16 len = BinarySerializer.DeSerializeUInt16_Le(buf, ref o, length);
            TextureEntry = BinarySerializer.DeSerializeTextureEntry(buf, ref o, len);
            //byte[] data = new byte[len];
            //Array.Copy(buf, o, data, 0, len);
            //o += len;


            len = buf[o++];
            // TODO: Decode visual params:
            VisualParam = new byte[len];
            Array.Copy(buf, o, VisualParam, 0, len);
            o += len;

            len = buf[o++];
            for (int i = 0; i < len; i++)
            {
                AppearanceDatas.Add(new AppearanceData()
                {
                    AppearanceVersion = buf[o++],
                    CofVersion = BinarySerializer.DeSerializeInt32_Le(buf, ref o, length),
                    Flags = BinarySerializer.DeSerializeUInt32_Le(buf, ref o, length)
                });
            }

            len = buf[o++];
            for (int i = 0; i < len; i++)
            {
                HoverHeights.Add(BinarySerializer.DeSerializeVector3(buf, ref o, length));
            }

            Logger.LogDebug(ToString());
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: SenderId={SenderId}, IsTrial={SenderIsTrial}, TextureEntry={TextureEntry}, VisualParam({VisualParam.Length}), AppearanceDatas({AppearanceDatas.Count}), HoverHeights({HoverHeights.Count})";
        }
    }
}