using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlayerHeldItemChangedProcessor(PlayerManager playerManager) : AuthenticatedPacketProcessor<PlayerHeldItemChanged>
{
    private readonly PlayerManager playerManager = playerManager;

    public override void Process(PlayerHeldItemChanged packet, NitroxServer.Player player)
    {
        if (packet.IsFirstTime != null && !player.UsedItems.Contains(packet.IsFirstTime))
        {
            player.UsedItems.Add(packet.IsFirstTime);
        }

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}