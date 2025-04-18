using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PdaScanFinishedPacketProcessor(PlayerService playerService, WorldEntityManager worldEntityManager) : AuthenticatedPacketProcessor<PdaScanFinished>
{
    private readonly PlayerService playerService = playerService;
    // TODO: USE DATABASE
    // private readonly PdaStateData pdaStateData = pdaStateData;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public override void Process(PdaScanFinished packet, NitroxServer.Player player)
    {
        if (!packet.WasAlreadyResearched)
        {
            // TODO: USE DATABASE
            // pdaStateData.UpdateEntryUnlockedProgress(packet.TechType, packet.UnlockedAmount, packet.FullyResearched);
        }
        playerService.SendPacketToOtherPlayers(packet, player);

        if (packet.Id != null)
        {
            if (packet.Destroy)
            {
                worldEntityManager.TryDestroyEntity(packet.Id, out _);
            }
            else
            {
                // TODO: USE DATABASE
                // pdaStateData.AddScannerFragment(packet.Id);
            }
        }
    }
}
