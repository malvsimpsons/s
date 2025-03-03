using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class LeakRepairedProcessor(WorldEntityManager worldEntityManager, PlayerManager playerManager) : AuthenticatedPacketProcessor<LeakRepaired>
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly PlayerManager playerManager = playerManager;

    public override void Process(LeakRepaired packet, NitroxServer.Player player)
    {
        if (worldEntityManager.TryDestroyEntity(packet.LeakId, out _))
        {
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
