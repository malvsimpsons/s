using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonAttackTargetProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonAttackTarget>(playerService, entityRegistry)
{
    public override void Process(SeaDragonAttackTarget packet, NitroxServer.Player sender) => TransmitIfCanSeeEntities(packet, sender, packet.SeaDragonId, packet.TargetId);
}
