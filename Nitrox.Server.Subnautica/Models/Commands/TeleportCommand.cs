using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("tp")]
[RequiresPermission(Perms.MODERATOR)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal sealed class TeleportCommand : ICommandHandler<int, int, int>
{
    [Description("Teleports yourself to a specific location")]
    public void Execute(ICommandContext context, [Description("x coordinate")] int x, [Description("y coordinate")] int y, [Description("z coordinate")] int z)
    {
        switch (context)
        {
            case PlayerToServerCommandContext { Player: { } player }:
                NitroxVector3 position = new(x, y, z);
                player.Teleport(position, Optional.Empty);
                context.Reply($"Teleported to {position}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context), "Only players can teleport themselves");
        }
    }
}
