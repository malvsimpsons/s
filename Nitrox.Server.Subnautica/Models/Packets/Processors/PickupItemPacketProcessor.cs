using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PickupItemPacketProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, PlayerService playerService, SimulationOwnershipData simulationOwnershipData)
    : AuthenticatedPacketProcessor<PickupItem>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly PlayerService playerService = playerService;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public override void Process(PickupItem packet, NitroxServer.Player player)
    {
        if (simulationOwnershipData.RevokeOwnerOfId(packet.Id))
        {
            ushort serverId = ushort.MaxValue;
            SimulationOwnershipChange simulationOwnershipChange = new(packet.Id, serverId, SimulationLockType.TRANSIENT);
            playerService.SendPacketToAllPlayers(simulationOwnershipChange);
        }

        StopTrackingExistingWorldEntity(packet.Id);

        entityRegistry.AddOrUpdate(packet.Item);

        // Have other players respawn the item inside the inventory.
        playerService.SendPacketToOtherPlayers(new SpawnEntities(packet.Item, forceRespawn: true), player);
    }

    private void StopTrackingExistingWorldEntity(NitroxId id)
    {
        Optional<Entity> entity = entityRegistry.GetEntityById(id);

        if (entity is { HasValue: true, Value: WorldEntity worldEntity })
        {
            // Do not track this entity in the open world anymore.
            worldEntityManager.StopTrackingEntity(worldEntity);
        }
    }
}
