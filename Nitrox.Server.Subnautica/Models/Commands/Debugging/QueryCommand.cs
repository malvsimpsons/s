#if DEBUG
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.SUPERADMIN)]
internal class QueryCommand(EntityRegistry entityRegistry, SimulationOwnershipData simulationOwnershipData) : ICommandHandler<NitroxId>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    [Description("Query the entity associated with the given NitroxId")]
    public Task Execute(ICommandContext context, [Description("NitroxId of an entity")] NitroxId entityId)
    {
        if (!entityRegistry.TryGetEntityById(entityId, out Entity entity))
        {
            context.ReplyAsync($"Entity with id {entityId} not found");
            return Task.CompletedTask;
        }

        context.ReplyAsync(entity.ToString());
        if (entity is WorldEntity worldEntity)
        {
            context.ReplyAsync(worldEntity.AbsoluteEntityCell.ToString());
        }
        if (simulationOwnershipData.TryGetLock(entityId, out SimulationOwnershipData.PlayerLock playerLock))
        {
            context.ReplyAsync($"Lock owner player id: {playerLock.PlayerId}");
        }
        else
        {
            context.ReplyAsync("Not locked");
        }

        return Task.CompletedTask;
    }
}
#endif
