using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SimulationOwnershipRequestProcessor(PlayerService playerService, SimulationOwnershipData simulationOwnershipData, EntitySimulation entitySimulation) : AuthenticatedPacketProcessor<SimulationOwnershipRequest>
{
    private readonly PlayerService playerService = playerService;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public override void Process(SimulationOwnershipRequest ownershipRequest, NitroxServer.Player player)
    {
        bool acquiredLock = simulationOwnershipData.TryToAcquire(ownershipRequest.Id, player, ownershipRequest.LockType);
        if (acquiredLock)
        {
            bool shouldEntityMove = entitySimulation.ShouldSimulateEntityMovement(ownershipRequest.Id);
            SimulationOwnershipChange simulationOwnershipChange = new(ownershipRequest.Id, player.Id, ownershipRequest.LockType, shouldEntityMove);
            playerService.SendPacketToOtherPlayers(simulationOwnershipChange, player);
        }

        SimulationOwnershipResponse responseToPlayer = new(ownershipRequest.Id, acquiredLock, ownershipRequest.LockType);
        player.SendPacket(responseToPlayer);
    }
}
