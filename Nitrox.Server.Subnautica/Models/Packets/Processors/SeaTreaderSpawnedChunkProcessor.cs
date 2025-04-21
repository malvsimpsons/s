using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaTreaderSpawnedChunkProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaTreaderSpawnedChunk>(playerService, entityRegistry)
{
    public async override Task Process(AuthProcessorContext context, SeaTreaderSpawnedChunk packet) => TransmitIfCanSeeEntities(packet, context.Sender, packet.CreatureId);
}
