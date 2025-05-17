using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;
using NitroxModel.Serialization;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Bases;

internal class BuildingManager(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, IServerPacketSender packetSender, IOptions<SubnauticaServerConfig> configProvider, ILogger<BuildingManager> logger)
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly IOptions<SubnauticaServerConfig> configProvider = configProvider;
    private readonly ILogger<BuildingManager> logger = logger;
    private readonly IServerPacketSender packetSender = packetSender;

    public bool AddGhost(PlaceGhost placeGhost)
    {
        GhostEntity ghostEntity = placeGhost.GhostEntity;
        if (ghostEntity.ParentId == null)
        {
            if (entityRegistry.GetEntityById(ghostEntity.Id).HasValue)
            {
                logger.ZLogError($"Trying to add a ghost to Global Root but another entity with the same id already exists (GhostId: {ghostEntity.Id:@GhostId})");
                return false;
            }

            worldEntityManager.AddOrUpdateGlobalRootEntity(ghostEntity);
            return true;
        }

        if (!entityRegistry.TryGetEntityById(ghostEntity.ParentId, out Entity parentEntity))
        {
            logger.ZLogError($"Trying to add a ghost to a build that isn't registered (ParentId: {ghostEntity.ParentId})");
            return false;
        }
        if (parentEntity is not BuildEntity)
        {
            logger.ZLogError($"Trying to add a ghost to an entity that is not a building (ParentId: {ghostEntity.ParentId})");
            return false;
        }
        if (parentEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(ghostEntity.Id)))
        {
            logger.ZLogError($"Trying to add a ghost to a building but another child with the same id already exists (GhostId: {ghostEntity.Id})");
            return false;
        }

        worldEntityManager.AddOrUpdateGlobalRootEntity(ghostEntity);
        return true;
    }

    public bool AddModule(PlaceModule placeModule)
    {
        ModuleEntity moduleEntity = placeModule.ModuleEntity;
        if (moduleEntity.ParentId == null)
        {
            if (entityRegistry.GetEntityById(moduleEntity.Id).HasValue)
            {
                logger.ZLogError($"Trying to add a module to Global Root but another entity with the same id already exists ({moduleEntity.Id})");
                return false;
            }

            worldEntityManager.AddOrUpdateGlobalRootEntity(moduleEntity);
            return true;
        }

        if (!entityRegistry.TryGetEntityById(moduleEntity.ParentId, out Entity parentEntity))
        {
            logger.ZLogError($"Trying to add a module to a build that isn't registered (ParentId: {moduleEntity.ParentId})");
            return false;
        }
        if (parentEntity is not BuildEntity && parentEntity is not VehicleWorldEntity)
        {
            logger.ZLogError($"Trying to add a module to an entity that is not a building/vehicle (ParentId: {moduleEntity.ParentId})");
            return false;
        }
        if (parentEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(moduleEntity.Id)))
        {
            logger.ZLogError($"Trying to add a module to a building but another child with the same id already exists (ModuleId: {moduleEntity.Id})");
            return false;
        }

        worldEntityManager.AddOrUpdateGlobalRootEntity(moduleEntity);
        return true;
    }

    public bool ModifyConstructedAmount(ModifyConstructedAmount modifyConstructedAmount)
    {
        if (!entityRegistry.TryGetEntityById(modifyConstructedAmount.GhostId, out Entity entity))
        {
            logger.ZLogError($"Trying to modify the constructed amount of a non-registered object (GhostId: {modifyConstructedAmount.GhostId})");
            return false;
        }

        // Certain entities with a Constructable are just "regular" WorldEntities (e.g. starship boxes) and for simplicity we'll just not persist their progress
        // since their only use is to be deconstructed to give materials to players
        if (entity is not GhostEntity && entity is not ModuleEntity)
        {
            // In case the entity was fully deconstructed
            if (modifyConstructedAmount.ConstructedAmount == 0f)
            {
                if (entity is GlobalRootEntity)
                {
                    worldEntityManager.RemoveGlobalRootEntity(entity.Id);
                }
                else
                {
                    entityRegistry.RemoveEntity(entity.Id);
                }
            }

            // In any case we'll broadcast the packet
            return true;
        }
        if (modifyConstructedAmount.ConstructedAmount == 0f)
        {
            worldEntityManager.RemoveGlobalRootEntity(entity.Id);
            return true;
        }

        switch (entity)
        {
            case GhostEntity ghostEntity:
                ghostEntity.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
                break;
            case ModuleEntity moduleEntity:
                moduleEntity.ConstructedAmount = modifyConstructedAmount.ConstructedAmount;
                break;
        }
        return true;
    }

    public bool CreateBase(PlaceBase placeBase)
    {
        if (!entityRegistry.TryGetEntityById(placeBase.FormerGhostId, out Entity entity))
        {
            logger.ZLogError($"Trying to place a base from a non-registered ghost (Id: {placeBase.FormerGhostId})");
            return false;
        }
        if (entity is not GhostEntity)
        {
            logger.ZLogError($"Trying to add a new build to Global Root but another build with the same id already exists (GhostId: {placeBase.FormerGhostId})");
            return false;
        }

        worldEntityManager.RemoveGlobalRootEntity(entity.Id);
        worldEntityManager.AddOrUpdateGlobalRootEntity(placeBase.BuildEntity);
        return true;
    }

    public bool UpdateBase(PeerId player, UpdateBase updateBase, out int operationId)
    {
        // TODO: MADE THIS ASYNC SO CAN FETCH PLAYER NAME FOR LOG!
        operationId = -1;
        return true;

        // if (!entityRegistry.TryGetEntityById<GhostEntity>(updateBase.FormerGhostId, out _))
        // {
        //     Log.Error($"Trying to place a base from a non-registered ghost (GhostId: {updateBase.FormerGhostId})");
        //     operationId = -1;
        //     return false;
        // }
        // if (!entityRegistry.TryGetEntityById(updateBase.BaseId, out BuildEntity buildEntity))
        // {
        //     Log.Error($"Trying to update a non-registered build (BaseId: {updateBase.BaseId})");
        //     operationId = -1;
        //     return false;
        // }
        // int deltaOperations = buildEntity.OperationId + 1 - updateBase.OperationId;
        // if (deltaOperations != 0 && configProvider.Value.SafeBuilding)
        // {
        //     logger.LogWarning("Ignoring an {TypeName} packet from [{PlayerName}] which is {Operation}", nameof(UpdateBase), player.Name, Math.Abs(deltaOperations) + (deltaOperations > 0 ? " operations ahead" : " operations late"));
        //     NotifyPlayerDesync(player);
        //     operationId = -1;
        //     return false;
        // }
        //
        // worldEntityManager.RemoveGlobalRootEntity(updateBase.FormerGhostId);
        // buildEntity.BaseData = updateBase.BaseData;
        //
        // foreach (KeyValuePair<NitroxId, NitroxBaseFace> updatedChild in updateBase.UpdatedChildren)
        // {
        //     if (entityRegistry.TryGetEntityById(updatedChild.Key, out InteriorPieceEntity childEntity))
        //     {
        //         childEntity.BaseFace = updatedChild.Value;
        //     }
        // }
        // foreach (KeyValuePair<NitroxId, NitroxInt3> updatedMoonpool in updateBase.UpdatedMoonpools)
        // {
        //     if (entityRegistry.TryGetEntityById(updatedMoonpool.Key, out MoonpoolEntity childEntity))
        //     {
        //         childEntity.Cell = updatedMoonpool.Value;
        //     }
        // }
        // foreach (KeyValuePair<NitroxId, NitroxInt3> updatedMapRoom in updateBase.UpdatedMapRooms)
        // {
        //     if (entityRegistry.TryGetEntityById(updatedMapRoom.Key, out MapRoomEntity childEntity))
        //     {
        //         childEntity.Cell = updatedMapRoom.Value;
        //     }
        // }
        //
        // if (updateBase.BuiltPieceEntity != null && updateBase.BuiltPieceEntity is GlobalRootEntity builtPieceEntity)
        // {
        //     worldEntityManager.AddOrUpdateGlobalRootEntity(builtPieceEntity);
        // }
        //
        // if (updateBase.ChildrenTransfer.Item1 != null && updateBase.ChildrenTransfer.Item2 != null)
        // {
        //     // NB: we don't want certain entities to be transferred (e.g. planters)
        //     entityRegistry.TransferChildren(updateBase.ChildrenTransfer.Item1, updateBase.ChildrenTransfer.Item2, entity => entity is not PlanterEntity);
        // }
        //
        // // After transferring required children, we need to clean the waterparks that were potentially removed when being merged
        // List<NitroxId> removedChildIds = buildEntity.ChildEntities.OfType<InteriorPieceEntity>()
        //     .Where(entity => entity.IsWaterPark).Select(childEntity => childEntity.Id)
        //     .Except(updateBase.UpdatedChildren.Keys).ToList();
        //
        // foreach (NitroxId removedChildId in removedChildIds)
        // {
        //     if (entityRegistry.GetEntityById(removedChildId).HasValue)
        //     {
        //         worldEntityManager.RemoveGlobalRootEntity(removedChildId);
        //     }
        // }
        // buildEntity.OperationId++;
        // operationId = buildEntity.OperationId;
        // return true;
    }

    public bool ReplaceBaseByGhost(BaseDeconstructed baseDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(baseDeconstructed.FormerBaseId, out BuildEntity _))
        {
            logger.ZLogError($"Trying to replace a non-registered build (BaseId: {baseDeconstructed.FormerBaseId})");
            return false;
        }

        worldEntityManager.RemoveGlobalRootEntity(baseDeconstructed.FormerBaseId);
        worldEntityManager.AddOrUpdateGlobalRootEntity(baseDeconstructed.ReplacerGhost);
        return true;
    }

    public async Task<(Entity RemovedEntity, int OperationId)> ReplacePieceByGhost(ConnectedPlayerDto player, PieceDeconstructed pieceDeconstructed)
    {
        if (!entityRegistry.TryGetEntityById(pieceDeconstructed.BaseId, out BuildEntity buildEntity))
        {
            logger.ZLogError($"Trying to replace a non-registered build (BaseId: {pieceDeconstructed.BaseId})");
            return (null, -1);
        }
        if (entityRegistry.TryGetEntityById(pieceDeconstructed.PieceId, out GhostEntity _))
        {
            logger.ZLogError($"Trying to add a ghost to a building but another ghost child with the same id already exists (GhostId: {pieceDeconstructed.PieceId})");
            return (null, -1);
        }

        int deltaOperations = buildEntity.OperationId + 1 - pieceDeconstructed.OperationId;
        if (deltaOperations != 0 && configProvider.Value.SafeBuilding)
        {
            logger.ZLogWarning($"Ignoring a {nameof(PieceDeconstructed):@TypeName} packet from [{player.Name:@PlayerName}] which is {Math.Abs(deltaOperations) + (deltaOperations > 0 ? " operations ahead" : " operations late"):@Operations}");
            await NotifyPlayerDesync(player.SessionId);
            return (null, -1);
        }

        Entity removedEntity = worldEntityManager.RemoveGlobalRootEntity(pieceDeconstructed.PieceId).Value;
        GhostEntity ghostEntity = pieceDeconstructed.ReplacerGhost;
        worldEntityManager.AddOrUpdateGlobalRootEntity(ghostEntity);
        buildEntity.BaseData = pieceDeconstructed.BaseData;
        buildEntity.OperationId++;
        return (removedEntity, buildEntity.OperationId);
    }

    public bool CreateWaterParkPiece(WaterParkDeconstructed waterParkDeconstructed, Entity removedEntity)
    {
        if (!entityRegistry.TryGetEntityById(waterParkDeconstructed.BaseId, out Entity entity) || entity is not BuildEntity buildEntity)
        {
            logger.ZLogError($"Trying to create a WaterPark piece in a non-registered build ({waterParkDeconstructed.BaseId})");
            return false;
        }
        if (buildEntity.ChildEntities.Any(childEntity => childEntity.Id.Equals(waterParkDeconstructed.NewWaterPark.Id)))
        {
            logger.ZLogError($"Trying to create a WaterPark piece with an already registered id ({waterParkDeconstructed.NewWaterPark.Id})");
            return false;
        }
        InteriorPieceEntity newPiece = waterParkDeconstructed.NewWaterPark;
        worldEntityManager.AddOrUpdateGlobalRootEntity(newPiece);

        foreach (NitroxId childId in waterParkDeconstructed.MovedChildrenIds)
        {
            entityRegistry.ReparentEntity(childId, newPiece);
        }

        if (removedEntity != null && waterParkDeconstructed.Transfer)
        {
            entityRegistry.TransferChildren(removedEntity, newPiece, e => e is not PlanterEntity);
        }
        return true;
    }

    private async Task NotifyPlayerDesync(SessionId playerSessionId)
    {
        Dictionary<NitroxId, int> operations = GetEntitiesOperations(worldEntityManager.GetGlobalRootEntities(true));
        await packetSender.SendPacket(new BuildingDesyncWarning(operations), playerSessionId);
    }

    public static Dictionary<NitroxId, int> GetEntitiesOperations(List<GlobalRootEntity> entities)
    {
        return entities.OfType<BuildEntity>().ToDictionary(entity => entity.Id, entity => entity.OperationId);
    }
}
