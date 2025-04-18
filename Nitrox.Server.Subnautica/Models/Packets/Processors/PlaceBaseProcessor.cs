using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceBaseProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerService playerService, GameLogic.EntitySimulation entitySimulation) : AuthenticatedPacketProcessor<PlaceBase>
{
    public override void Process(PlaceBase packet, NitroxServer.Player player)
    {
        if (buildingManager.CreateBase(packet))
        {
            entitySimulation.ClaimBuildPiece(packet.BuildEntity, player);
            packet.Deflate();
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
