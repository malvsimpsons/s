using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class DeopCommand : ICommandHandler<ConnectedPlayerDto>
{
    [Description("Removes admin rights from user")]
    public async Task Execute(ICommandContext context, [Description("Username to remove admin rights from")] ConnectedPlayerDto targetPlayer)
    {
        // TODO: USE DATABASE
        // targetPlayer.Permissions = Perms.PLAYER;
        // Need to notify him so that he no longer shows admin stuff on client (which would in any way stop working)
        // targetPlayer.SendPacket(new PermsChanged(targetPlayer.Permissions));
        await context.MessageAsync(targetPlayer.Id, $"You were demoted to {targetPlayer.Permissions}");
        context.Reply($"Updated {targetPlayer.Name}'s permissions to {targetPlayer.Permissions}");
    }
}
