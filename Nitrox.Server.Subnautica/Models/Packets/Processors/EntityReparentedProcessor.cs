using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class EntityReparentedProcessor(EntityRegistry entityRegistry, PlayerManager playerManager) : AuthenticatedPacketProcessor<EntityReparented>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerManager playerManager = playerManager;

    public override void Process(EntityReparented packet, NitroxServer.Player player)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            Log.Error($"Couldn't find entity for {packet.Id}");
            return;
        }
        if (!entityRegistry.TryGetEntityById(packet.NewParentId, out Entity parentEntity))
        {
            Log.Error($"Couldn't find parent entity for {packet.NewParentId}");
            return;
        }
        
        entityRegistry.ReparentEntity(packet.Id, packet.NewParentId);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
