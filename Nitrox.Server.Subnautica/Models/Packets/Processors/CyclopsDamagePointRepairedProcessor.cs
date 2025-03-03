using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel_Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class CyclopsDamagePointRepairedProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<CyclopsDamagePointRepaired>
{
    private readonly PlayerService playerService = playerService;

    public override void Process(CyclopsDamagePointRepaired packet, NitroxServer.Player simulatingPlayer)
    {
        playerService.SendPacketToOtherPlayers(packet, simulatingPlayer);
    }
}
