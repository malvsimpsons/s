using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class PlayerSeeOutOfCellEntity : Packet
{
    public NitroxId EntityId { get; set; }

    public PlayerSeeOutOfCellEntity(NitroxId entityId)
    {
        EntityId = entityId;
    }
}
