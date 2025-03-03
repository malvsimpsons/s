using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Processes packets based on their type.
/// </summary>
internal sealed class PacketService(IEnumerable<PacketProcessor> packetProcessors, PlayerService playerService, DefaultPacketProcessor defaultProcessor) : IHostedService
{
    private readonly DefaultPacketProcessor defaultProcessor = defaultProcessor;
    private readonly Dictionary<Type, PacketProcessor> packetProcessorAuthCache = new();

    private readonly IEnumerable<PacketProcessor> packetProcessors = packetProcessors;
    private readonly Dictionary<Type, PacketProcessor> packetProcessorUnauthCache = new();
    private readonly PlayerService playerService = playerService;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: Turn packet processors into FrozenDictionary<Type, Processor>

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Process(Packet packet, INitroxConnection connection)
    {
        NitroxServer.Player player = playerService.GetPlayer(connection);
        if (player == null)
        {
            ProcessUnauthenticated(packet, connection);
        }
        else
        {
            ProcessAuthenticated(packet, player);
        }
    }

    private void ProcessAuthenticated(Packet packet, NitroxServer.Player player)
    {
        Type packetType = packet.GetType();
        if (!packetProcessorAuthCache.TryGetValue(packetType, out PacketProcessor processor))
        {
            Type packetProcessorType = typeof(AuthenticatedPacketProcessor<>).MakeGenericType(packetType);
            packetProcessorAuthCache[packetType] = processor = NitroxServiceLocator.LocateOptionalService(packetProcessorType).Value as PacketProcessor;
        }

        if (processor != null)
        {
            try
            {
                processor.ProcessPacket(packet, player);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error in packet processor {processor.GetType()}");
            }
        }
        else
        {
            defaultProcessor.ProcessPacket(packet, player);
        }
    }

    private void ProcessUnauthenticated(Packet packet, INitroxConnection connection)
    {
        Type packetType = packet.GetType();
        if (!packetProcessorUnauthCache.TryGetValue(packetType, out PacketProcessor processor))
        {
            Type packetProcessorType = typeof(UnauthenticatedPacketProcessor<>).MakeGenericType(packetType);
            packetProcessorUnauthCache[packetType] = processor = NitroxServiceLocator.LocateOptionalService(packetProcessorType).Value as PacketProcessor;
        }
        if (processor == null)
        {
            Log.Warn($"Received invalid, unauthenticated packet: {packet}");
            return;
        }

        try
        {
            processor.ProcessPacket(packet, connection);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error in packet processor {processor.GetType()}");
        }
    }
}
