using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public class PlayerTeleported(PeerId playerId, NitroxVector3 destinationFrom, NitroxVector3 destinationTo, Optional<NitroxId> subRootID)
        : Packet
    {
        public PeerId PlayerId { get; } = playerId;
        public NitroxVector3 DestinationFrom { get; } = destinationFrom;
        public NitroxVector3 DestinationTo { get; } = destinationTo;
        public Optional<NitroxId> SubRootID { get; } = subRootID;
    }
}
