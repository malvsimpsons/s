using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerUnseeOutOfCellEntityProcessor(SimulationOwnershipData simulationOwnershipData, PlayerService playerService, EntitySimulation entitySimulation, EntityRegistry entityRegistry)
    : AuthenticatedPacketProcessor<PlayerUnseeOutOfCellEntity>
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly PlayerService playerManager = playerService;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(PlayerUnseeOutOfCellEntity packet, NitroxServer.Player player)
    {
        // Most of this packet's utility is in the below Remove
        if (!player.OutOfCellVisibleEntities.Remove(packet.EntityId) ||
            !entityRegistry.TryGetEntityById(packet.EntityId, out Entity entity))
        {
            return;
        }

        // If player can still see the entity even after removing it from the OutOfCellVisibleEntities, then we don't need to change anything
        if (player.CanSee(entity))
        {
            return;
        }

        // If the player doesn't own the entity's simulation then we don't need to do anything
        if (!simulationOwnershipData.RevokeIfOwner(packet.EntityId, player))
        {
            return;
        }

        List<NitroxServer.Player> otherPlayers = playerManager.GetConnectedPlayersExcept(player);
        if (entitySimulation.TryAssignEntityToPlayers(otherPlayers, entity, out SimulatedEntity simulatedEntity))
        {
            entitySimulation.BroadcastSimulationChanges([simulatedEntity]);
        }
        else
        {
            // No player has taken simulation on the entity
            playerManager.SendPacketToAllPlayers(new DropSimulationOwnership(packet.EntityId));
        }
    }
}
