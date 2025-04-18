using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerMovementProcessor(PlayerService playerService, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<PlayerMovement>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(PlayerMovement packet, NitroxServer.Player player)
    {
        Optional<PlayerWorldEntity> playerEntity = entityRegistry.GetEntityById<PlayerWorldEntity>(player.PlayerContext.PlayerNitroxId);

        if (playerEntity.HasValue)
        {
            playerEntity.Value.Transform.Position = packet.Position;
            playerEntity.Value.Transform.Rotation = packet.BodyRotation;
        }

        player.Position = packet.Position;
        player.Rotation = packet.BodyRotation;
        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
