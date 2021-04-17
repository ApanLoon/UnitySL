using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
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
        public List<TextureEntry> TextureEntries { get; set; } = new List<TextureEntry>();
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
            // TODO: Decode texture entries:
            byte[] data = new byte[len];
            Array.Copy(buf, o, data, 0, len);
            o += len;
            int x = 0;

            string logMessage = "**** image_id:\n";
            BinarySerializer.DeSerializeTextureEntryField<Guid>(data, ref x, len, BinarySerializer.DeSerializeGuid,
                               value => logMessage += $"Default image_id: curl http://asset-cdn.glb.agni.lindenlab.com/?texture_id={value} --output {value}.j2k\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} curl http://asset-cdn.glb.agni.lindenlab.com/?texture_id={value} --output {value}.j2k\n"));

            logMessage += "**** color:\n";
            BinarySerializer.DeSerializeTextureEntryField<Color>(data, ref x, len, BinarySerializer.DeSerializeColor,
                value => logMessage += $"Default color: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            logMessage += "**** scale_s:\n";
            BinarySerializer.DeSerializeTextureEntryField<float>(data, ref x, len, BinarySerializer.DeSerializeFloat_Le,
                value => logMessage += $"Default scale_s: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            logMessage += "**** scale_t:\n";
            BinarySerializer.DeSerializeTextureEntryField<float>(data, ref x, len, BinarySerializer.DeSerializeFloat_Le,
                value => logMessage += $"Default scale_t: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            logMessage += "**** offset_s:\n";
            BinarySerializer.DeSerializeTextureEntryField<Int16>(data, ref x, len, BinarySerializer.DeSerializeInt16_Le,
                value => logMessage += $"Default offset_s: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            logMessage += "**** offset_t:\n";
            BinarySerializer.DeSerializeTextureEntryField<Int16>(data, ref x, len, BinarySerializer.DeSerializeInt16_Le,
                value => logMessage += $"Default offset_t: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            logMessage += "**** image_rot:\n";
            BinarySerializer.DeSerializeTextureEntryField<Int16>(data, ref x, len, BinarySerializer.DeSerializeInt16_Le,
                value => logMessage += $"Default image_rot: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            logMessage += "**** bump:\n";
            BinarySerializer.DeSerializeTextureEntryField<byte>(data, ref x, len, BinarySerializer.DeSerializeUInt8,
                value => logMessage += $"Default bump: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            logMessage += "**** media_flags:\n";
            BinarySerializer.DeSerializeTextureEntryField<byte>(data, ref x, len, BinarySerializer.DeSerializeUInt8,
                value => logMessage += $"Default media_flags: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            logMessage += "**** glow:\n";
            BinarySerializer.DeSerializeTextureEntryField<byte>(data, ref x, len, BinarySerializer.DeSerializeUInt8,
                value => logMessage += $"Default glow: {value}\n",
                ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            if (x < len)
            {
                logMessage += "**** material_id:\n";
                BinarySerializer.DeSerializeTextureEntryField<Guid>(data, ref x, len, BinarySerializer.DeSerializeGuid,
                    value => logMessage += $"Default material_id: {value}\n",
                    ((mask, value) => logMessage += $"Exception: 0x{mask:x} {value}\n"));

            }

            //logMessage += "**** Static:\n";

            //x = 0;
            //Guid textureId = BinarySerializer.DeSerializeGuid(data, ref x, len);
            //logMessage += $"Default image_id: http://asset-cdn.glb.agni.lindenlab.com/?texture_id={textureId}\n";

            //while (x < len)
            //{
            //    UInt64 mask = BinarySerializer.DeSerializeUInt64v_Be(data, ref x, len);
            //    if (mask == 0)
            //    {
            //        break;
            //    }
            //    textureId = BinarySerializer.DeSerializeGuid(data, ref x, len);
            //    logMessage += $"Exception: 0x{mask:x} http://asset-cdn.glb.agni.lindenlab.com/?texture_id={textureId}\n";
            //}

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

            Logger.LogDebug(ToString() + "\n" + logMessage);
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: SenderId={SenderId}, IsTrial={SenderIsTrial}, TextureEntries({TextureEntries.Count}), VisualParam({VisualParam.Length}), AppearanceDatas({AppearanceDatas.Count}), HoverHeights({HoverHeights.Count})";
        }
    }
}