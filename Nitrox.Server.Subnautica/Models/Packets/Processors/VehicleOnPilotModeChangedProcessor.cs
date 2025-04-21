using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleOnPilotModeChangedProcessor(PlayerService playerManager) : IAuthPacketProcessor<VehicleOnPilotModeChanged>
{
    private readonly PlayerService playerManager = playerManager;

    public async Task Process(AuthProcessorContext context, VehicleOnPilotModeChanged packet)
    {
        // TODO: USE DATABASE
        // player.PlayerContext.DrivingVehicle = packet.IsPiloting ? packet.VehicleId : null;

        context.ReplyToOthers(packet);
    }
}
