using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonGrabExosuitProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonGrabExosuit>(playerService, entityRegistry)
{
    public async override Task Process(AuthProcessorContext context, SeaDragonGrabExosuit packet) => TransmitIfCanSeeEntities(packet, context.Sender, packet.SeaDragonId, packet.TargetId);
}
