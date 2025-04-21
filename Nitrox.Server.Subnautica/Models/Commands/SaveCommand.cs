using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

// TODO: This command might not be necessary when using SQLite database as it'll save itself.
[RequiresPermission(Perms.MODERATOR)]
internal class SaveCommand(PersistenceService persistenceService) : ICommandHandler
{
    private readonly PersistenceService persistenceService = persistenceService;

    [Description("Saves the world")]
    public Task Execute(ICommandContext context)
    {
        context.MessageAll("World is saving...");

        // TODO: Run save action on server.
        // persistenceService.SaveAsync() (allow async command execute)
        // NitroxServer.Server.Instance.Save();

        return Task.CompletedTask;
    }
}
