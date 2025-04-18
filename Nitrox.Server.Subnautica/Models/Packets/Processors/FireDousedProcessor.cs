using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class FireDousedProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<FireDoused>
{
    private readonly PlayerService playerService = playerService;

    public override void Process(FireDoused packet, NitroxServer.Player simulatingPlayer)
    {
        playerService.SendPacketToOtherPlayers(packet, simulatingPlayer);
    }
}
