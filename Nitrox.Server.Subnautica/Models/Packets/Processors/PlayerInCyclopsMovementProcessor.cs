using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerInCyclopsMovementProcessor(PlayerService playerService, EntityRegistry entityRegistry, ILogger<PlayerInCyclopsMovementProcessor> logger) : AuthenticatedPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(PlayerInCyclopsMovement packet, NitroxServer.Player player)
    {
        if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerWorldEntity playerWorldEntity))
        {
            playerWorldEntity.Transform.LocalPosition = packet.LocalPosition;
            playerWorldEntity.Transform.LocalRotation = packet.LocalRotation;

            player.Position = playerWorldEntity.Transform.Position;
            player.Rotation = playerWorldEntity.Transform.Rotation;
            playerService.SendPacketToOtherPlayers(packet, player);
        }
        else
        {
            logger.LogErrorOnce("{TypeName} couldn't be found for player {PlayerName}. It is advised the player reconnects before losing too much progression.", nameof(PlayerWorldEntity), player.Name);
        }
    }
}
