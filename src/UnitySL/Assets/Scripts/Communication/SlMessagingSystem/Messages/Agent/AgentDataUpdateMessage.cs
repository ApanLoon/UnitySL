using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Agent
{
    public class AgentDataUpdateMessage : Message
    {
        public Guid AgentId { get; set; }

        public string FirstName { get; set; }
    
        public string LastName { get; set; }

        public string GroupTitle { get; set; }

        public Guid ActiveGroupId { get; set; }

        public UInt64 GroupPowers { get; set; }

        public string GroupName { get; set; }

        public AgentDataUpdateMessage()
        {
            MessageId = MessageId.AgentDataUpdate;
            Flags = 0;
        }
    
        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            AgentId       = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
            FirstName     = BinarySerializer.DeSerializeString    (buf, ref o, length, 1);
            LastName      = BinarySerializer.DeSerializeString    (buf, ref o, length, 1);
            GroupTitle    = BinarySerializer.DeSerializeString    (buf, ref o, length, 1);
            ActiveGroupId = BinarySerializer.DeSerializeGuid      (buf, ref o, length);
            GroupPowers   = BinarySerializer.DeSerializeUInt64_Le (buf, ref o, length);
            GroupName     = BinarySerializer.DeSerializeString    (buf, ref o, length, 1);
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: AgentId = {AgentId}, FirstName = {FirstName}, LastName = {LastName}, GroupTitle = {GroupTitle}, ActiveGroupId = {ActiveGroupId}, GroupName = {GroupName}";
        }
    }
}
