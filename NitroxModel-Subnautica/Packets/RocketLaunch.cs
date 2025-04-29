using System;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxModel_Subnautica.Packets;

[Serializable]
public record RocketLaunch : Packet
{
    public NitroxId RocketId { get; }

    public RocketLaunch(NitroxId rocketId)
    {
        RocketId = rocketId;
    }
}
