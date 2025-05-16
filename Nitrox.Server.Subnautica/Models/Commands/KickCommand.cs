using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Administration;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class KickCommand(IKickPlayer playerKicker) : ICommandHandler<ConnectedPlayerDto, string>
{
    private readonly IKickPlayer playerKicker = playerKicker;

    [Description("Kicks a player from the server")]
    public async Task Execute(ICommandContext context, ConnectedPlayerDto playerToKick, string reason = "")
    {
        reason ??= "";
        if (context.OriginId == playerToKick.Id)
        {
            await context.ReplyAsync("You can't kick yourself");
            return;
        }

        switch (context.Origin)
        {
            case CommandOrigin.PLAYER when playerToKick.Permissions >= context.Permissions:
                await context.ReplyAsync($"You're not allowed to kick {playerToKick.Name}");
                break;
            case CommandOrigin.PLAYER:
            case CommandOrigin.SERVER:
                if (!await playerKicker.KickPlayer(playerToKick.SessionId, reason))
                {
                    await context.ReplyAsync($"Failed to kick '{playerToKick.Name}'");
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context.Origin), "Command does not support the issuer origin");
        }
    }
}
