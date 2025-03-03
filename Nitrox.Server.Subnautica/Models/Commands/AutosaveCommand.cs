using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;

namespace Nitrox.Server.Subnautica.Models.Commands;

[RequiresPermission(Perms.ADMIN)]
internal class AutoSaveCommand(IOptions<SubnauticaServerConfig> serverOptionsProvider) : ICommandHandler<bool>
{
    private readonly IOptions<SubnauticaServerConfig> serverOptionsProvider = serverOptionsProvider;

    [Description("Whether autosave should be on or off")]
    public void Execute(ICommandContext context, bool toggle)
    {
        if (toggle)
        {
            serverOptionsProvider.Value.DisableAutoSave = false;
            context.Reply("Enabled periodical saving");
        }
        else
        {
            serverOptionsProvider.Value.DisableAutoSave = true;
            context.Reply("Disabled periodical saving");
        }
    }
}
