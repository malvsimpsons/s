using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class Disconnect : Packet
{
    public Disconnect(PeerId playerId)
    {
        PlayerId = playerId;
    }

    public PeerId PlayerId { get; }
}
