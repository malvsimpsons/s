using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class BaseDeconstructedProcessor(BuildingManager buildingManager, PlayerService playerService) : AuthenticatedPacketProcessor<BaseDeconstructed>
{
    private readonly BuildingManager buildingManager = buildingManager;
    private readonly PlayerService playerService = playerService;

    public override void Process(BaseDeconstructed packet, NitroxServer.Player player)
    {
        if (buildingManager.ReplaceBaseByGhost(packet))
        {
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
