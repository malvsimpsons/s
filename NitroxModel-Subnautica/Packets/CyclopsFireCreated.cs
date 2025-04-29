using System;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace NitroxModel_Subnautica.Packets
{
    /// <summary>
    /// Triggered when a fire has been created in <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>
    /// </summary>
    [Serializable]
    public record CyclopsFireCreated : Packet
    {
        public CyclopsFireData FireCreatedData { get; }

        public CyclopsFireCreated(NitroxId id, NitroxId cyclopsId, CyclopsRooms room, int nodeIndex)
        {
            FireCreatedData = new CyclopsFireData(id, cyclopsId, room, nodeIndex);
        }

        /// <remarks>Used for deserialization</remarks>
        public CyclopsFireCreated(CyclopsFireData fireCreatedData)
        {
            FireCreatedData = fireCreatedData;
        }

        public override string ToString()
        {
            return $"[CyclopsFireCreated - {FireCreatedData}]";
        }
    }
}
