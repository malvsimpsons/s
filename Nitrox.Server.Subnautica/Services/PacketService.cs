extern alias JB;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Database;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Processes packets based on their type.
/// </summary>
internal sealed class PacketService(IEnumerable<IPacketProcessor> packetProcessors, PlayerService playerService, DefaultPacketProcessor defaultProcessor, IDbContextFactory<WorldDbContext> dbContextFactory, ILogger<PacketService> logger) : IHostedService
{
    private readonly DefaultPacketProcessor defaultProcessor = defaultProcessor;
    private readonly IDbContextFactory<WorldDbContext> dbContextFactory = dbContextFactory;
    private readonly ILogger<PacketService> logger = logger;
    private readonly IEnumerable<IPacketProcessor> packetProcessors = packetProcessors;
    private readonly PlayerService playerService = playerService;
    private FrozenDictionary<Type, IPacketProcessor> packetTypeToAnonProcessorLookup;
    private FrozenDictionary<Type, IPacketProcessor> packetTypeToAuthProcessorLookup;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Dictionary<Type, IPacketProcessor> authLookupBuilder = [];
        Dictionary<Type, IPacketProcessor> anonLookupBuilder = [];
        foreach (IPacketProcessor packetProcessor in packetProcessors)
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
        logger.LogDebug("{Count} anonymous packet processors found and registered", packetTypeToAnonProcessorLookup.Count);
        packetTypeToAuthProcessorLookup = authLookupBuilder.ToFrozenDictionary();
        logger.LogDebug("{Count} authenticated packet processors found and registered", packetTypeToAuthProcessorLookup.Count);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task Process(PlayerSession session, Packet packet)
    {
        if (session is { Player.Id: var playerId } && playerId > 0)
        {
            await ProcessAuth(packet, playerId);
        }
        else
        {
            await ProcessAnon(packet, session.SessionId ?? throw new Exception("Session id must not be null"));
        }
    }

    private async Task ProcessAnon(Packet packet, SessionId sessionId)
    {
        Type packetType = packet.GetType();
        if (packetTypeToAnonProcessorLookup.TryGetValue(packetType, out IPacketProcessor processor) && processor is IAnonPacketProcessor anonProcessor)
        {
            // TODO: Use object pooling for context
            await anonProcessor.Process(new AnonProcessorContext(sessionId, playerService), packet);
        }
        else
        {
            logger.LogWarning("Received invalid, unauthenticated packet: {TypeName}", packetType);
        }
    }

    private async Task ProcessAuth(Packet packet, PeerId peerId)
    {
        IPacketProcessor processor = defaultProcessor;
        Type packetType = packet.GetType();

        try
        {
            processor = packetTypeToAuthProcessorLookup.GetValueOrDefault(packetType, defaultProcessor);
            if (processor is not IAuthPacketProcessor authProcessor)
            {
                logger.LogWarning("No authenticated processor is defined for packet {TypeName}", packetType);
                return;
            }
            // TODO: Use object pooling for context
            await authProcessor.Process(new AuthProcessorContext(peerId, playerService), packet);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in packet processor {TypeName}", processor!.GetType());
        }
    }
}
