using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Bases;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class UpdateBaseProcessor(BuildingManager buildingManager, PlayerService playerService, GameLogic.EntitySimulation entitySimulation) : AuthenticatedPacketProcessor<UpdateBase>
{
    public override void Process(UpdateBase packet, NitroxServer.Player player)
    {
        if (buildingManager.UpdateBase(player, packet, out int operationId))
        {
            if (packet.BuiltPieceEntity is GlobalRootEntity entity)
            {
                entitySimulation.ClaimBuildPiece(entity, player);
            }
            // End-players can process elementary operations without this data (packet would be heavier for no reason)
            packet.Deflate();
            packet.OperationId = operationId;
            playerService.SendPacketToOtherPlayers(packet, player);
        }
    }
}
