using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerHeldItemChangedProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<PlayerHeldItemChanged>
{
    private readonly PlayerService playerService = playerService;

    public override void Process(PlayerHeldItemChanged packet, NitroxServer.Player player)
    {
        if (packet.IsFirstTime != null && !player.UsedItems.Contains(packet.IsFirstTime))
        {
            player.UsedItems.Add(packet.IsFirstTime);
        }

        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
