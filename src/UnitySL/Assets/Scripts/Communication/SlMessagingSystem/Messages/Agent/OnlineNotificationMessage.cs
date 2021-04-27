using System;
using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Agent
{
    public class OnlineNotificationMessage : Message
    {
        public List<Guid> Agents { get; set; } = new List<Guid>();

        public OnlineNotificationMessage()
        {
            MessageId = MessageId.OnlineNotification;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            byte nAgents = buf[o++];
            for (byte i = 0; i < nAgents; i++)
            {
                Agents.Add (BinarySerializer.DeSerializeGuid (buf, ref o, length));
                Logger.LogDebug("OnlineNotificationMessage", ToString());
            }
        }
        #endregion DeSerialise

        public override string ToString()
        {
            string s = $"{base.ToString()}:";
            foreach (Guid agent in Agents)
            {
                s += $"\n    AgentId={agent}";
            }
            return s;
        }
    }
}
