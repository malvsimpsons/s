using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SeaTreaderSpawnedChunkProcessor(
    PlayerService playerService,
    GameLogic.EntityRegistry entityRegistry
) : TransmitIfCanSeePacketProcessor<SeaTreaderSpawnedChunk>(playerService, entityRegistry)
{
    public override void Process(SeaTreaderSpawnedChunk packet, NitroxServer.Player sender) => TransmitIfCanSeeEntities(packet, sender, packet.CreatureId);
}
