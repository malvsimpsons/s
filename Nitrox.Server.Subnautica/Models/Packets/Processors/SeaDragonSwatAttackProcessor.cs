using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaDragonSwatAttackProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaDragonSwatAttack>(playerService, entityRegistry)
{
    public async override Task Process(AuthProcessorContext context, SeaDragonSwatAttack packet) => TransmitIfCanSeeEntities(packet, context.Sender, packet.SeaDragonId, packet.TargetId);
}
