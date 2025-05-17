using System;
using System.Collections.Generic;
using Nitrox.Server.Subnautica.Database.Models;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Respositories.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Networking.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class EntitySimulation : ISessionCleaner
{
    private const SimulationLockType DEFAULT_ENTITY_SIMULATION_LOCKTYPE = SimulationLockType.TRANSIENT;

    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;
    private readonly IServerPacketSender packetSender;
    private readonly ISimulationWhitelist simulationWhitelist;
    private readonly SimulationOwnershipData simulationOwnershipData;

    public EntitySimulation(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData, IServerPacketSender packetSender, ISimulationWhitelist simulationWhitelist)
    {
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
        this.simulationOwnershipData = simulationOwnershipData;
        this.packetSender = packetSender;
        this.simulationWhitelist = simulationWhitelist;
    }

    public List<SimulatedEntity> GetSimulationChangesForCell(PeerId player, AbsoluteEntityCell cell)
    {
        List<WorldEntity> entities = worldEntityManager.GetEntities(cell);
        List<WorldEntity> addedEntities = FilterSimulatableEntities(player, entities);

        List<SimulatedEntity> ownershipChanges = new();

        foreach (WorldEntity entity in addedEntities)
        {
            bool doesEntityMove = ShouldSimulateEntityMovement(entity);
            ownershipChanges.Add(new SimulatedEntity(entity.Id, player, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE));
        }

        return ownershipChanges;
    }

    public void FillWithRemovedCells(PeerId player, AbsoluteEntityCell removedCell, List<SimulatedEntity> ownershipChanges)
    {
        // TODO: USE DATABASE
        // List<WorldEntity> entities = worldEntityManager.GetEntities(removedCell);
        // IEnumerable<WorldEntity> revokedEntities = entities.Where(entity => !player.CanSee(entity) && simulationOwnershipData.RevokeIfOwner(entity.Id, player));
        // AssignEntitiesToOtherPlayers(player, revokedEntities, ownershipChanges);
    }

    public async Task BroadcastSimulationChanges(List<SimulatedEntity> ownershipChanges)
    {
        if (ownershipChanges.Count > 0)
        {
            SimulationOwnershipChange ownershipChange = new(ownershipChanges);
            await packetSender.SendPacketToAll(ownershipChange);
        }
    }

    public List<SimulatedEntity> CalculateSimulationChangesFromPlayerDisconnect(PeerId player)
    {
        List<SimulatedEntity> ownershipChanges = new();

        List<NitroxId> revokedEntityIds = simulationOwnershipData.RevokeAllForOwner(player);
        List<Entity> revokedEntities = entityRegistry.GetEntities(revokedEntityIds);

        AssignEntitiesToOtherPlayers(player, revokedEntities, ownershipChanges);

        return ownershipChanges;
    }

    public SimulatedEntity AssignNewEntityToPlayer(Entity entity, PeerId player, bool shouldEntityMove = true)
    {
        if (simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
        {
            bool doesEntityMove = shouldEntityMove && entity is WorldEntity worldEntity && ShouldSimulateEntityMovement(worldEntity);
            return new SimulatedEntity(entity.Id, player, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
        }

        throw new Exception($"New entity was already being simulated by someone else: {entity.Id}");
    }

    public List<SimulatedEntity> AssignGlobalRootEntitiesAndGetData(PeerId player)
    {
        List<SimulatedEntity> simulatedEntities = new();
        foreach (GlobalRootEntity entity in worldEntityManager.GetGlobalRootEntities())
        {
            simulationOwnershipData.TryToAcquire(entity.Id, player, SimulationLockType.TRANSIENT);
            if (!simulationOwnershipData.TryGetLock(entity.Id, out SimulationOwnershipData.PlayerLock playerLock))
            {
                continue;
            }
            bool doesEntityMove = ShouldSimulateEntityMovement(entity);
            SimulatedEntity simulatedEntity = new(entity.Id, playerLock.PlayerId, doesEntityMove, playerLock.LockType);
            simulatedEntities.Add(simulatedEntity);
        }
        return simulatedEntities;
    }

    private void AssignEntitiesToOtherPlayers(PeerId oldPlayer, IEnumerable<Entity> entities, List<SimulatedEntity> ownershipChanges)
    {
        // TODO: USE DATABASE
        // List<PeerId> otherPlayers = playerService.GetConnectedPlayersExcept(oldPlayer);
        //
        // foreach (Entity entity in entities)
        // {
        //     if (TryAssignEntityToPlayers(otherPlayers, entity, out SimulatedEntity simulatedEntity))
        //     {
        //         ownershipChanges.Add(simulatedEntity);
        //     }
        // }
    }

    public bool TryAssignEntityToPlayers(List<PeerId> players, Entity entity, out SimulatedEntity simulatedEntity)
    {
        NitroxId id = entity.Id;

        // TODO: USE DATABASE
        // foreach (PeerId player in players)
        // {
        //     if (player.CanSee(entity) && simulationOwnershipData.TryToAcquire(id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE))
        //     {
        //         bool doesEntityMove = entity is WorldEntity worldEntity && ShouldSimulateEntityMovement(worldEntity);
        //
        //         Log.Verbose($"Player {player.Name} has taken over simulating {id}");
        //         simulatedEntity = new(id, player.Id, doesEntityMove, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
        //         return true;
        //     }
        // }
        
        simulatedEntity = null;
        return false;
    }

    private List<WorldEntity> FilterSimulatableEntities(PeerId player, List<WorldEntity> entities)
    {
        return [];

        // TODO: USE DATABASE
        // return entities.Where(entity => {
        //     bool isEligibleForSimulation = player.CanSee(entity) && ShouldSimulateEntity(entity);
        //     return isEligibleForSimulation && simulationOwnershipData.TryToAcquire(entity.Id, player, DEFAULT_ENTITY_SIMULATION_LOCKTYPE);
        // }).ToList();
    }

    public bool ShouldSimulateEntity(WorldEntity entity)
    {
        return simulationWhitelist.UtilityWhitelist.Contains(entity.TechType) || ShouldSimulateEntityMovement(entity);
    }

    public bool ShouldSimulateEntityMovement(WorldEntity entity)
    {
        return !entity.SpawnedByServer || simulationWhitelist.MovementWhitelist.Contains(entity.TechType);    
    }

    public bool ShouldSimulateEntityMovement(NitroxId entityId)
    {
        return entityRegistry.TryGetEntityById(entityId, out WorldEntity worldEntity) && ShouldSimulateEntityMovement(worldEntity);
    }

    public void EntityDestroyed(NitroxId id)
    {
        simulationOwnershipData.RevokeOwnerOfId(id);
    }

    public async Task ClaimBuildPiece(Entity entity, PeerId player)
    {
        SimulatedEntity simulatedEntity = AssignNewEntityToPlayer(entity, player, false);
        SimulationOwnershipChange ownershipChangePacket = new(simulatedEntity);
        await packetSender.SendPacketToAll(ownershipChangePacket);
    }

    public async Task CleanSessionAsync(Session disconnectedSession)
    {
        if (disconnectedSession is not { Player.Id: var playerId })
        {
            return;
        }

        List<SimulatedEntity> ownershipChanges = CalculateSimulationChangesFromPlayerDisconnect(playerId);
        if (ownershipChanges.Count > 0)
        {
            SimulationOwnershipChange ownershipChange = new(ownershipChanges);
            await packetSender.SendPacketToAll(ownershipChange);
        }
    }
}
