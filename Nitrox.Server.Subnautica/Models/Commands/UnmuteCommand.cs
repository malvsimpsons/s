using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal class UnmuteCommand(PlayerService playerService) : ICommandHandler<NitroxServer.Player>
{
    private readonly PlayerService playerService = playerService;

    [Description("Removes a mute from a player")]
    public async Task Execute(ICommandContext context, [Description("Player to unmute")] NitroxServer.Player targetPlayer)
    {
        switch (context)
        {
            case not null when context.OriginId == targetPlayer.Id:
                context.Reply("You can't unmute yourself");
                break;
            case PlayerToServerCommandContext when targetPlayer.Permissions >= context.Permissions:
                context.Reply($"You're not allowed to unmute {targetPlayer.Name}");
                break;
            case not null when !targetPlayer.PlayerContext.IsMuted:
                context.Reply($"{targetPlayer.Name} is already unmuted");
                targetPlayer.SendPacket(new MutePlayer(targetPlayer.Id, targetPlayer.PlayerContext.IsMuted));
                break;
            case not null:
                targetPlayer.PlayerContext.IsMuted = false;
                playerService.SendPacketToAllPlayers(new MutePlayer(targetPlayer.Id, targetPlayer.PlayerContext.IsMuted));
                await context.MessageAsync(targetPlayer.Id, "You're no longer muted");
                context.Reply($"Unmuted {targetPlayer.Name}");
                break;
        }
    }
}
