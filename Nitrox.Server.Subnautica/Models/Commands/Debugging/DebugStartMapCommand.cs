#if DEBUG
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Commands.Debugging;

[RequiresPermission(Perms.ADMIN)]
internal class DebugStartMapCommand(IOptions<Configuration.SubnauticaServerOptions> optionsProvider, RandomStartResource randomStart, PlayerService playerService) : ICommandHandler
{
    private readonly RandomStartResource randomStart = randomStart;
    private readonly IOptions<Configuration.SubnauticaServerOptions> optionsProvider = optionsProvider;
    private readonly PlayerService playerService = playerService;

    [Description("Spawns blocks at spawn positions")]
    public void Execute(ICommandContext context)
    {
        List<NitroxVector3> randomStartPositions = randomStart.RandomStartGenerator.GenerateRandomStartPositions(optionsProvider.Value.Seed);

        playerService.SendPacketToAllPlayers(new DebugStartMapPacket(randomStartPositions));
        context.Reply($"Rendered {randomStartPositions.Count} spawn positions");
    }
}
#endif
