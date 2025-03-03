using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlaceGhostProcessor(BuildingManager buildingManager, PlayerService playerService) : AuthenticatedPacketProcessor<PlaceGhost>
{
    public override void Process(PlaceGhost packet, NitroxServer.Player player)
    {
        if (buildingManager.AddGhost(packet))
        {
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
