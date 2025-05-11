extern alias JB;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Collects packet processors into a fast lookup, based on the packet type they can handle.
/// </summary>
internal sealed class PacketRegistryService(Func<IPacketProcessor[]> packetProcessorsProvider, DefaultPacketProcessor defaultProcessor, ILogger<PacketRegistryService> logger) : IHostedService
{
    private readonly DefaultPacketProcessor defaultProcessor = defaultProcessor;
    private readonly ILogger<PacketRegistryService> logger = logger;
    private FrozenDictionary<Type, IPacketProcessor> packetTypeToAnonProcessorLookup;
    private FrozenDictionary<Type, IPacketProcessor> packetTypeToAuthProcessorLookup;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Dictionary<Type, IPacketProcessor> authLookupBuilder = [];
        Dictionary<Type, IPacketProcessor> anonLookupBuilder = [];
        foreach (IPacketProcessor packetProcessor in packetProcessorsProvider())
        {
            Type processorType = packetProcessor.GetType();
            Type packetType = processorType.GetInterfaces()
                                           .Where(i => typeof(IPacketProcessor).IsAssignableFrom(i))
                                           .Select(i => i.GetGenericArguments().FirstOrDefault())
                                           .FirstOrDefault(a => a != null && typeof(Packet).IsAssignableFrom(a) && a != typeof(Packet));
            if (packetType == null)
            {
                if (processorType == typeof(DefaultPacketProcessor))
                {
                    continue;
                }
                throw new Exception("A packet processor must have an interface that specifies the packet type it can handle");
            }
            if (typeof(IAnonPacketProcessor).IsAssignableFrom(processorType))
            {
                anonLookupBuilder[packetType] = packetProcessor;
            }
            else if (typeof(IAuthPacketProcessor).IsAssignableFrom(processorType))
            {
                authLookupBuilder[packetType] = packetProcessor;
            }
            else if (packetProcessor is not DefaultPacketProcessor)
            {
                throw new Exception($"Invalid packet processor {packetProcessor.GetType().Name}");
            }
        }
        packetTypeToAnonProcessorLookup = anonLookupBuilder.ToFrozenDictionary();
        logger.ZLogDebug($"{packetTypeToAnonProcessorLookup.Count:@Count} anonymous packet processors found and registered");
        packetTypeToAuthProcessorLookup = authLookupBuilder.ToFrozenDictionary();
        logger.ZLogDebug($"{packetTypeToAuthProcessorLookup.Count:@Count} authenticated packet processors found and registered");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public IPacketProcessor GetProcessor(PlayerSession session, Packet packet)
    {
        Type packetType = packet.GetType();
        if (session is { Player.Id: var playerId } && playerId > 0)
        {
            IPacketProcessor processor = packetTypeToAuthProcessorLookup.GetValueOrDefault(packetType, defaultProcessor);
            if (processor is not IAuthPacketProcessor authProcessor)
            {
                logger.ZLogWarning($"No authenticated processor is defined for packet {packetType:@TypeName}");
                return null;
            }

            return authProcessor;
        }
        else
        {
            if (packetTypeToAnonProcessorLookup.TryGetValue(packetType, out IPacketProcessor processor) && processor is IAnonPacketProcessor anonProcessor)
            {
                return anonProcessor;
            }
        }

        return null;
    }
}
