using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class EntitySpawnedByClientProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, EntitySimulation entitySimulation)
    : AuthenticatedPacketProcessor<EntitySpawnedByClient>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public override void Process(EntitySpawnedByClient packet, NitroxServer.Player playerWhoSpawned)
    {
        Entity entity = packet.Entity;

        // If the entity already exists in the registry, it is fine to update.  This is a normal case as the player
        // may have an item in their inventory (that the registry knows about) then wants to spawn it into the world.
        entityRegistry.AddOrUpdate(entity);

        SimulatedEntity simulatedEntity = null;
        if (entity is WorldEntity worldEntity)
        {
            worldEntityManager.TrackEntityInTheWorld(worldEntity);

            if (packet.RequireSimulation)
            {
                simulatedEntity = entitySimulation.AssignNewEntityToPlayer(entity, playerWhoSpawned);

                SimulationOwnershipChange ownershipChangePacket = new SimulationOwnershipChange(simulatedEntity);
                playerManager.SendPacketToAllPlayers(ownershipChangePacket);
            }
        }

        SpawnEntities spawnEntities = new(entity, simulatedEntity, packet.RequireRespawn);
        foreach (NitroxServer.Player player in playerManager.GetConnectedPlayers())
        {
            bool isOtherPlayer = player != playerWhoSpawned;
            if (isOtherPlayer && player.CanSee(entity))
            {
                player.SendPacket(spawnEntities);
            }
        }
    }
}