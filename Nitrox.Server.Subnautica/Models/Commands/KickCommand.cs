using System;
using System.Collections.Generic;
using System.ComponentModel;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.MODERATOR)]
internal sealed class KickCommand(PlayerService playerService, EntitySimulation entitySimulation) : ICommandHandler<NitroxServer.Player, string>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly PlayerService playerService = playerService;

    [Description("Kicks a player from the server")]
    public Task Execute(ICommandContext context, NitroxServer.Player playerToKick, string reason = "")
    {
        reason ??= "";
        if (context.OriginId == playerToKick.Id)
        {
            context.Reply("You can't kick yourself");
            return Task.CompletedTask;
        }

        switch (context.Origin)
        {
            case CommandOrigin.PLAYER when playerToKick.Permissions >= context.Permissions:
                context.Reply($"You're not allowed to kick {playerToKick.Name}");
                break;
            case CommandOrigin.PLAYER:
            case CommandOrigin.SERVER:
                playerToKick.SendPacket(new PlayerKicked(reason));
                playerService.Disconnect(playerToKick.Connection);

                List<SimulatedEntity> revokedEntities = entitySimulation.CalculateSimulationChangesFromPlayerDisconnect(playerToKick);
                if (revokedEntities.Count > 0)
                {
                    SimulationOwnershipChange ownershipChange = new(revokedEntities);
                    playerService.SendPacketToAllPlayers(ownershipChange);
                }

                playerService.SendPacketToOtherPlayers(new Disconnect(playerToKick.Id), playerToKick);
                context.Reply($"The player {playerToKick.Name} has been disconnected");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(context.Origin), "The origin of this command is unsupported");
        }
        return Task.CompletedTask;
    }
}
