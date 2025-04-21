using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class OpCommand(PlayerService playerService) : ICommandHandler<ConnectedPlayerDto>
{
    private readonly PlayerService playerService = playerService;

    [Description("Sets a user as admin")]
    public async Task Execute(ICommandContext context, [Description("The player to make an admin")] ConnectedPlayerDto targetPlayer)
    {
        switch (context)
        {
            case not null when targetPlayer.Permissions >= Perms.ADMIN:
                context.Reply($"Player {targetPlayer.Name} already has {Perms.ADMIN} permissions");
                break;
            case not null:
                // TODO: USE DATABASE
                // targetPlayer.Permissions = Perms.ADMIN;
                playerService.SendPacket(new PermsChanged(targetPlayer.Permissions), targetPlayer.Id); // Notify this player that they can show all the admin-related stuff
                await context.MessageAsync(targetPlayer.Id, $"You were promoted to {targetPlayer.Permissions}");
                context.Reply($"Updated {targetPlayer.Name}'s permissions to {targetPlayer.Permissions}");
                break;
        }
    }
}
