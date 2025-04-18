using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityReparentedProcessor(EntityRegistry entityRegistry, PlayerService playerService) : AuthenticatedPacketProcessor<EntityReparented>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerService playerManager = playerService;

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
