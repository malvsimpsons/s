extern alias JB;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Processes packets based on their type.
/// </summary>
internal sealed class PacketService(IEnumerable<PacketProcessor> packetProcessors, PlayerService playerService, DefaultPacketProcessor defaultProcessor, ILogger<PacketService> logger) : IHostedService
{
    private readonly DefaultPacketProcessor defaultProcessor = defaultProcessor;
    private readonly ILogger<PacketService> logger = logger;
    private readonly IEnumerable<PacketProcessor> packetProcessors = packetProcessors;
    private readonly PlayerService playerService = playerService;
    private FrozenDictionary<Type, PacketProcessor> packetTypeToAnonProcessorLookup;
    private FrozenDictionary<Type, PacketProcessor> packetTypeToAuthProcessorLookup;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Dictionary<Type, PacketProcessor> authLookupBuilder = [];
        Dictionary<Type, PacketProcessor> anonLookupBuilder = [];
        foreach (PacketProcessor packetProcessor in packetProcessors)
        {
            Type processorBaseType = packetProcessor.GetType().BaseType;
            Type packetType = processorBaseType?.GetGenericArguments().FirstOrDefault();
            if (!IsValidProcessor(packetType, packetProcessor))
            {
                throw new Exception("First type of a packet processor must be the packet type it can handle");
            }
            if (packetType == null)
            {
                continue;
            }
            if (processorBaseType.IsAssignableToGenericType(typeof(UnauthenticatedPacketProcessor<>)))
            {
                anonLookupBuilder[packetType] = packetProcessor;
            }
            else if (processorBaseType.IsAssignableToGenericType(typeof(AuthenticatedPacketProcessor<>)))
            {
                authLookupBuilder[packetType] = packetProcessor;
            }
            else if (packetProcessor is not DefaultPacketProcessor)
            {
                throw new Exception($"Invalid packet processor {packetProcessor.GetType().Name}");
            }
        }
        packetTypeToAnonProcessorLookup = anonLookupBuilder.ToFrozenDictionary();
        logger.LogDebug("{Count} anonymous packet processors found and registered", packetTypeToAnonProcessorLookup.Count);
        packetTypeToAuthProcessorLookup = authLookupBuilder.ToFrozenDictionary();
        logger.LogDebug("{Count} authenticated packet processors found and registered", packetTypeToAuthProcessorLookup.Count);

        return Task.CompletedTask;

        static bool IsValidProcessor(Type packetType, PacketProcessor processor)
        {
            if (packetType == null)
            {
                if (processor is DefaultPacketProcessor)
                {
                    return true;
                }
            }
            else if (typeof(Packet).IsAssignableFrom(packetType))
            {
                return true;
            }
            return false;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public void Process(Packet packet, INitroxConnection connection)
    {
        NitroxServer.Player player = playerService.GetPlayer(connection);
        PacketProcessor processor = defaultProcessor;
        Type packetType = packet.GetType();

        try
        {
            if (player == null)
            {
                if (packetTypeToAnonProcessorLookup.TryGetValue(packetType, out processor))
                {
                    processor.ProcessPacket(packet, connection);
                }
                else
                {
                    logger.LogWarning("Received invalid, unauthenticated packet: {TypeName}", packetType);
                }
            }
            else
            {
                processor = packetTypeToAuthProcessorLookup.GetValueOrDefault(packetType, defaultProcessor);
                processor.ProcessPacket(packet, player);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in packet processor {TypeName}", processor!.GetType());
        }
    }
}
