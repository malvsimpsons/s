using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PinnedRecipeMovedProcessor : AuthenticatedPacketProcessor<PinnedRecipeMoved>
{
    public override void Process(PinnedRecipeMoved packet, NitroxServer.Player player)
    {
        player.PinnedRecipePreferences.Clear();
        player.PinnedRecipePreferences.AddRange(packet.RecipePins);
    }
}
