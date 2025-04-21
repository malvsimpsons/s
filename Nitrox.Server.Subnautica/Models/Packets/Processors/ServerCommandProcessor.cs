using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ServerCommandProcessor(CommandService commandService, PlayerService playerService, ILogger<ServerCommandProcessor> logger) : IAuthPacketProcessor<ServerCommand>
{
    public async Task Process(AuthProcessorContext context, ServerCommand packet)
    {
        // TODO: USE DATABASE
        // logger.LogInformation("{PlayerName} issued command: /{Command}", player.Name, packet.Cmd);
        // commandService.ExecuteCommand(packet.Cmd, new PlayerToServerCommandContext(playerService, player));
    }
}
