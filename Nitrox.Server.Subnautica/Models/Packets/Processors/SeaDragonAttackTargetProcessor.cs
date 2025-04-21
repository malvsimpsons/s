using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonAttackTargetProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonAttackTarget>(playerService, entityRegistry)
{
    public async override Task Process(AuthProcessorContext context, SeaDragonAttackTarget packet) => TransmitIfCanSeeEntities(packet, context.Sender, packet.SeaDragonId, packet.TargetId);
}
