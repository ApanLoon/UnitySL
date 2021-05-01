namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem
{
    public class CompletePingCheckMessage : Message
    {
        public byte PingId { get; set; }

        public CompletePingCheckMessage()
        {
            MessageId = MessageId.CompletePingCheck;
            Flags = 0;
        }

        public CompletePingCheckMessage(byte pingId) : this()
        {
            PingId = pingId;
        }

        #region Serialise
        public override int GetSerializedLength()
        {
            return base.GetSerializedLength()
                   + 1;  // PingId
        }
        public override int Serialize(byte[] buffer, int offset, int length)
        {
            int o = offset;
            o += base.Serialize(buffer, offset, length);

            buffer[o++] = PingId;

            return o - offset;
        }
        #endregion Serialise

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            PingId = buf[o++];
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: PingId={PingId}";
        }
    }
}
