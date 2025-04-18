using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class WeldActionProcessor(SimulationOwnershipData simulationOwnershipData) : AuthenticatedPacketProcessor<WeldAction>
{
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public override void Process(WeldAction packet, NitroxServer.Player player)
    {
        NitroxServer.Player simulatingPlayer = simulationOwnershipData.GetPlayerForLock(packet.Id);

        if (simulatingPlayer != null)
        {
            Log.Debug($"Send WeldAction to simulating player {simulatingPlayer.Name} for entity {packet.Id}");
            simulatingPlayer.SendPacket(packet);
        }
    }
}
