using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

internal class PromoteCommand : ICommandHandler<NitroxServer.Player, Perms>
{
    [Description("Sets specific permissions to a user")]
    public Task Execute(ICommandContext context, [Description("The username to change the permissions of")] NitroxServer.Player targetPlayer, [Description("Permission level")] Perms permissions)
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
                //Allows a bounded permission hierarchy
                targetPlayer.Permissions = permissions;

                targetPlayer.SendPacket(new PermsChanged(targetPlayer.Permissions));
                context.Reply($"Updated {targetPlayer.Name}'s permissions to {permissions}");
                context.Message(targetPlayer.Id, $"You've been promoted to {permissions}");
                break;
        }

        return Task.CompletedTask;
    }
}
