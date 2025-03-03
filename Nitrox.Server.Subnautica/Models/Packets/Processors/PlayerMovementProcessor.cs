using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class PlayerMovementProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<PlayerMovement>
{
    private readonly PlayerManager playerManager = playerManager;
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
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}