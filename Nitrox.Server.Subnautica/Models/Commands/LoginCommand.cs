using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresOrigin(CommandOrigin.PLAYER)]
internal class LoginCommand(IOptions<Configuration.SubnauticaServerOptions> optionsProvider) : ICommandHandler<string>
{
    private readonly IOptions<Configuration.SubnauticaServerOptions> optionsProvider = optionsProvider;

    [Description("Log in to server as admin (requires password)")]
    public Task Execute(ICommandContext context, [Description("The admin password for the server")] string adminPassword)
    {
        switch (context)
        {
            case PlayerToServerCommandContext { Player: { Permissions: < Perms.ADMIN } player }:
                if (optionsProvider.Value.AdminPassword == adminPassword)
                {
                    player.Permissions = Perms.ADMIN;
                    context.Reply($"You've been made {nameof(Perms.ADMIN)} on this server!");
                }
                else
                {
                    context.Reply("Incorrect Password");
                }
                break;
            default:
                context.Reply("You already have admin permissions");
                break;
        }

        return Task.CompletedTask;
    }
}
