using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonGrabExosuitProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonGrabExosuit>(playerService, entityRegistry)
{
    public override void Process(SeaDragonGrabExosuit packet, NitroxServer.Player sender) => TransmitIfCanSeeEntities(packet, sender, packet.SeaDragonId, packet.TargetId);
}
