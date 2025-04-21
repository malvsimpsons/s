using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class OpCommand : ICommandHandler<NitroxServer.Player>
{
    [Description("Sets a user as admin")]
    public Task Execute(ICommandContext context, [Description("The player to make an admin")] NitroxServer.Player targetPlayer)
    {
        switch (context)
        {
            case not null when targetPlayer.Permissions >= Perms.ADMIN:
                context.Reply($"Player {targetPlayer.Name} already has {Perms.ADMIN} permissions");
                break;
            case not null:
                targetPlayer.Permissions = Perms.ADMIN;
                // We need to notify this player that he can show all the admin-related stuff
                targetPlayer.SendPacket(new PermsChanged(targetPlayer.Permissions));
                context.Message(targetPlayer.Id, $"You were promoted to {targetPlayer.Permissions}");
                context.Reply($"Updated {targetPlayer.Name}'s permissions to {targetPlayer.Permissions}");
                break;
        }

        return Task.CompletedTask;
    }
}
