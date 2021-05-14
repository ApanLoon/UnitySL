using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using Assets.Scripts.Extensions;
using UnityEngine;

namespace Assets.Scripts.MonoBehaviours.DebugScripts
{
    public class MessageDebug : MonoBehaviour
    {
        [Flags]
        public enum Source
        {
            Original = 1,
            Uncompressed = 2
        }

        [Flags]
        public enum Output
        {
            HexDump = 1, 
            CSharp = 2,
            String = 4
        }

        public static Dictionary<MessageId, Setting> Messages = new Dictionary<MessageId, Setting>();

        [Serializable]
        public class Setting
        {
            [UIntEnum(typeof(MessageId))] public MessageId MessageId;
            public Source Source;
            public Output Output;
        }

        [SerializeField] protected Setting[] Settings = // TODO: Due to a bug in Unity enum serialisation, I have to define MessageIds >= 0x80000000 here.
        {
            new Setting {MessageId = MessageId.AgentDataUpdate,            Output = Output.String, Source = Source.Uncompressed},
            new Setting {MessageId = MessageId.AvatarAppearance,           Output = Output.String, Source = Source.Uncompressed},
            new Setting {MessageId = MessageId.OfflineNotification,        Output = Output.String, Source = Source.Uncompressed},
            new Setting {MessageId = MessageId.OnlineNotification,         Output = Output.String, Source = Source.Uncompressed},
            new Setting {MessageId = MessageId.LogoutReply,                Output = Output.String, Source = Source.Uncompressed},
            new Setting {MessageId = MessageId.ParcelOverlay,              Output = Output.String, Source = Source.Uncompressed},
            new Setting {MessageId = MessageId.SimulatorViewerTimeMessage, Output = Output.String, Source = Source.Uncompressed}
        };

        private void Awake()
        {
            Messages.Clear();
            foreach (Setting setting in Settings)
            {
                Messages.Add(setting.MessageId, setting);
            }
        }
    }
}
