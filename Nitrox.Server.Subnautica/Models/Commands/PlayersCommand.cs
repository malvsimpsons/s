using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Dto;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("list", "p")]
internal class PlayersCommand(IOptions<SubnauticaServerOptions> serverOptionsProvider, PlayerService playerService) : ICommandHandler
{
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<SubnauticaServerOptions> serverOptionsProvider = serverOptionsProvider;

    [Description("Shows who's online")]
    public async Task Execute(ICommandContext context)
    {
        SubnauticaServerOptions options = serverOptionsProvider.Value;

        ConnectedPlayerDto[] players = await playerService.GetConnectedPlayersAsync();
        string playerNamesWithIds = string.Join(", ", players.OrderBy(p => p.Name).Select(p => $"{p.Name} (#{p.Id})"));
        context.Reply($"List of players ({players.Length}/{options.MaxConnections}):{Environment.NewLine}{playerNamesWithIds}");
    }
}
