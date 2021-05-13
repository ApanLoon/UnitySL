using System.Collections.Generic;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Agent
{
    public class ControlsChange
    {
        public bool TakeControls { get; set; }
        public AgentControlFlags ControlFlags { get; set; }
        public bool PassToAgent { get; set; }
    }

    public class ScriptControlChangeMessage : Message
    {
        public List<ControlsChange> Controls { get; set; } = new List<ControlsChange>();

        public ScriptControlChangeMessage()
        {
            MessageId = MessageId.ScriptControlChange;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            byte nControls = buf[o++];
            for (byte i = 0; i < nControls; i++)
            {
                ControlsChange c = new ControlsChange();
                c.TakeControls = BinarySerializer.DeSerializeBool(buf, ref o, length);
                c.ControlFlags = (AgentControlFlags)BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
                c.PassToAgent  = BinarySerializer.DeSerializeBool(buf, ref o, length);
                Controls.Add(c);
            }

        }
        #endregion DeSerialise

        public override string ToString()
        {
            string s = $"{base.ToString()}:";
            foreach (ControlsChange control in Controls)
            {
                s += $"\n    TakeControls={control.TakeControls}, ControlFlags={control.ControlFlags}, PassToAgent={control.PassToAgent}";
            }

            return s;
        }
    }
}