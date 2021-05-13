using System;
using Assets.Scripts.Communication.SlMessagingSystem.Messages.MessageSystem;
using UnityEngine;

namespace Assets.Scripts.Communication.SlMessagingSystem.Messages.Region
{
    public class SimulatorViewerTimeMessage : Message
    {
        /// <summary>
        /// Micro seconds since start
        /// </summary>
        public UInt64 UsecSinceStart { get; set; }
        public UInt32 SecPerDay { get; set; }
        public UInt32 SecPerYear { get; set; }
        public Vector3 SunDirection { get; set; }
        public float SunPhase { get; set; }
        public Vector3 SunAngVelocity { get; set; }

        public SimulatorViewerTimeMessage()
        {
            MessageId = MessageId.SimulatorViewerTimeMessage;
            Flags = 0;
        }

        #region DeSerialise
        protected override void DeSerialise(byte[] buf, ref int o, int length)
        {
            UsecSinceStart = BinarySerializer.DeSerializeUInt64_Le (buf, ref o, length);
            SecPerDay      = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
            SecPerYear     = BinarySerializer.DeSerializeUInt32_Le (buf, ref o, length);
            SunDirection   = BinarySerializer.DeSerializeVector3   (buf, ref o, buf.Length);
            SunPhase       = BinarySerializer.DeSerializeFloat_Le  (buf, ref o, length);
            SunAngVelocity = BinarySerializer.DeSerializeVector3   (buf, ref o, buf.Length);
        }
        #endregion DeSerialise

        public override string ToString()
        {
            return $"{base.ToString()}: UsecSinceStart={UsecSinceStart}, SecPerDay={SecPerDay}, SecPerYear={SecPerYear}, SunDirection={SunDirection}, SunPhase={SunPhase}, SunAngVelocity={SunAngVelocity}";
        }
    }
}
