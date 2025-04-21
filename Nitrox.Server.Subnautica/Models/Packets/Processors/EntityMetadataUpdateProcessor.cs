using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Networking.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityMetadataUpdateProcessor(PlayerService playerService, EntityRegistry entityRegistry) : IAuthPacketProcessor<EntityMetadataUpdate>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, EntityMetadataUpdate packet)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            Log.Error($"Entity metadata {packet.NewValue.GetType()} updated on an entity unknown to the server {packet.Id}");
            return;
        }

        // TODO: FIX
        // if (TryProcessMetadata(context.Sender, entity, packet.NewValue))
        // {
        //     entity.Metadata = packet.NewValue;
        //     SendUpdateToVisiblePlayers(packet, context.Sender, entity);
        // }
    }

    private void SendUpdateToVisiblePlayers(EntityMetadataUpdate packet, PeerId sendingPlayer, Entity entity)
    {
        // TODO: FIX WITH DATABASE
        // foreach (NitroxServer.Player player in playerService.GetConnectedPlayersAsync())
        // {
        //     bool updateVisibleToPlayer = player.CanSee(entity);
        //
        //     if (player != sendingPlayer && updateVisibleToPlayer)
        //     {
        //         player.SendPacket(packet);
        //     }
        // }
    }

    private bool TryProcessMetadata(PeerId sendingPlayer, Entity entity, EntityMetadata metadata)
    {
        return metadata switch
        {
            PlayerMetadata playerMetadata => ProcessPlayerMetadata(sendingPlayer, entity, playerMetadata),

            // Allow metadata updates from any player by default
            _ => true
        };
    }

    private bool ProcessPlayerMetadata(PeerId sendingPlayer, Entity entity, PlayerMetadata metadata)
    {
        // TODO: FIX WITH DATABASE
        // if (sendingPlayer.GameObjectId == entity.Id)
        // {
        //     sendingPlayer.EquippedItems.Clear();
        //     foreach (PlayerMetadata.EquippedItem item in metadata.EquippedItems)
        //     {
        //         sendingPlayer.EquippedItems.Add(item.Slot, item.Id);
        //     }
        //
        //     return true;
        // }
        //
        // Log.WarnOnce($"Player {sendingPlayer.Name} tried updating metadata of another player's entity {entity.Id}");
        // return false;

        return false;
    }
}
