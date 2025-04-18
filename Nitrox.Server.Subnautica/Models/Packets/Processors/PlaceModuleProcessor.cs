using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceModuleProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerService playerService, GameLogic.EntitySimulation entitySimulation) : AuthenticatedPacketProcessor<PlaceModule>
{
    public override void Process(PlaceModule packet, NitroxServer.Player player)
    {
        if (buildingManager.AddModule(packet))
        {
            if (packet.ModuleEntity.ParentId == null)
            {
                entitySimulation.ClaimBuildPiece(packet.ModuleEntity, player);
            }
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
