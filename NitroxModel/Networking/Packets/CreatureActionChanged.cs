using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public sealed class CreatureActionChanged : Packet
{
    public NitroxId CreatureId { get; }
    public string CreatureActionType { get; }

    public CreatureActionChanged(NitroxId creatureId, string creatureActionType)
    {
        CreatureId = creatureId;
        CreatureActionType = creatureActionType;
    }
}
