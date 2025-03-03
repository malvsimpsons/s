using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel_Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class CyclopsFireCreatedProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<CyclopsFireCreated>
{
    private readonly PlayerService playerService = playerService;

    public override void Process(CyclopsFireCreated packet, NitroxServer.Player simulatingPlayer)
    {
        playerService.SendPacketToOtherPlayers(packet, simulatingPlayer);
    }
}
