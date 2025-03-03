using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonSwatAttackProcessor(
    PlayerService playerService,
    EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonSwatAttack>(playerService, entityRegistry)
{
    public override void Process(SeaDragonSwatAttack packet, NitroxServer.Player sender) => TransmitIfCanSeeEntities(packet, sender, packet.SeaDragonId, packet.TargetId);
}
