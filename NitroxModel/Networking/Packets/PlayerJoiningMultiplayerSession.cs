using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PlayerJoiningMultiplayerSession : Packet
{
    public PlayerJoiningMultiplayerSession(string reservationKey)
    {
        ReservationKey = reservationKey;
    }

    public string ReservationKey { get; }
}
