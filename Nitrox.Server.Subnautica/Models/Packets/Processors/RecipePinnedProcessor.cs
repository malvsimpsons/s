using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PinnedRecipeProcessor : AuthenticatedPacketProcessor<RecipePinned>
{
    public override void Process(RecipePinned packet, NitroxServer.Player player)
    {
        if (packet.Pinned)
        {
            player.PinnedRecipePreferences.Add(packet.TechType);
        }
        else
        {
            player.PinnedRecipePreferences.Remove(packet.TechType);
        }        
    }
}
