using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerSeeOutOfCellEntityProcessor(EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<PlayerSeeOutOfCellEntity>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(PlayerSeeOutOfCellEntity packet, NitroxServer.Player player)
    {
        if (entityRegistry.GetEntityById(packet.EntityId).HasValue)
        {
            player.OutOfCellVisibleEntities.Add(packet.EntityId);
        }
    }
}
