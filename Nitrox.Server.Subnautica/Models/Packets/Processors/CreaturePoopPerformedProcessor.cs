using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreaturePoopPerformedProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreaturePoopPerformed>(playerService, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, CreaturePoopPerformed packet) => await TransmitIfCanSeeEntities(packet, context.Sender, packet.CreatureId);
}
