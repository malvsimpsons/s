using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerHeldItemChangedProcessor(PlayerService playerService) : IAuthPacketProcessor<PlayerHeldItemChanged>
{
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, PlayerHeldItemChanged packet)
    {
        // TODO: USE DATABASE
        // if (packet.IsFirstTime != null && !player.UsedItems.Contains(packet.IsFirstTime))
        // {
        //     player.UsedItems.Add(packet.IsFirstTime);
        // }
        //
        // playerService.SendPacketToOtherPlayers(packet, player);
    }
}
