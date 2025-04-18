using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WaterParkDeconstructedProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerService playerService) : AuthenticatedPacketProcessor<WaterParkDeconstructed>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;

    public override void Process(WaterParkDeconstructed packet, NitroxServer.Player player)
    {
        if (buildingManager.ReplacePieceByGhost(player, packet, out Entity removedEntity, out int operationId) &&
            buildingManager.CreateWaterParkPiece(packet, removedEntity))
        {
            packet.BaseData = null;
            packet.OperationId = operationId;
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
