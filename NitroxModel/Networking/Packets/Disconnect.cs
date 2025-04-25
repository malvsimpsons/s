using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class Disconnect : Packet
{
    public Disconnect(SessionId playerId)
    {
        PlayerId = playerId;
    }

    public SessionId PlayerId { get; }
}
