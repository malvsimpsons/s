using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class PromoteCommand(PlayerService playerService) : ICommandHandler<ConnectedPlayerDto, Perms>
{
    private readonly PlayerService playerService = playerService;

    [Description("Sets specific permissions to a user")]
    public async Task Execute(ICommandContext context, [Description("The username to change the permissions of")] ConnectedPlayerDto targetPlayer, [Description("Permission level")] Perms permissions)
    {
        switch (context)
        {
            case not null when context.OriginId == targetPlayer.Id:
                context.Reply("You can't promote yourself");
                break;
            case { Permissions: var originPerms } when originPerms < permissions:
                context.Reply($"You're not allowed to update {targetPlayer.Name}'s permissions");
                break;
            case not null:
                // TODO: USE DATAGBASE
                // targetPlayer.Permissions = permissions; // Allows a bounded permission hierarchy
                playerService.SendPacket(new PermsChanged(targetPlayer.Permissions), targetPlayer.Id);
                context.Reply($"Updated {targetPlayer.Name}'s permissions to {permissions}");
                await context.MessageAsync(targetPlayer.Id, $"You've been promoted to {permissions}");
                break;
        }
    }
}
