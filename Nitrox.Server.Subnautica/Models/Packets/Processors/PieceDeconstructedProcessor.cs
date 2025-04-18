using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PieceDeconstructedProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerService playerService) : AuthenticatedPacketProcessor<PieceDeconstructed>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;

    public override void Process(PieceDeconstructed packet, NitroxServer.Player player)
    {
        if (buildingManager.ReplacePieceByGhost(player, packet, out _, out int operationId))
        {
            packet.BaseData = null;
            packet.OperationId = operationId;
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
