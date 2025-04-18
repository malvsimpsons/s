using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class AggressiveWhenSeeTargetChangedProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<AggressiveWhenSeeTargetChanged>(playerService, entityRegistry)
{
    public override void Process(AggressiveWhenSeeTargetChanged packet, NitroxServer.Player sender) => TransmitIfCanSeeEntities(packet, sender, packet.CreatureId, packet.TargetId);
}
