using System;
using NitroxModel.Networking.Session;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record SessionReservationRequest(PlayerSettings PlayerSettings, AuthenticationContext AuthenticationContext) : Packet;
