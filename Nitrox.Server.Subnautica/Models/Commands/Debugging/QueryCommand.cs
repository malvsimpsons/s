#if DEBUG
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.CONSOLE)]
internal class QueryCommand(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData, ILogger<QueryCommand> logger) : ICommandHandler<NitroxId>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly ILogger<QueryCommand> logger = logger;

    [Description("Query the entity associated with the given NitroxId")]
    public void Execute(ICommandContext context, [Description("NitroxId of an entity")] NitroxId entityId)
    {
        if (!entityRegistry.TryGetEntityById(entityId, out Entity entity))
        {
            context.Reply($"Entity with id {entityId} not found");
            return;
        }

        context.Reply(entity.ToString());
        if (entity is WorldEntity worldEntity)
        {
            context.Reply(worldEntity.AbsoluteEntityCell.ToString());
        }
        if (simulationOwnershipData.TryGetLock(entityId, out SimulationOwnershipData.PlayerLock playerLock))
        {
            context.Reply($"Lock owner: {playerLock.Player.Name}");
        }
        else
        {
            context.Reply("Not locked");
        }
    }
}
#endif
