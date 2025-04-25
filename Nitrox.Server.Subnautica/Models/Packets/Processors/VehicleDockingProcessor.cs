using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class VehicleDockingProcessor(EntityRegistry entityRegistry) : IAuthPacketProcessor<VehicleDocking>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, VehicleDocking packet)
    {
        if (!entityRegistry.TryGetEntityById(packet.VehicleId, out Entity vehicleEntity))
        {
            Log.Error($"Unable to find vehicle to dock {packet.VehicleId}");
            return;
        }

        if (!entityRegistry.TryGetEntityById(packet.DockId, out Entity dockEntity))
        {
            Log.Error($"Unable to find dock {packet.DockId} for docking vehicle {packet.VehicleId}");
            return;
        }

        entityRegistry.ReparentEntity(vehicleEntity, dockEntity);
        context.ReplyToOthers(packet);
    }
}
