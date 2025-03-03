using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class KnownTechEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaStateData) : AuthenticatedPacketProcessor<KnownTechEntryAdd>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly PDAStateData pdaStateData = pdaStateData;

    public override void Process(KnownTechEntryAdd packet, NitroxServer.Player player)
    {
        switch (packet.Category)
        {
            case KnownTechEntryAdd.EntryCategory.KNOWN:
                pdaStateData.AddKnownTechType(packet.TechType, packet.PartialTechTypesToRemove);
                break;
            case KnownTechEntryAdd.EntryCategory.ANALYZED:
                pdaStateData.AddAnalyzedTechType(packet.TechType);
                break;
        }

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}