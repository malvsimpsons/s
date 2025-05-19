using System;
using NitroxModel.Networking.Session;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record SessionReservation : Packet
{
    public SessionReservation(SessionReservationState reservationState)
    {
        ReservationState = reservationState;
    }

    public SessionReservation(string reservationKey, SessionReservationState reservationState = SessionReservationState.RESERVED) : this(reservationState)
    {
        ReservationKey = reservationKey;
    }

    public string ReservationKey { get; }
    public SessionReservationState ReservationState { get; }
}
