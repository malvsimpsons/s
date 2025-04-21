using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerSyncFinishedProcessor(PlayerService playerManager, HibernationService hibernationService) : IAuthPacketProcessor<PlayerSyncFinished>
{
    private readonly PlayerService playerManager = playerManager;
    private readonly HibernationService hibernationService = hibernationService;

    public async Task Process(AuthProcessorContext context, PlayerSyncFinished packet)
    {
        // TODO: USE DATABASE
        // // If this is the first player connecting we need to restart time at this exact moment
        // if (playerManager.GetConnectedPlayersAsync().Count == 1)
        // {
        //     hibernationService.Resume();
        // }
        //
        // playerManager.FinishProcessingReservation(player);
    }
}
