using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets;

/// <summary>
///     The default packet processor for packets which don't define one. This processor will send those packets to other
///     players as they were received.
/// </summary>
internal sealed class DefaultPacketProcessor(PlayerService playerService, ILogger<DefaultPacketProcessor> logger) : AuthenticatedPacketProcessor<Packet>
{
    /// <summary>
    ///     Packet types which don't have a server packet processor but should not be transmitted
    /// </summary>
    private readonly HashSet<Type> defaultPacketProcessorBlacklist =
    [
        typeof(GameModeChanged),
        typeof(DropSimulationOwnership)
    ];

    private readonly ILogger<DefaultPacketProcessor> logger = logger;

    private readonly HashSet<Type> loggingPacketBlackList =
    [
        typeof(AnimationChangeEvent),
        typeof(PlayerMovement),
        typeof(ItemPosition),
        typeof(PlayerStats),
        typeof(StoryGoalExecuted),
        typeof(FMODAssetPacket),
        typeof(FMODCustomEmitterPacket),
        typeof(FMODCustomLoopingEmitterPacket),
        typeof(FMODStudioEmitterPacket),
        typeof(PlayerCinematicControllerCall),
        typeof(TorpedoShot),
        typeof(TorpedoHit),
        typeof(TorpedoTargetAcquired),
        typeof(StasisSphereShot),
        typeof(StasisSphereHit),
        typeof(SeaTreaderChunkPickedUp)
    ];

    private readonly PlayerService playerService = playerService;

    public override void Process(Packet packet, NitroxServer.Player player)
    {
        if (!loggingPacketBlackList.Contains(packet.GetType()))
        {
            logger.LogDebug("Using default packet processor for: {Packet} and player {PlayerId}", packet, player.Id);
        }

        if (defaultPacketProcessorBlacklist.Contains(packet.GetType()))
        {
            logger.LogErrorOnce("Player {PlayerName} [{PlayerId}] sent a packet which is blacklisted by the server. It's likely that the said player is using a modified version of Nitrox and action could be taken accordingly.", player.Name, player.Id);
            return;
        }
        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
