using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityDestroyedPacketProcessor(PlayerService playerService, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : AuthenticatedPacketProcessor<EntityDestroyed>
{
    private readonly PlayerService playerManager = playerService;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public override void Process(EntityDestroyed packet, NitroxServer.Player destroyingPlayer)
    {
        entitySimulation.EntityDestroyed(packet.Id);

        if (worldEntityManager.TryDestroyEntity(packet.Id, out Entity entity))
        {
            if (entity is VehicleWorldEntity vehicleWorldEntity)
            {
                worldEntityManager.MovePlayerChildrenToRoot(vehicleWorldEntity);
            }

            foreach (NitroxServer.Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != destroyingPlayer;
                if (isOtherPlayer && player.CanSee(entity))
                {
                    player.SendPacket(packet);
                }
            }
        }
    }
}
