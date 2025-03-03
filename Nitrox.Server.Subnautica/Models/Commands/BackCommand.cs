using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal class BackCommand : ICommandHandler
{
    [Description("Teleports you back on your last location")]
    public void Execute(ICommandContext context)
    {
        switch (context)
        {
            case PlayerToServerCommandContext playerContext:
                NitroxServer.Player player = playerContext.Player;
                if (player.LastStoredPosition == null)
                {
                    context.Reply("No previous location...");
                    return;
                }

                player.Teleport(player.LastStoredPosition.Value, player.LastStoredSubRootID);
                context.Reply($"Teleported back to {player.LastStoredPosition.Value}");
                break;
        }
    }
}
