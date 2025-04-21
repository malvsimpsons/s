using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SimulationOwnershipRequestProcessor(PlayerService playerService, SimulationOwnershipData simulationOwnershipData, EntitySimulation entitySimulation) : IAuthPacketProcessor<SimulationOwnershipRequest>
{
    private readonly PlayerService playerService = playerService;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public async Task Process(AuthProcessorContext context, SimulationOwnershipRequest ownershipRequest)
    {
        bool acquiredLock = simulationOwnershipData.TryToAcquire(ownershipRequest.Id, context.Sender, ownershipRequest.LockType);
        if (acquiredLock)
        {
            bool shouldEntityMove = entitySimulation.ShouldSimulateEntityMovement(ownershipRequest.Id);
            SimulationOwnershipChange simulationOwnershipChange = new(ownershipRequest.Id, context.Sender, ownershipRequest.LockType, shouldEntityMove);
            playerService.SendPacketToOtherPlayers(simulationOwnershipChange, context.Sender);
        }

        SimulationOwnershipResponse responseToPlayer = new(ownershipRequest.Id, acquiredLock, ownershipRequest.LockType);
        context.ReplyToSender(responseToPlayer);
    }
}
