using System;
using NitroxModel.Networking.Session;

namespace NitroxModel.Networking.Packets
{
    [Serializable]
    public record SessionReservationRequest : CorrelatedPacket    {
        public PlayerSettings PlayerSettings { get; }
        public AuthenticationContext AuthenticationContext { get; }

        public SessionReservationRequest(string correlationId, PlayerSettings playerSettings, AuthenticationContext authenticationContext) : base(correlationId)
        {
            PlayerSettings = playerSettings;
            AuthenticationContext = authenticationContext;
        }
    }
}
