using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlayerQuickSlotsBindingChangedProcessor : AuthenticatedPacketProcessor<PlayerQuickSlotsBindingChanged>
{
    public override void Process(PlayerQuickSlotsBindingChanged packet, NitroxServer.Player player)
    {
        player.QuickSlotsBindingIds = packet.SlotItemIds;
    }
}