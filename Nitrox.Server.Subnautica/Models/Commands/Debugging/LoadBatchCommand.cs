#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.ADMIN)]
internal sealed class LoadBatchCommand(BatchEntitySpawnerService batchEntitySpawnerService) : ICommandHandler<int, int, int>
{
    private readonly BatchEntitySpawnerService batchEntitySpawnerService = batchEntitySpawnerService;

    [Description("Loads entities at x y z")]
    public Task Execute(ICommandContext context, int xCoordinate, int yCoordinate, int zCoordinate)
    {
        NitroxInt3 batchId = new(xCoordinate, yCoordinate, zCoordinate);
        List<Entity> entities = batchEntitySpawnerService.LoadUnspawnedEntities(batchId);

        context.Reply($"Loaded {entities.Count} entities from batch {batchId}");

        return Task.CompletedTask;
    }
}
#endif
