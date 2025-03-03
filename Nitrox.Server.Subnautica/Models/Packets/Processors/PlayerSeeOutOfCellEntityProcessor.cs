using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlayerSeeOutOfCellEntityProcessor(EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<PlayerSeeOutOfCellEntity>
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
