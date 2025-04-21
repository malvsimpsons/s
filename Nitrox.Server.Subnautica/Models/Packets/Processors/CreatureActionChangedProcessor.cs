using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking;
using NitroxModel.Networking.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class CreatureActionChangedProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<CreatureActionChanged>(playerService, entityRegistry)
{
    public override async Task Process(AuthProcessorContext context, CreatureActionChanged packet) => await TransmitIfCanSeeEntities(packet, context.Sender, packet.CreatureId);
}
