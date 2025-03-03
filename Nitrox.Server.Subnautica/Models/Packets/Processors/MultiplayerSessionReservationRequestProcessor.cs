using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxServer.Communication;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class MultiplayerSessionReservationRequestProcessor(PlayerService playerManager, ILogger<MultiplayerSessionReservationRequestProcessor> logger) : UnauthenticatedPacketProcessor<MultiplayerSessionReservationRequest>
{
    private readonly PlayerService playerManager = playerManager;

    public override void Process(MultiplayerSessionReservationRequest packet, INitroxConnection connection)
    {
        logger.LogInformation("Processing reservation request from {Username}", packet.AuthenticationContext.Username);

        string correlationId = packet.CorrelationId;
        PlayerSettings playerSettings = packet.PlayerSettings;
        AuthenticationContext authenticationContext = packet.AuthenticationContext;
        MultiplayerSessionReservation reservation = playerManager.ReservePlayerContext(
            connection,
            playerSettings,
            authenticationContext,
            correlationId);

        logger.LogInformation("Reservation processed successfully for {Username} - {Reservation}", packet.AuthenticationContext.Username, reservation);

        connection.SendPacket(reservation);
    }
}
