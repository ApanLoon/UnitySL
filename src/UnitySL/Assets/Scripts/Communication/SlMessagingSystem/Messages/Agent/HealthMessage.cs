using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Agent
{
    public class HealthMessage : Message
    {
        public float Health { get; set; }

        public HealthMessage()
        {
            MessageId = MessageId.HealthMessage;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            Health = BinarySerializer.DeSerializeFloat_Le (buf, ref o, length);
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: Health={Health}";
        }
    }
}
