using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class GameModeCommand(PlayerService playerService) : ICommandHandler<NitroxGameMode, NitroxServer.Player>
{
    private readonly PlayerService playerService = playerService;

    [Description("Changes a player's gamemode")]
    public Task Execute(ICommandContext context, NitroxGameMode gameMode, NitroxServer.Player targetPlayer = null)
    {
        switch (context.Origin)
        {
            case CommandOrigin.SERVER when targetPlayer == null:
                context.Reply("Console can't use the gamemode command without providing a player name.");
                return Task.CompletedTask;
            case CommandOrigin.PLAYER when context is PlayerToServerCommandContext playerContext:
                // The target player if not set, is the player who sent the command
                targetPlayer ??= playerContext.Player;
                goto default;
            default:
                if (targetPlayer == null)
                {
                    throw new ArgumentException("Target player must not be null");
                }
                targetPlayer.GameMode = gameMode;
                playerService.SendPacketToAllPlayers(GameModeChanged.ForPlayer(targetPlayer.Id, gameMode));
                context.Message(targetPlayer.Id, $"GameMode changed to {gameMode}");
                if (context.Origin == CommandOrigin.SERVER)
                {
                    context.Reply($"Changed {targetPlayer.Name} [{targetPlayer.Id}]'s gamemode to {gameMode}");
                }
                else if (targetPlayer.Id != context.OriginId)
                {
                    context.Reply($"GameMode of {targetPlayer.Name} changed to {gameMode}");
                }
                break;
        }
        return Task.CompletedTask;
    }
}
