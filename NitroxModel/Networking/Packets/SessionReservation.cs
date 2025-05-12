using System;
using NitroxModel.Networking.Session;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record SessionReservation : CorrelatedPacket    {
        public ushort PlayerId { get; }
        public string ReservationKey { get; }
        public SessionReservationState ReservationState { get; }

        public SessionReservation(string correlationId, SessionReservationState reservationState) : base(correlationId)
        {
            ReservationState = reservationState;
        }
        
        public SessionReservation(string correlationId, ushort playerId, string reservationKey, 
                                             SessionReservationState reservationState = SessionReservationState.RESERVED) : this(correlationId, reservationState)
        {
            PlayerId = playerId;
            ReservationKey = reservationKey;
        }
    }
}
