using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDAEncyclopediaEntryAddProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<PDAEncyclopediaEntryAdd>
{
    private readonly PlayerService playerService = playerService;
    // TODO: USE DATABASE
    // private readonly PdaStateData pdaStateData = pdaStateData;

    public override void Process(PDAEncyclopediaEntryAdd packet, NitroxServer.Player player)
    {
        // TODO: USE DATABASE
        // pdaStateData.AddEncyclopediaEntry(packet.Key);
        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
