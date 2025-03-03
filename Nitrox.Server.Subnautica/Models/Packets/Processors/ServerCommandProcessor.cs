using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ServerCommandProcessor(CommandService commandService, PlayerService playerService, ILogger<ServerCommandProcessor> logger) : AuthenticatedPacketProcessor<ServerCommand>
{
    public override void Process(ServerCommand packet, NitroxServer.Player player)
    {
        logger.LogInformation("{PlayerName} issued command: /{Command}", player.Name, packet.Cmd);
        commandService.ExecuteCommand(packet.Cmd, new PlayerToServerCommandContext(playerService, player));
    }
}
