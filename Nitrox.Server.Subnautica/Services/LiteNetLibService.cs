using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.Communication;
using NitroxServer.Communication.LiteNetLib;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Opens the LiteNetLib channel and starts sending incoming messages to <see cref="packetService" /> for processing.
/// </summary>
internal class LiteNetLibService : IHostedService
{
    private readonly Dictionary<int, INitroxConnection> connectionsByRemoteIdentifier = new();
    private readonly EntitySimulation entitySimulation;
    private readonly IHostEnvironment hostEnvironment;
    private readonly ILogger<LiteNetLibService> logger;
    private readonly EventBasedNetListener listener;
    private readonly IOptions<SubnauticaServerOptions> optionsProvider;
    private readonly PacketService packetService;
    private readonly PlayerService playerService;
    private readonly NetManager server;

    public LiteNetLibService(PacketService packetService, PlayerService playerService, EntitySimulation entitySimulation, IOptions<SubnauticaServerOptions> optionsProvider, IHostEnvironment hostEnvironment, ILogger<LiteNetLibService> logger)
    {
        this.packetService = packetService;
        this.playerService = playerService;
        this.entitySimulation = entitySimulation;
        this.optionsProvider = optionsProvider;
        this.hostEnvironment = hostEnvironment;
        this.logger = logger;
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Packet.InitSerializer();

        listener.PeerConnectedEvent += PeerConnected;
        listener.PeerDisconnectedEvent += PeerDisconnected;
        listener.NetworkReceiveEvent += NetworkDataReceived;
        listener.ConnectionRequestEvent += OnConnectionRequest;

        server.ChannelsCount = (byte)typeof(Packet.UdpChannelId).GetEnumValues().Length;
        server.BroadcastReceiveEnabled = true;
        server.UnconnectedMessagesEnabled = true;
        server.UpdateTime = 15;
        server.UnsyncedEvents = true;
        if (hostEnvironment.IsDevelopment() && Debugger.IsAttached)
        {
            server.DisconnectTimeout = 300000; //Disables Timeout (for 5 min) for debug purpose (like if you jump though the server code)
        }
        if (!server.Start(optionsProvider.Value.Port))
        {
            throw new Exception("Failed to start LiteNetLib service");
        }
        logger.LogDebug("Now listening for connections");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        playerService.SendPacketToAllPlayers(new ServerStopped());
        await Task.Delay(500, cancellationToken); // We want every player to receive "ServerStopped" packet
        server.Stop();
        logger.LogDebug("stopped");
    }

    //
    //     private async Task PortForwardAsync(ushort port, CancellationToken ct = default)
    //     {
    //         if (await NatHelper.GetPortMappingAsync(port, Protocol.Udp, ct) != null)
    //         {
    //             Log.Info($"Port {port} UDP is already port forwarded");
    //             return;
    //         }
    //
    //         NatHelper.ResultCodes mappingResult = await NatHelper.AddPortMappingAsync(port, Protocol.Udp, ct);
    //         if (!ct.IsCancellationRequested)
    //         {
    //             switch (mappingResult)
    //             {
    //                 case NatHelper.ResultCodes.SUCCESS:
    //                     Log.Info($"Server port {port} UDP has been automatically opened on your router (port is closed when server closes)");
    //                     break;
    //                 case NatHelper.ResultCodes.CONFLICT_IN_MAPPING_ENTRY:
    //                     Log.Warn($"Port forward for {port} UDP failed. It appears to already be port forwarded or it conflicts with another port forward rule.");
    //                     break;
    //                 case NatHelper.ResultCodes.UNKNOWN_ERROR:
    //                     Log.Warn($"Failed to port forward {port} UDP through UPnP. If using Hamachi or you've manually port-forwarded, please disregard this warning. To disable this feature you can go into the server settings.");
    //                     break;
    //             }
    //         }
    //     }
    //
    //     public override void Stop()
    //     {
    //         if (useUpnpPortForwarding)
    //         {
    //             if (NatHelper.DeletePortMappingAsync((ushort)portNumber, Protocol.Udp, CancellationToken.None).GetAwaiter().GetResult())
    //             {
    //                 Log.Debug($"Port forward rule removed for {portNumber} UDP");
    //             }
    //             else
    //             {
    //                 Log.Warn($"Failed to remove port forward rule {portNumber} UDP");
    //             }
    //         }
    //         if (useLANBroadcast)
    //         {
    //             LANBroadcastServer.Stop();
    //         }
    //     }
    //
    private void OnConnectionRequest(ConnectionRequest request)
    {
        if (server.ConnectedPeersCount < optionsProvider.Value.MaxConnections)
        {
            request.AcceptIfKey("nitrox");
        }
        else
        {
            request.Reject();
        }
    }

    private void ClientDisconnected(INitroxConnection connection)
    {
        NitroxServer.Player player = playerService.GetPlayer(connection);

        if (player != null)
        {
            playerService.Disconnect(connection);

            Disconnect disconnect = new(player.Id);
            playerService.SendPacketToAllPlayers(disconnect);

            List<SimulatedEntity> ownershipChanges = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(player);

            if (ownershipChanges.Count > 0)
            {
                SimulationOwnershipChange ownershipChange = new(ownershipChanges);
                playerService.SendPacketToAllPlayers(ownershipChange);
            }
        }
        else
        {
            playerService.NonPlayerDisconnected(connection);
        }
    }

    private void ProcessIncomingData(INitroxConnection connection, Packet packet) => packetService.Process(packet, connection);

    private void PeerConnected(NetPeer peer)
    {
        LiteNetLibConnection connection = new(peer);
        lock (connectionsByRemoteIdentifier)
        {
            connectionsByRemoteIdentifier[peer.Id] = connection;
        }
        logger.LogInformation("Connection made by {Address}:{Port}", peer.Address, peer.Port);
    }

    private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) => ClientDisconnected(GetConnection(peer.Id));

    private void NetworkDataReceived(NetPeer peer, NetDataReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        int packetDataLength = reader.GetInt();
        byte[] packetData = ArrayPool<byte>.Shared.Rent(packetDataLength);
        try
        {
            reader.GetBytes(packetData, packetDataLength);
            Packet packet = Packet.Deserialize(packetData);
            INitroxConnection connection = GetConnection(peer.Id);
            ProcessIncomingData(connection, packet);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetData, true);
        }
    }

    private INitroxConnection GetConnection(int remoteIdentifier)
    {
        INitroxConnection connection;
        lock (connectionsByRemoteIdentifier)
        {
            connectionsByRemoteIdentifier.TryGetValue(remoteIdentifier, out connection);
        }

        return connection;
    }
}
