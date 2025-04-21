using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class ModuleRemovedProcessor(EntityRegistry entityRegistry) : IAuthPacketProcessor<ModuleRemoved>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, ModuleRemoved packet)
    {
        Optional<Entity> entity = entityRegistry.GetEntityById(packet.Id);

        if (!entity.HasValue)
        {
            Log.Error($"Could not find entity {packet.Id} module added to a vehicle.");
            return;
        }

        if (entity.Value is InstalledModuleEntity installedModule)
        {
            InventoryItemEntity inventoryEntity = new(installedModule.Id, installedModule.ClassId, installedModule.TechType, installedModule.Metadata, packet.NewParentId, installedModule.ChildEntities);

            // Convert the world entity into an inventory item
            entityRegistry.AddOrUpdate(inventoryEntity);

            // Have other players respawn the item inside the inventory.
            context.ReplyToOthers(new SpawnEntities(inventoryEntity, forceRespawn: true));
        }
    }
}
