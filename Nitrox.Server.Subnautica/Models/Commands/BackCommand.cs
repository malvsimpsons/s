using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal class BackCommand(PlayerService playerService) : ICommandHandler
{
    [Description("Teleports you back on your last location")]
    public Task Execute(ICommandContext context)
    {
        switch (context)
        {
            case PlayerToServerCommandContext playerContext:
                ConnectedPlayerDto player = playerContext.Player;
                // TODO: USE DATABASE
                // if (player.LastStoredPosition == null)
                // {
                //     context.Reply("No previous location...");
                //     return Task.CompletedTask;
                // }
                // playerService.Teleport(player, player.LastStoredPosition.Value);
                // context.Reply($"Teleported back to {player.LastStoredPosition.Value}");
                break;
        }
        return Task.CompletedTask;
    }
}
