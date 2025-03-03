#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.CONSOLE)]
internal class PlayerCommand(GameLogic.SimulationOwnershipData simulationOwnership, GameLogic.WorldEntityManager entityManager, ILogger<PlayerCommand> logger) : ICommandHandler<NitroxServer.Player>
{
    private readonly GameLogic.SimulationOwnershipData simulationOwnership = simulationOwnership;
    private readonly GameLogic.WorldEntityManager entityManager = entityManager;
    private readonly ILogger<PlayerCommand> logger = logger;


    [Description("Lists all visible cells of a player, their simulated entities per cell and the player's visible out of cell entities")]
    public void Execute(ICommandContext context, [Description("name of the target player")] NitroxServer.Player player)
    {
        List<AbsoluteEntityCell> visibleCells = player.GetVisibleCells();

        logger.LogInformation("{Player}", player);
        logger.LogInformation("Visible cells [{VisibleCellCount}]:", visibleCells.Count);
        foreach (AbsoluteEntityCell visibleCell in visibleCells)
        {
            string simulatedEntities = "";
            foreach (WorldEntity worldEntity in entityManager.GetEntities(visibleCell))
            {
                if (simulationOwnership.TryGetLock(worldEntity.Id, out GameLogic.SimulationOwnershipData.PlayerLock playerLock) &&
                    playerLock.Player.Id == player.Id)
                {
                    simulatedEntities += $"[{worldEntity.Id}; {worldEntity.TechType?.ToString() ?? worldEntity.ClassId}], ";
                }
            }
            logger.LogInformation("{VisibleCell}: {Distance}", visibleCell, NitroxVector3.Distance(visibleCell.Position, player.Position));
            if (simulatedEntities.Length > 0)
            {
                // Get everything but the last ", " of the string
                logger.LogInformation(simulatedEntities[..^2]);
            }
        }
        logger.LogInformation($"\nOut of cell entities:\n{string.Join(", ", player.OutOfCellVisibleEntities)}");
    }
}
#endif
