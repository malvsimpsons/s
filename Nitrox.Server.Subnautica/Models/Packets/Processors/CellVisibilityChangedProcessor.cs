using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class CellVisibilityChangedProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : AuthenticatedPacketProcessor<CellVisibilityChanged>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public override void Process(CellVisibilityChanged packet, NitroxServer.Player player)
    {
        player.AddCells(packet.Added);
        player.RemoveCells(packet.Removed);

        List<Entity> totalEntities = [];
        List<SimulatedEntity> totalSimulationChanges = [];

        foreach (AbsoluteEntityCell addedCell in packet.Added)
        {
            worldEntityManager.LoadUnspawnedEntities(addedCell.BatchId, false);

            totalSimulationChanges.AddRange(entitySimulation.GetSimulationChangesForCell(player, addedCell));
            totalEntities.AddRange(worldEntityManager.GetEntities(addedCell));
        }

        foreach (AbsoluteEntityCell removedCell in packet.Removed)
        {
            entitySimulation.FillWithRemovedCells(player, removedCell, totalSimulationChanges);
        }

        // Simulation update must be broadcasted before the entities are spawned
        if (totalSimulationChanges.Count > 0)
        {
            entitySimulation.BroadcastSimulationChanges(new(totalSimulationChanges));
        }

        if (totalEntities.Count > 0)
        {
            SpawnEntities batchEntities = new(totalEntities);
            player.SendPacket(batchEntities);
        }
    }
}
