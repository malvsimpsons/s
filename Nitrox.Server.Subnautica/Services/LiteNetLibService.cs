using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Respositories;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Opens the LiteNetLib channel and starts sending incoming messages to <see cref="packetService" /> for processing.
/// </summary>
internal class LiteNetLibService : BackgroundService
{
    private readonly IHostEnvironment hostEnvironment;
    private readonly SessionRepository sessionRepository;
    private readonly EventBasedNetListener listener;
    private readonly ILogger<LiteNetLibService> logger;
    private readonly IOptions<SubnauticaServerOptions> optionsProvider;
    private readonly PacketService packetService;
    private readonly PlayerService playerService;
    private readonly NetManager server;
    private readonly Channel<Task> taskChannel = Channel.CreateUnbounded<Task>();

    public LiteNetLibService(PacketService packetService, PlayerService playerService, IOptions<SubnauticaServerOptions> optionsProvider, IHostEnvironment hostEnvironment, SessionRepository sessionRepository, ILogger<LiteNetLibService> logger)
    {
        this.packetService = packetService;
        this.playerService = playerService;
        this.optionsProvider = optionsProvider;
        this.hostEnvironment = hostEnvironment;
        this.sessionRepository = sessionRepository;
        this.logger = logger;
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Packet.InitSerializer();

        listener.PeerConnectedEvent += PeerConnected;
        listener.PeerDisconnectedEvent += (peer, _) => ClientDisconnected(peer);
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
            await Task.Delay(500, CancellationToken.None); // TODO: Need async function to wait for all packets to be sent away.
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
        if (request.Data.GetString() != "nitrox")
        {
            request.Reject();
            return;
        }
        if (server.ConnectedPeersCount >= optionsProvider.Value.MaxConnections)
        {
            request.Reject();
            return;
        }

        if (!taskChannel.Writer.TryWrite(ProcessConnectionRequestAsync(sessionRepository, request)))
        {
            logger.LogWarning("Failed to queue client connect request task for {Address}:{Port}", request.RemoteEndPoint.Address.ToString(), request.RemoteEndPoint.Port);
        }

        static async Task ProcessConnectionRequestAsync(SessionRepository sessionRepository, ConnectionRequest request)
        {
            PlayerSession session = await sessionRepository.GetOrCreateSessionAsync(request.RemoteEndPoint.Address.ToString(), (ushort)request.RemoteEndPoint.Port);
            if (session == null)
            {
                request.Reject();
                return;
            }
            request.Accept();
        }
    }

    private void ClientDisconnected(NetPeer peer)
    {
        string address = peer.Address.ToString();
        if (!taskChannel.Writer.TryWrite(sessionRepository.DeleteSessionAsync(address, (ushort)peer.Port)))
        {
            logger.LogWarning("Failed to queue client disconnect task for {Address}:{Port}", address, peer.Port);
        }
    }

    private void PeerConnected(NetPeer peer) => logger.LogInformation("Connection made by {Address}:{Port}", peer.Address, peer.Port);

    private void NetworkDataReceived(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet packet = Packet.Deserialize(packetData);
            if (!taskChannel.Writer.TryWrite(ProcessPacket(peer, packet)))
            {
                logger.LogError("Failed to queue packet processor task for packet type {TypeName} from {Address}:{Port}", packet.GetType().Name, peer.Address, peer.Port);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetData, true);
        }
    }

    private async Task ProcessPacket(NetPeer peer, Packet packet)
    {
        PlayerSession session = await sessionRepository.GetOrCreateSessionAsync(peer.Address.ToString(), (ushort)peer.Port);
        logger.LogTrace("Incoming packet {TypeName} by session #{SessionId}", packet.GetType().Name, session.SessionId);
        await packetService.Process(session, packet);
    }
}
