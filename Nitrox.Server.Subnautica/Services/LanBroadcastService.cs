using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using NitroxModel.Constants;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Broadcasts the server port over LAN. Clients listening for this broadcast automatically add this server to the server list.
///     TODO: Verify this works when changing ports at runtime via config.
/// </summary>
internal class LanBroadcastService(IOptions<SubnauticaServerOptions> optionsProvider, ILogger<LanBroadcastService> logger) : BackgroundService
{
    private readonly ILogger<LanBroadcastService> logger = logger;
    private readonly IOptions<SubnauticaServerOptions> optionsProvider = optionsProvider;
    private readonly PeriodicTimer pollTimer = new(TimeSpan.FromMilliseconds(100));
    private EventBasedNetListener listener;

    private NetManager server;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        listener = new EventBasedNetListener();
        listener.NetworkReceiveUnconnectedEvent += NetworkReceiveUnconnected;
        server = new NetManager(listener)
        {
            AutoRecycle = true,
            BroadcastReceiveEnabled = true,
            UnconnectedMessagesEnabled = true
        };

        int selectedPort = 0;
        foreach (int port in LANDiscoveryConstants.BROADCAST_PORTS)
        {
            if (server.Start(port))
            {
                selectedPort = port;
                break;
            }
        }

        logger.LogInformation("started");
        logger.LogDebug("broadcasting on port {Port}", selectedPort);
        try
        {
            while (true)
            {
                await pollTimer.WaitForNextTickAsync(stoppingToken);
                server.PollEvents();
            }
        }
        catch (OperationCanceledException)
        {
            listener?.ClearNetworkReceiveUnconnectedEvent();
            server?.Stop();
            pollTimer?.Dispose();
            logger.LogDebug("stopped");
            throw;
        }
    }

    private void NetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType != UnconnectedMessageType.Broadcast)
        {
            return;
        }
        string requestString = reader.GetString();
        if (requestString != LANDiscoveryConstants.BROADCAST_REQUEST_STRING)
        {
            return;
        }

        ushort port = optionsProvider.Value.Port;
        logger.LogDebug("Broadcasting server port {Port} over LAN...", port);
        NetDataWriter writer = new();
        writer.Put(LANDiscoveryConstants.BROADCAST_RESPONSE_STRING);
        writer.Put(port);
        server.SendBroadcast(writer, remoteEndPoint.Port);
    }
}
