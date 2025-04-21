using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PdaScanFinishedPacketProcessor(PlayerService playerService, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<PdaScanFinished>
{
    private readonly PlayerService playerService = playerService;
    // TODO: USE DATABASE
    // private readonly PdaStateData pdaStateData = pdaStateData;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, PdaScanFinished packet)
    {
        if (!packet.WasAlreadyResearched)
        {
            // TODO: USE DATABASE
            // pdaStateData.UpdateEntryUnlockedProgress(packet.TechType, packet.UnlockedAmount, packet.FullyResearched);
        }
        context.ReplyToOthers(packet);

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
