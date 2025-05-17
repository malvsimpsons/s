using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityReparentedProcessor(EntityRegistry entityRegistry, ILogger<EntityReparentedProcessor> logger) : IAuthPacketProcessor<EntityReparented>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<EntityReparentedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, EntityReparented packet)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            logger.ZLogError($"Couldn't find entity for {packet.Id}");
            return;
        }
        if (!entityRegistry.TryGetEntityById(packet.NewParentId, out Entity parentEntity))
        {
            logger.ZLogError($"Couldn't find parent entity for {packet.NewParentId}");
            return;
        }

        entityRegistry.ReparentEntity(packet.Id, packet.NewParentId);
        await context.ReplyToOthers(packet);
    }
}
