using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class RangedAttackLastTargetUpdateProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<RangedAttackLastTargetUpdate>(playerService, entityRegistry)
{
    public async override Task Process(AuthProcessorContext context, RangedAttackLastTargetUpdate packet) => TransmitIfCanSeeEntities(packet, context.Sender, packet.CreatureId, packet.TargetId);
}
