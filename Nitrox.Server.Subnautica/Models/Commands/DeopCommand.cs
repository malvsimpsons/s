using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class DeopCommand : ICommandHandler<NitroxServer.Player>
{
    [Description("Removes admin rights from user")]
    public Task Execute(ICommandContext context, [Description("Username to remove admin rights from")] NitroxServer.Player targetPlayer)
    {
        targetPlayer.Permissions = Perms.PLAYER;

        // Need to notify him so that he no longer shows admin stuff on client (which would in any way stop working)
        targetPlayer.SendPacket(new PermsChanged(targetPlayer.Permissions));
        context.Message(targetPlayer.Id, $"You were demoted to {targetPlayer.Permissions}");
        context.Reply($"Updated {targetPlayer.Name}'s permissions to {targetPlayer.Permissions}");
        return Task.CompletedTask;
    }
}
