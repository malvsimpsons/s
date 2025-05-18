using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Session;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Workflows;

/// <summary>
///     Handles packets related to the game join process.
/// </summary>
internal sealed class JoinWorkflow(SessionRepository sessionRepository, IOptionsMonitor<SubnauticaServerOptions> optionsProvider, ILogger<JoinWorkflow> logger) :
    IAnonPacketProcessor<SessionPolicyRequest>,
    IAnonPacketProcessor<SessionReservationRequest>
{
    private readonly SessionRepository sessionRepository = sessionRepository;
    private readonly ILogger<JoinWorkflow> logger = logger;
    private readonly IOptionsMonitor<SubnauticaServerOptions> optionsProvider = optionsProvider;

    public async Task Process(AnonProcessorContext context, SessionPolicyRequest packet)
    {
        logger.ZLogInformation($"Providing join policies to session #{context.Sender:@SessionId}...");
        SubnauticaServerOptions options = optionsProvider.CurrentValue;
        await context.ReplyToSender(new MultiplayerSessionPolicy(packet.CorrelationId, options.DisableConsole, options.MaxConnections, options.IsPasswordRequired()));
    }

    public async Task Process(AnonProcessorContext context, SessionReservationRequest packet)
    {
        // TODO: USE DATABASE
        logger.ZLogInformation($"Processing reservation request from session #{context.Sender:@SessionId}");
        AuthenticationContext authenticationContext = packet.AuthenticationContext;
        SubnauticaServerOptions options = optionsProvider.CurrentValue;

        if (await sessionRepository.GetActiveSessionCount() >= options.MaxConnections)
        {
            SessionReservationState rejectedState = SessionReservationState.REJECTED | SessionReservationState.SERVER_PLAYER_CAPACITY_REACHED;
            await context.ReplyToSender(new SessionReservation(packet.CorrelationId, rejectedState));
            return;
        }
        if (!string.IsNullOrEmpty(options.ServerPassword) && (!authenticationContext.ServerPassword.HasValue || authenticationContext.ServerPassword.Value != options.ServerPassword))
        {
            SessionReservationState rejectedState = SessionReservationState.REJECTED | SessionReservationState.AUTHENTICATION_FAILED;
            await context.ReplyToSender(new SessionReservation(packet.CorrelationId, rejectedState));
            return;
        }
        //https://regex101.com/r/eTWiEs/2/
        if (!Regex.IsMatch(authenticationContext.Username, @"^[a-zA-Z0-9._-]{3,25}$")) // TODO: Allow player names with spaces.
        {
            SessionReservationState rejectedState = SessionReservationState.REJECTED | SessionReservationState.INCORRECT_USERNAME;
            await context.ReplyToSender(new SessionReservation(packet.CorrelationId, rejectedState));
            return;
        }

        // TODO: Reject if requested player data is in use.
        // if (PlayerCurrentlyJoining)
        // {
        //     // JoinQueue.Enqueue(new KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest>(
        //     //                       connection,
        //     //                       new MultiplayerSessionReservationRequest(correlationId, playerSettings, authenticationContext)));
        //
        //     await context.ReplyToSender(new MultiplayerSessionReservation(packet.CorrelationId, MultiplayerSessionReservationState.ENQUEUED_IN_JOIN_QUEUE));
        // }

        // TODO: LOGIN TO PLAYER DATA IF AUTHENTICATED
        // string playerName = authenticationContext.Username;
        // if (player?.IsPermaDeath == true && options.IsHardcore)
        // {
        //     MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.HARDCORE_PLAYER_DEAD;
        //     return new MultiplayerSessionReservation(correlationId, rejectedState);
        // }
        //
        // if (reservedPlayerNames.Contains(playerName))
        // {
        //     MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.UNIQUE_PLAYER_NAME_CONSTRAINT_VIOLATED;
        //     return new MultiplayerSessionReservation(correlationId, rejectedState);
        // }
        //
        // assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage);
        // if (assetPackage == null)
        // {
        //     assetPackage = new ConnectionAssets();
        //     assetsByConnection.Add(connection, assetPackage);
        //     reservedPlayerNames.Add(playerName);
        // }
        //
        // bool hasSeenPlayerBefore = player != null;
        // ushort playerId = hasSeenPlayerBefore ? player.Id : ++currentPlayerId;
        // NitroxId playerNitroxId = hasSeenPlayerBefore ? player.GameObjectId : new NitroxId();
        // SubnauticaGameMode gameMode = hasSeenPlayerBefore ? player.GameMode : serverConfig.GameMode;
        // IntroCinematicMode introCinematicMode = hasSeenPlayerBefore ? IntroCinematicMode.COMPLETED : IntroCinematicMode.LOADING;
        //
        // PlayerContext playerContext = new(playerName, playerId, playerNitroxId, !hasSeenPlayerBefore, playerSettings, false, gameMode, null, introCinematicMode);
        // string reservationKey = Guid.NewGuid().ToString();
        //
        // reservations.Add(reservationKey, playerContext);
        // assetPackage.ReservationKey = reservationKey;
        //
        // PlayerCurrentlyJoining = true;
        //
        // InitialSyncTimerData timerData = new(connection, authenticationContext, serverConfig.InitialSyncTimeout);
        // initialSyncTimer = new Timer(InitialSyncTimerElapsed, timerData, 0, 200);

        SessionReservation reservation = new(packet.CorrelationId, 1, Guid.NewGuid().ToString()); // TODO: Change player id!
        logger.ZLogInformation($"Reservation processed successfully for session #{context.Sender} - {reservation}");
        await context.ReplyToSender(reservation);
    }
}
