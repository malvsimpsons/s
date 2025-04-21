using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WaterParkDeconstructedProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerService playerService) : IAuthPacketProcessor<WaterParkDeconstructed>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;

    public async Task Process(AuthProcessorContext context, WaterParkDeconstructed packet)
    {
        ConnectedPlayerDto player = await playerService.GetConnectedPlayerByIdAsync(context.Sender);

        if (buildingManager.ReplacePieceByGhost(player, packet, out Entity removedEntity, out int operationId) &&
            buildingManager.CreateWaterParkPiece(packet, removedEntity))
        {
            packet.BaseData = null;
            packet.OperationId = operationId;
            context.ReplyToOthers(packet);
        }
    }
}
