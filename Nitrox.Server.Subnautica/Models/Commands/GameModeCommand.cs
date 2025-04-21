using System;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;
using NitroxModel.Server;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class GameModeCommand(PlayerService playerService) : ICommandHandler<SubnauticaGameMode, ConnectedPlayerDto>
{
    private readonly PlayerService playerService = playerService;

    [Description("Changes a player's gamemode")]
    public async Task Execute(ICommandContext context, SubnauticaGameMode gameMode, ConnectedPlayerDto targetPlayer = null)
    {
        switch (context.Origin)
        {
            case CommandOrigin.SERVER when targetPlayer == null:
                context.Reply("Console can't use the gamemode command without providing a player name.");
                return;
            case CommandOrigin.PLAYER when context is PlayerToServerCommandContext playerContext:
                // The target player if not set, is the player who sent the command
                targetPlayer ??= playerContext.Player;
                goto default;
            default:
                if (targetPlayer == null)
                {
                    throw new ArgumentException("Target player must not be null");
                }
                // TODO: USE DATABASE
                // targetPlayer.GameMode = gameMode;
                playerService.SendPacketToAllPlayers(GameModeChanged.ForPlayer(targetPlayer.Id, gameMode));
                await context.MessageAsync(targetPlayer.Id, $"GameMode changed to {gameMode}");
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
    }
}
