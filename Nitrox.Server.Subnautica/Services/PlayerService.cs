using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Persistence;
using Nitrox.Server.Subnautica.Models.Persistence.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Server;
using NitroxServer.Communication;
using NitroxServer.GameLogic;
using NitroxPlayer = NitroxServer.Player;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class PlayerService(IOptions<Models.Configuration.SubnauticaServerOptions> serverConfig, IStateManager<PlayerData> players) : IHostedService
{
    private readonly IStateManager<PlayerData> players = players;
    private ThreadSafeDictionary<string, NitroxPlayer> allPlayersByName = [];
    private readonly ThreadSafeDictionary<ushort, NitroxPlayer> connectedPlayersById = [];
    private readonly ThreadSafeDictionary<INitroxConnection, ConnectionAssets> assetsByConnection = new();
    private readonly ThreadSafeDictionary<string, PlayerContext> reservations = new();
    private readonly ThreadSafeSet<string> reservedPlayerNames = ["Player"]; // "Player" is often used to identify the local player and should not be used by any user

    private ThreadSafeQueue<KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest>> JoinQueue { get; set; } = new();
    private bool PlayerCurrentlyJoining { get; set; }

    private Timer initialSyncTimer;

    private readonly Models.Configuration.SubnauticaServerOptions serverConfig = serverConfig.Value;
    private ushort currentPlayerId;

    public List<NitroxPlayer> GetConnectedPlayers()
    {
        return ConnectedPlayers().ToList();
    }

    public List<NitroxPlayer> GetConnectedPlayersExcept(NitroxPlayer excludePlayer)
    {
        return ConnectedPlayers().Where(player => player != excludePlayer).ToList();
    }

    public IEnumerable<NitroxPlayer> GetAllPlayers()
    {
        return allPlayersByName.Values;
    }

    public MultiplayerSessionReservation ReservePlayerContext(
        INitroxConnection connection,
        PlayerSettings playerSettings,
        AuthenticationContext authenticationContext,
        string correlationId)
    {
        if (reservedPlayerNames.Count >= serverConfig.MaxConnections)
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.SERVER_PLAYER_CAPACITY_REACHED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (!string.IsNullOrEmpty(serverConfig.ServerPassword) && (!authenticationContext.ServerPassword.HasValue || authenticationContext.ServerPassword.Value != serverConfig.ServerPassword))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.AUTHENTICATION_FAILED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        //https://regex101.com/r/eTWiEs/2/
        if (!Regex.IsMatch(authenticationContext.Username, @"^[a-zA-Z0-9._-]{3,25}$"))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.INCORRECT_USERNAME;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (PlayerCurrentlyJoining)
        {
            if (JoinQueue.Any(pair => ReferenceEquals(pair.Key, connection)))
            {
                // Don't enqueue the request if there is already another enqueued request by the same user
                return new MultiplayerSessionReservation(correlationId, MultiplayerSessionReservationState.REJECTED);
            }

            JoinQueue.Enqueue(new KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest>(
                                  connection,
                                  new MultiplayerSessionReservationRequest(correlationId, playerSettings, authenticationContext)));

            return new MultiplayerSessionReservation(correlationId, MultiplayerSessionReservationState.ENQUEUED_IN_JOIN_QUEUE);
        }

        string playerName = authenticationContext.Username;

        allPlayersByName.TryGetValue(playerName, out NitroxServer.Player player);
        if (player?.IsPermaDeath == true && serverConfig.IsHardcore)
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.HARDCORE_PLAYER_DEAD;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        if (reservedPlayerNames.Contains(playerName))
        {
            MultiplayerSessionReservationState rejectedState = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.UNIQUE_PLAYER_NAME_CONSTRAINT_VIOLATED;
            return new MultiplayerSessionReservation(correlationId, rejectedState);
        }

        assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage);
        if (assetPackage == null)
        {
            assetPackage = new ConnectionAssets();
            assetsByConnection.Add(connection, assetPackage);
            reservedPlayerNames.Add(playerName);
        }

        bool hasSeenPlayerBefore = player != null;
        ushort playerId = hasSeenPlayerBefore ? player.Id : ++currentPlayerId;
        NitroxId playerNitroxId = hasSeenPlayerBefore ? player.GameObjectId : new NitroxId();
        NitroxGameMode gameMode = hasSeenPlayerBefore ? player.GameMode : serverConfig.GameMode;
        IntroCinematicMode introCinematicMode = hasSeenPlayerBefore ? IntroCinematicMode.COMPLETED : IntroCinematicMode.LOADING;

        // TODO: At some point, store the muted state of a player
        PlayerContext playerContext = new(playerName, playerId, playerNitroxId, !hasSeenPlayerBefore, playerSettings, false, gameMode, null, introCinematicMode);
        string reservationKey = Guid.NewGuid().ToString();

        reservations.Add(reservationKey, playerContext);
        assetPackage.ReservationKey = reservationKey;

        PlayerCurrentlyJoining = true;

        InitialSyncTimerData timerData = new(connection, authenticationContext, serverConfig.InitialSyncTimeout);
        initialSyncTimer = new Timer(InitialSyncTimerElapsed, timerData, 0, 200);

        return new MultiplayerSessionReservation(correlationId, playerId, reservationKey);
    }

    private void InitialSyncTimerElapsed(object state)
    {
        if (state is InitialSyncTimerData { Disposing: false } timerData)
        {
            allPlayersByName.TryGetValue(timerData.Context.Username, out NitroxServer.Player player);

            if (timerData.Connection.State < NitroxConnectionState.Connected)
            {
                if (player == null) // player can cancel the joining process before this timer elapses
                {
                    Log.Error("Player was nulled while joining");
                    Disconnect(timerData.Connection);
                }
                else
                {
                    player.SendPacket(new PlayerKicked("An error occured while loading, Initial sync took too long to complete"));
                    Disconnect(player.Connection);
                    SendPacketToOtherPlayers(new Disconnect(player.Id), player);
                }
                timerData.Disposing = true;
                FinishProcessingReservation();
            }

            if (timerData.Counter >= timerData.MaxCounter)
            {
                Log.Error("An unexpected Error occured during InitialSync");
                Disconnect(timerData.Connection);

                timerData.Disposing = true;
                initialSyncTimer.Dispose(); // Looped long enough to require an override
            }

            timerData.Counter++;
        }
    }

    public void NonPlayerDisconnected(INitroxConnection connection)
    {
        // Remove any requests sent by the connection from the join queue
        JoinQueue = new(JoinQueue.Where(pair => !Equals(pair.Key, connection)));
    }

    public NitroxServer.Player AddConnectedPlayer(INitroxConnection connection, string reservationKey, out bool wasBrandNewPlayer)
    {
        PlayerContext playerContext = reservations[reservationKey];
        Validate.NotNull(playerContext);
        ConnectionAssets assetPackage = assetsByConnection[connection];
        Validate.NotNull(assetPackage);

        wasBrandNewPlayer = playerContext.WasBrandNewPlayer;

        if (!allPlayersByName.TryGetValue(playerContext.PlayerName, out NitroxPlayer player))
        {
            player = new NitroxPlayer(playerContext.PlayerId,
                                             playerContext.PlayerName,
                                             false,
                                             playerContext,
                                             connection,
                                             NitroxVector3.Zero,
                                             NitroxQuaternion.Identity,
                                             playerContext.PlayerNitroxId,
                                             Optional.Empty,
                                             serverConfig.DefaultPlayerPerm,
                                             serverConfig.DefaultPlayerStats,
                                             serverConfig.GameMode,
                                             new List<NitroxTechType>(),
                                             [],
                                             new Dictionary<string, NitroxId>(),
                                             new Dictionary<string, float>(),
                                             new Dictionary<string, PingInstancePreference>(),
                                             new List<int>()
            );
            allPlayersByName[playerContext.PlayerName] = player;
        }

        connectedPlayersById.Add(playerContext.PlayerId, player);

        // TODO: make a ConnectedPlayer wrapper so this is not stateful
        player.PlayerContext = playerContext;
        player.Connection = connection;

        // reconnecting players need to have their cell visibility refreshed
        player.ClearVisibleCells();

        assetPackage.Player = player;
        assetPackage.ReservationKey = null;
        reservations.Remove(reservationKey);

        return player;
    }

    public void Disconnect(INitroxConnection connection)
    {
        assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage);
        if (assetPackage == null)
        {
            return;
        }

        if (assetPackage.ReservationKey != null)
        {
            PlayerContext playerContext = reservations[assetPackage.ReservationKey];
            reservedPlayerNames.Remove(playerContext.PlayerName);
            reservations.Remove(assetPackage.ReservationKey);
        }

        if (assetPackage.Player != null)
        {
            NitroxServer.Player player = assetPackage.Player;
            reservedPlayerNames.Remove(player.Name);
            connectedPlayersById.Remove(player.Id);
        }

        assetsByConnection.Remove(connection);

        if (!ConnectedPlayers().Any())
        {
            // TODO: Pause and save server (use hibernation service).
            // NitroxServer.Server.Instance.PauseServer();
            // NitroxServer.Server.Instance.Save();
        }
    }

    public void FinishProcessingReservation(NitroxServer.Player player = null)
    {
        initialSyncTimer.Dispose();
        PlayerCurrentlyJoining = false;
        if (player != null)
        {
            BroadcastPlayerJoined(player);
        }

        Log.Info($"Finished processing reservation. Remaining requests: {JoinQueue.Count}");

        // Tell next client that it can start joining.
        if (JoinQueue.Count > 0)
        {
            KeyValuePair<INitroxConnection, MultiplayerSessionReservationRequest> keyValuePair = JoinQueue.Dequeue();
            INitroxConnection requestConnection = keyValuePair.Key;
            MultiplayerSessionReservationRequest reservationRequest = keyValuePair.Value;

            MultiplayerSessionReservation reservation = ReservePlayerContext(requestConnection,
                                                                             reservationRequest.PlayerSettings,
                                                                             reservationRequest.AuthenticationContext,
                                                                             reservationRequest.CorrelationId);

            requestConnection.SendPacket(reservation);
        }
    }

    public bool TryGetPlayerByName(string playerName, out NitroxPlayer foundPlayer)
    {
        foundPlayer = null;
        foreach (NitroxPlayer player in ConnectedPlayers())
        {
            if (player.Name == playerName)
            {
                foundPlayer = player;
                return true;
            }
        }

        return false;
    }

    public bool TryGetPlayerById(ushort playerId, out NitroxPlayer player)
    {
        return connectedPlayersById.TryGetValue(playerId, out player);
    }

    public NitroxPlayer GetPlayer(INitroxConnection connection)
    {
        if (!assetsByConnection.TryGetValue(connection, out ConnectionAssets assetPackage))
        {
            return null;
        }
        return assetPackage.Player;
    }

    public Optional<NitroxServer.Player> GetPlayer(string playerName)
    {
        allPlayersByName.TryGetValue(playerName, out NitroxPlayer player);
        return Optional.OfNullable(player);
    }

    public void SendPacketToAllPlayers(Packet packet)
    {
        foreach (NitroxServer.Player player in ConnectedPlayers())
        {
            player.SendPacket(packet);
        }
    }

    public void SendPacketToOtherPlayers(Packet packet, NitroxPlayer sendingPlayer)
    {
        foreach (NitroxPlayer player in ConnectedPlayers())
        {
            if (player != sendingPlayer)
            {
                player.SendPacket(packet);
            }
        }
    }

    public void SendPacketToOtherPlayers(Packet packet, ushort sendingPlayerId)
    {
        foreach (NitroxPlayer player in ConnectedPlayers())
        {
            if (player.Id != sendingPlayerId)
            {
                player.SendPacket(packet);
            }
        }
    }

    public IEnumerable<NitroxPlayer> ConnectedPlayers()
    {
        return assetsByConnection.Values
                                 .Where(assetPackage => assetPackage.Player != null)
                                 .Select(assetPackage => assetPackage.Player);
    }

    public void BroadcastPlayerJoined(NitroxPlayer player)
    {
        PlayerJoinedMultiplayerSession playerJoinedPacket = new(player.PlayerContext, player.SubRootId, player.Entity);
        SendPacketToOtherPlayers(playerJoinedPacket, player);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await players.GetStateAsync(cancellationToken);
        allPlayersByName = new ThreadSafeDictionary<string, NitroxPlayer>(players.State.GetPlayers().ToDictionary(x => x.Name), false);
        currentPlayerId = players.State.Players.Count == 0 ? (ushort)0 : players.State.Players.Max(x => x.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
