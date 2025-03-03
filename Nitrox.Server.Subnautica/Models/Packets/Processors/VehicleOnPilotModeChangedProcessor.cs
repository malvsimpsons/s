using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleOnPilotModeChangedProcessor(PlayerService playerManager) : AuthenticatedPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly PlayerService playerManager = playerManager;

    public override void Process(VehicleOnPilotModeChanged packet, NitroxServer.Player player)
    {
        player.PlayerContext.DrivingVehicle = packet.IsPiloting ? packet.VehicleId : null;

        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
