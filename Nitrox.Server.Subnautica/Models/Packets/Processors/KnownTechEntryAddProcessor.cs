using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class KnownTechEntryAddProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<KnownTechEntryAdd>
{
    private readonly PlayerService playerService = playerService;
    // TODO: USE DATABASE
    // private readonly PdaStateData pdaStateData = pdaStateData;

    public override void Process(KnownTechEntryAdd packet, NitroxServer.Player player)
    {
        // TODO: USE DATABASE
        // switch (packet.Category)
        // {
        //     case KnownTechEntryAdd.EntryCategory.KNOWN:
        //         pdaStateData.AddKnownTechType(packet.TechType, packet.PartialTechTypesToRemove);
        //         break;
        //     case KnownTechEntryAdd.EntryCategory.ANALYZED:
        //         pdaStateData.AddAnalyzedTechType(packet.TechType);
        //         break;
        // }

        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
