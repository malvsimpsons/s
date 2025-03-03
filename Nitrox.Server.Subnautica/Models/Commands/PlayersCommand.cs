using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Commands;

[Alias("list", "p")]
internal class PlayersCommand(IOptions<SubnauticaServerOptions> serverOptionsProvider, PlayerService playerService) : ICommandHandler
{
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<SubnauticaServerOptions> serverOptionsProvider = serverOptionsProvider;

    [Description("Shows who's online")]
    public void Execute(ICommandContext context)
    {
        SubnauticaServerOptions options = serverOptionsProvider.Value;

        IList<string> players = playerService.GetConnectedPlayers().Select(player => player.Name).ToArray();
        context.Reply($"List of players ({players.Count}/{options.MaxConnections}):{Environment.NewLine}{string.Join(", ", players)}");
    }
}
