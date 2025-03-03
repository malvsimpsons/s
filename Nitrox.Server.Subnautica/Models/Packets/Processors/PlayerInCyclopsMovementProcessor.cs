using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlayerInCyclopsMovementProcessor(PlayerManager playerManager, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<PlayerInCyclopsMovement>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(PlayerInCyclopsMovement packet, NitroxServer.Player player)
    {
        if (entityRegistry.TryGetEntityById(player.PlayerContext.PlayerNitroxId, out PlayerWorldEntity playerWorldEntity))
        {
            playerWorldEntity.Transform.LocalPosition = packet.LocalPosition;
            playerWorldEntity.Transform.LocalRotation = packet.LocalRotation;

            player.Position = playerWorldEntity.Transform.Position;
            player.Rotation = playerWorldEntity.Transform.Rotation;
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
        else
        {
            Log.ErrorOnce($"{nameof(PlayerWorldEntity)} couldn't be found for player {player.Name}. It is adviced the player reconnects before losing too much progression.");
        }
    }
}
