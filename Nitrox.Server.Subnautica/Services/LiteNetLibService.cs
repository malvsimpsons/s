using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using NitroxModel.Networking.Packets;
using NitroxServer.Communication.LiteNetLib;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Opens the LiteNetLib channel and starts sending incoming messages to <see cref="packetService" /> for processing.
/// </summary>
internal class LiteNetLibService : BackgroundService
{
    private readonly EntitySimulation entitySimulation;
    private readonly IHostEnvironment hostEnvironment;
    private readonly EventBasedNetListener listener;
    private readonly Lock lnlPeerIdToSessionLocker = new();
    private readonly Dictionary<int, SessionObject> lnlPeerIdToSession = [];
    private readonly ILogger<LiteNetLibService> logger;
    private readonly IOptions<SubnauticaServerOptions> optionsProvider;
    private readonly PacketService packetService;
    private readonly PlayerService playerService;
    private readonly Channel<Task> taskChannel = Channel.CreateUnbounded<Task>();
    private readonly NetManager server;
    private readonly IDbContextFactory<WorldDbContext> dbContextFactory;

    public LiteNetLibService(PacketService packetService, PlayerService playerService, EntitySimulation entitySimulation, IOptions<SubnauticaServerOptions> optionsProvider, IHostEnvironment hostEnvironment, IDbContextFactory<WorldDbContext> dbContextFactory, ILogger<LiteNetLibService> logger)
    {
        this.packetService = packetService;
        this.playerService = playerService;
        this.entitySimulation = entitySimulation;
        this.optionsProvider = optionsProvider;
        this.hostEnvironment = hostEnvironment;
        this.dbContextFactory = dbContextFactory;
        this.logger = logger;
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Packet.InitSerializer();

        listener.PeerConnectedEvent += PeerConnected;
        listener.PeerDisconnectedEvent += (peer, _) => ClientDisconnected(GetSession(peer.Id));
        listener.NetworkReceiveEvent += NetworkDataReceived;
        listener.ConnectionRequestEvent += OnConnectionRequest;

        server.ChannelsCount = (byte)typeof(Packet.UdpChannelId).GetEnumValues().Length;
        server.BroadcastReceiveEnabled = true;
        server.UnconnectedMessagesEnabled = true;
        server.UnsyncedEvents = true;
        server.UpdateTime = 15;
        if (hostEnvironment.IsDevelopment() && Debugger.IsAttached)
        {
            server.DisconnectTimeout = 300000; //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
        }
        if (!server.Start(optionsProvider.Value.ServerPort))
        {
            throw new Exception("Failed to start LiteNetLib service");
        }
        logger.LogDebug("Now listening for connections");

        try
        {
            await foreach (Task task in taskChannel.Reader.ReadAllAsync(stoppingToken))
            {
                await task;
            }
        }
        catch (OperationCanceledException)
        {
            playerService.SendPacketToAllPlayers(new ServerStopped());
            while (server.PoolCount > 0)
            {
                server.PollEvents();
                await Task.Delay(15, CancellationToken.None);
            }
            server.Stop();
            logger.LogDebug("stopped");
            listener.ClearPeerConnectedEvent();
            listener.ClearPeerDisconnectedEvent();
            listener.ClearNetworkReceiveEvent();
            listener.ClearConnectionRequestEvent();
            throw;
        }
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        // TODO: Check if we have session IDs available (from database)

        if (server.ConnectedPeersCount < optionsProvider.Value.MaxConnections)
        {
            request.AcceptIfKey("nitrox");
        }
        else
        {
            request.Reject();
        }
    }

    private void ClientDisconnected(SessionObject session)
    {
        Debug.Assert(session != null);

        // TODO: Move this outside of LiteNetLib service.
        // if (player != null)
        // {
        //     playerService.Disconnect(session.Id);
        //
        //     Disconnect disconnect = new(player.Id);
        //     playerService.SendPacketToAllPlayers(disconnect);
        //
        //     List<SimulatedEntity> ownershipChanges = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);
        //
        //     if (ownershipChanges.Count > 0)
        //     {
        //         SimulationOwnershipChange ownershipChange = new(ownershipChanges);
        //         playerService.SendPacketToAllPlayers(ownershipChange);
        //     }
        // }
        // else
        // {
        //     playerService.NonPlayerDisconnected(connection);
        // }
    }

    private void PeerConnected(NetPeer peer)
    {
        lock (lnlPeerIdToSessionLocker)
        {
            // 0 session id will be fixed by ProcessPacket().
            lnlPeerIdToSession[peer.Id] = new SessionObject(0, new(peer));
        }
        logger.LogInformation("Connection made by {Address}:{Port}", peer.Address, peer.Port);
    }

    private void NetworkDataReceived(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet packet = Packet.Deserialize(packetData);
            SessionObject sessionObject = GetSession(peer.Id);
            Debug.Assert(sessionObject != null);
            if (!taskChannel.Writer.TryWrite(ProcessPacket(sessionObject, packet)))
            {
                logger.LogError("Failed to queue packet processor task for packet type {TypeName} by session id {SessionId}", packet.GetType().Name, sessionObject.Id);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetData, true);
        }
    }

    private async Task ProcessPacket(SessionObject session, Packet packet)
    {
        PlayerSession playerSession = null;
        await using (WorldDbContext db = await dbContextFactory.CreateDbContextAsync())
        {
            if (session.Id != 0)
            {
                playerSession = await db.PlayerSessions
                                        .AsTracking()
                                        .Include(s => s.Player)
                                        .Where(s => s.SessionId == session.Id)
                                        .FirstOrDefaultAsync();
            }
            if (playerSession == null)
            {
                playerSession = new PlayerSession();
                db.PlayerSessions.Add(playerSession);
                if (await db.SaveChangesAsync() != 1)
                {
                    logger.LogError("Failed to create session for {EndPoint}", session.Connection.Endpoint);
                    return;
                }
            }
        }
        lock (lnlPeerIdToSessionLocker)
        {
            session.Id = playerSession.SessionId ?? throw new Exception("Failed to generate session id for new connection");
        }

        await packetService.Process(playerSession, packet);
    }

    private SessionObject GetSession(int lnlPeerId)
    {
        lock (lnlPeerIdToSessionLocker)
        {
            if (lnlPeerIdToSession.TryGetValue(lnlPeerId, out SessionObject obj))
            {
                return obj;
            }
        }
        return null;
    }

    /// <summary>
    ///     Internal session object to track which Nitrox session id belong to a LiteNetLib connection.
    /// </summary>
    private record SessionObject(SessionId Id, LiteNetLibConnection Connection)
    {
        public SessionId Id { get; set; } = Id;
    }
}
