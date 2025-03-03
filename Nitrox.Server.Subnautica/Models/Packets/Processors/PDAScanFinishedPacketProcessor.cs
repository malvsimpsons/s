using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PDAScanFinishedPacketProcessor(PlayerService playerService, PDAStateData pdaStateData, WorldEntityManager worldEntityManager) : AuthenticatedPacketProcessor<PDAScanFinished>
{
    private readonly PlayerService playerService = playerService;
    private readonly PDAStateData pdaStateData = pdaStateData;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public override void Process(PDAScanFinished packet, NitroxServer.Player player)
    {
        if (!packet.WasAlreadyResearched)
        {
            pdaStateData.UpdateEntryUnlockedProgress(packet.TechType, packet.UnlockedAmount, packet.FullyResearched);
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
                pdaStateData.AddScannerFragment(packet.Id);
            }
        }
    }
}
