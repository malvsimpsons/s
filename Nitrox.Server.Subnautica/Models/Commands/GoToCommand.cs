using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

// TODO: Fix this command
[Alias("warp")]
[RequiresOrigin(CommandOrigin.PLAYER)]
[RequiresPermission(Perms.MODERATOR)]
public sealed class GoToCommand : ICommandHandler<float, float, float>, ICommandHandler<string>, ICommandHandler<NitroxServer.Player>, ICommandHandler<NitroxServer.Player, NitroxServer.Player>
{
    [Description("Teleports to a player")]
    public void Execute(ICommandContext context, NitroxServer.Player player) => context.Reply($"Received arg {player} of type {player.GetType().Name}");

    [Description("Teleports to a position")]
    public void Execute(ICommandContext context, float x, float y, float z) => context.Reply($"Teleporting to position {x} {y} {z}...");

    [Description("Teleports to a location given its name")]
    public void Execute(ICommandContext context, string subnauticaLocationName) => context.Reply($"Teleporting to location {subnauticaLocationName}...");

    [Description("Teleports player A to Player B")]
    [RequiresOrigin(CommandOrigin.ANY)]
    public void Execute(ICommandContext context, NitroxServer.Player playerA, NitroxServer.Player playerB) => throw new NotImplementedException();
}
