using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal class MuteCommand(PlayerService playerService) : ICommandHandler<NitroxServer.Player>
{
    private readonly PlayerService playerService = playerService;

    [Description("Prevents a user from chatting")]
    public Task Execute(ICommandContext context, [Description("Player to mute")] NitroxServer.Player targetPlayer)
    {
        switch (context)
        {
            case not null when context.OriginId == targetPlayer.Id:
                context.Reply("You can't mute yourself");
                break;
            case { Permissions: var contextPerms } when contextPerms < targetPlayer.Permissions:
                context.Reply($"You're not allowed to mute {targetPlayer.Name}");
                break;
            case not null when targetPlayer is { PlayerContext.IsMuted: true }:
                context.Reply($"{targetPlayer.Name} is already muted");
                targetPlayer.SendPacket(new MutePlayer(targetPlayer.Id, targetPlayer.PlayerContext.IsMuted)); // TODO: Is sending this packet necessary?
                break;
            case not null:
                targetPlayer.PlayerContext.IsMuted = true;
                playerService.SendPacketToAllPlayers(new MutePlayer(targetPlayer.Id, targetPlayer.PlayerContext.IsMuted));
                context.Message(targetPlayer.Id, "You're now muted");
                context.Reply($"Muted {targetPlayer.Name}");
                break;
            default:
                throw new ArgumentNullException(nameof(context), "Expected command context to not be null");
        }

        return Task.CompletedTask;
    }
}
