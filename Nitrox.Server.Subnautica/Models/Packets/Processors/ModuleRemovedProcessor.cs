using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class ModuleRemovedProcessor(PlayerService playerService, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<ModuleRemoved>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(ModuleRemoved packet, NitroxServer.Player player)
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
            playerService.SendPacketToOtherPlayers(new SpawnEntities(inventoryEntity, forceRespawn: true), player);
        }
    }
}
