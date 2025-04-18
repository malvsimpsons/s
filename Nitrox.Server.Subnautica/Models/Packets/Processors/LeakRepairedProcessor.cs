using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class LeakRepairedProcessor(WorldEntityManager worldEntityManager, PlayerService playerService) : AuthenticatedPacketProcessor<LeakRepaired>
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly PlayerService playerService = playerService;

    public override void Process(LeakRepaired packet, NitroxServer.Player player)
    {
        if (worldEntityManager.TryDestroyEntity(packet.LeakId, out _))
        {
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
