using Microsoft.Extensions.Logging;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PieceDeconstructedProcessor(GameLogic.Bases.BuildingManager buildingManager, PlayerService playerService, ILogger<PieceDeconstructedProcessor> logger) : IAuthPacketProcessor<PieceDeconstructed>
{
    private readonly GameLogic.Bases.BuildingManager buildingManager = buildingManager;
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<PieceDeconstructedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PieceDeconstructed packet)
    {
        // TODO: Verify behavior is correct
        ConnectedPlayerDto connectedPlayer = await playerService.GetConnectedPlayerByIdAsync(context.Sender);
        if (connectedPlayer == null)
        {
            logger.LogWarning("Lost connection with player id {PlayerId}", context.Sender);
            packet.BaseData = null;
            packet.OperationId = -1;
            context.ReplyToOthers(packet);
            return;
        }

        if (buildingManager.ReplacePieceByGhost(connectedPlayer, packet, out _, out int operationId))
        {
            packet.BaseData = null;
            packet.OperationId = operationId;
            context.ReplyToOthers(packet);
        }
    }
}
