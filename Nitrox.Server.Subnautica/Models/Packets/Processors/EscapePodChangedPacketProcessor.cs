using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EscapePodChangedPacketProcessor(PlayerService playerService, ILogger<EscapePodChangedPacketProcessor> logger) : AuthenticatedPacketProcessor<EscapePodChanged>
{
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<EscapePodChangedPacketProcessor> logger = logger;

    public override void Process(EscapePodChanged packet, NitroxServer.Player player)
    {
        logger.LogDebug("Processing packet {Packet}", packet);
        player.SubRootId = packet.EscapePodId;
        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
