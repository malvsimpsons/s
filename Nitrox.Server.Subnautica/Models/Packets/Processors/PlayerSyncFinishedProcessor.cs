using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerSyncFinishedProcessor(PlayerService playerManager, HibernationService hibernationService) : AuthenticatedPacketProcessor<PlayerSyncFinished>
{
    private readonly PlayerService playerManager = playerManager;
    private readonly HibernationService hibernationService = hibernationService;

    public override void Process(PlayerSyncFinished packet, NitroxServer.Player player)
    {
        // If this is the first player connecting we need to restart time at this exact moment
        if (playerManager.GetConnectedPlayers().Count == 1)
        {
            hibernationService.Resume();
        }

        playerManager.FinishProcessingReservation(player);
    }
}
