#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.ADMIN)]
internal class DebugStartMapCommand(IOptions<Configuration.SubnauticaServerOptions> optionsProvider, RandomStartResource randomStart) : ICommandHandler
{
    private readonly RandomStartResource randomStart = randomStart;
    private readonly IOptions<Configuration.SubnauticaServerOptions> optionsProvider = optionsProvider;

    [Description("Spawns blocks at spawn positions")]
    public Task Execute(ICommandContext context)
    {
        List<NitroxVector3> randomStartPositions = randomStart.RandomStartGenerator.GenerateRandomStartPositions(optionsProvider.Value.Seed);

        context.SendToAll(new DebugStartMapPacket(randomStartPositions));
        context.ReplyAsync($"Rendered {randomStartPositions.Count} spawn positions");

        return Task.CompletedTask;
    }
}
#endif
