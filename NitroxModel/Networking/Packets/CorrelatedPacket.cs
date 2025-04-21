using System;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public abstract class CorrelatedPacket : Packet
    {
        public string CorrelationId { get; protected set; }

        protected CorrelatedPacket(string correlationId)
        {
            CorrelationId = correlationId;
        }
    }
}
