using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class AggressiveWhenSeeTargetChangedProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<AggressiveWhenSeeTargetChanged>(playerService, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, AggressiveWhenSeeTargetChanged packet) => await TransmitIfCanSeeEntities(packet, context.Sender, packet.CreatureId, packet.TargetId);
}
