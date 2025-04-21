using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class CellVisibilityChangedProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager, PlayerService playerService) : IAuthPacketProcessor<CellVisibilityChanged>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, CellVisibilityChanged packet)
    {
        // TODO: Use database services to change visible cells.
        // sender.AddCells(packet.Added);
        // sender.RemoveCells(packet.Removed);

        List<Entity> totalEntities = [];
        List<SimulatedEntity> totalSimulationChanges = [];

        foreach (AbsoluteEntityCell addedCell in packet.Added)
        {
            worldEntityManager.LoadUnspawnedEntities(addedCell.BatchId, false);

            // TODO: Use database services
            // totalSimulationChanges.AddRange(entitySimulation.GetSimulationChangesForCell(sender, addedCell));
            totalEntities.AddRange(worldEntityManager.GetEntities(addedCell));
        }

        foreach (AbsoluteEntityCell removedCell in packet.Removed)
        {
            // TODO: Use database services
            // entitySimulation.FillWithRemovedCells(sender, removedCell, totalSimulationChanges);
        }

        // Simulation update must be broadcasted before the entities are spawned
        if (totalSimulationChanges.Count > 0)
        {
            entitySimulation.BroadcastSimulationChanges(new(totalSimulationChanges));
        }

        if (totalEntities.Count > 0)
        {
            SpawnEntities batchEntities = new(totalEntities);
            context.ReplyToSender(batchEntities);
        }
    }
}
