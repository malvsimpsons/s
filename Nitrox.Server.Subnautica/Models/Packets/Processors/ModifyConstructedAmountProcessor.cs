using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ModifyConstructedAmountProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerService playerService) : AuthenticatedPacketProcessor<ModifyConstructedAmount>
{
    private readonly PlayerService playerService = playerService;
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;

    public override void Process(ModifyConstructedAmount packet, NitroxServer.Player player)
    {
        if (buildingManager.ModifyConstructedAmount(packet))
        {
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
