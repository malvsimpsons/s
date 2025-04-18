using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityMetadataUpdateProcessor(PlayerService playerService, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<EntityMetadataUpdate>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(EntityMetadataUpdate packet, NitroxServer.Player sendingPlayer)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            Log.Error($"Entity metadata {packet.NewValue.GetType()} updated on an entity unknown to the server {packet.Id}");
            return;
        }

        if (TryProcessMetadata(sendingPlayer, entity, packet.NewValue))
        {
            entity.Metadata = packet.NewValue;
            SendUpdateToVisiblePlayers(packet, sendingPlayer, entity);
        }
    }

    private void SendUpdateToVisiblePlayers(EntityMetadataUpdate packet, NitroxServer.Player sendingPlayer, Entity entity)
    {
        foreach (NitroxServer.Player player in playerService.GetConnectedPlayers())
        {
            bool updateVisibleToPlayer = player.CanSee(entity);

            if (player != sendingPlayer && updateVisibleToPlayer)
            {
                player.SendPacket(packet);
            }
        }
    }

    private bool TryProcessMetadata(NitroxServer.Player sendingPlayer, Entity entity, EntityMetadata metadata)
    {
        return metadata switch
        {
            PlayerMetadata playerMetadata => ProcessPlayerMetadata(sendingPlayer, entity, playerMetadata),

            // Allow metadata updates from any player by default
            _ => true
        };
    }

    private bool ProcessPlayerMetadata(NitroxServer.Player sendingPlayer, Entity entity, PlayerMetadata metadata)
    {
        if (sendingPlayer.GameObjectId == entity.Id)
        {
            sendingPlayer.EquippedItems.Clear();
            foreach (PlayerMetadata.EquippedItem item in metadata.EquippedItems)
            {
                sendingPlayer.EquippedItems.Add(item.Slot, item.Id);
            }

            return true;
        }

        Log.WarnOnce($"Player {sendingPlayer.Name} tried updating metadata of another player's entity {entity.Id}");
        return false;
    }
}
