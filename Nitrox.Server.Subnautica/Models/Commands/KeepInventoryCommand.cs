using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
[RequiresOrigin(CommandOrigin.PLAYER)]
internal class KeepInventoryCommand(PlayerService playerService, IOptions<Configuration.SubnauticaServerOptions> optionsProvider) : ICommandHandler<bool>
{
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<Configuration.SubnauticaServerOptions> serverConfig = optionsProvider;

    [Description("Sets \"keep inventory\" setting to on/off. If \"on\", players won't lose items when they die.")]
    public Task Execute(ICommandContext context, [Description("The true/false state to set keep inventory on death to")] bool newKeepInventoryState)
    {
        if (serverConfig.Value.KeepInventoryOnDeath != newKeepInventoryState)
        {
            serverConfig.Value.KeepInventoryOnDeath = newKeepInventoryState;
            playerService.SendPacketToAllPlayers(new KeepInventoryChanged(newKeepInventoryState));
            context.MessageAll($"KeepInventoryOnDeath changed to \"{newKeepInventoryState}\" by {context.OriginName}");
        }
        else
        {
            context.Reply($"KeepInventoryOnDeath already set to {newKeepInventoryState}");
        }
        return Task.CompletedTask;
    }
}
