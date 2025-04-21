using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class AttackCyclopsTargetChangedProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<AttackCyclopsTargetChanged>(playerService, entityRegistry)
{
    public override Task Process(AuthProcessorContext context, AttackCyclopsTargetChanged packet) => TransmitIfCanSeeEntities(packet, context.Sender, packet.CreatureId, packet.TargetId);
}
