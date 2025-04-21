using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityTransformUpdatesProcessor(PlayerService playerService, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData) : IAuthPacketProcessor<EntityTransformUpdates>
{
    private readonly PlayerService playerService = playerService;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public Task Process(AuthProcessorContext context, EntityTransformUpdates packet)
    {
        Dictionary<PeerId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer = InitializeVisibleUpdateMapWithOtherPlayers(context.Sender);
        AssignVisibleUpdatesToPlayers(context.Sender, packet.Updates, visibleUpdatesByPlayer);
        SendUpdatesToPlayers(visibleUpdatesByPlayer);
        return Task.CompletedTask;
    }

    private Dictionary<PeerId, List<EntityTransformUpdates.EntityTransformUpdate>> InitializeVisibleUpdateMapWithOtherPlayers(PeerId simulatingPlayer)
    {
        Dictionary<PeerId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer = new();

        // TODO: Fix using database
        // foreach (PeerId player in playerManager.GetConnectedPlayersAsync())
        // {
        //     if (!player.Equals(simulatingPlayer))
        //     {
        //         visibleUpdatesByPlayer[player] = new List<EntityTransformUpdates.EntityTransformUpdate>();
        //     }
        // }

        return visibleUpdatesByPlayer;
    }

    private void AssignVisibleUpdatesToPlayers(PeerId sendingPlayer, List<EntityTransformUpdates.EntityTransformUpdate> updates, Dictionary<PeerId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        // TODO: Fix with database and peerId
        // foreach (EntityTransformUpdates.EntityTransformUpdate update in updates)
        // {
        //     if (!simulationOwnershipData.TryGetLock(update.Id, out SimulationOwnershipData.PlayerLock playerLock) || playerLock.Player != sendingPlayer)
        //     {
        //         // This will happen pretty frequently when a player moves very fast (swimfast or maybe some more edge cases) so we can just ignore this
        //         continue;
        //     }
        //
        //     if (!worldEntityManager.TryUpdateEntityPosition(update.Id, update.Position, update.Rotation, out AbsoluteEntityCell currentCell, out WorldEntity worldEntity))
        //     {
        //         // Normal behaviour if the entity was removed at the same time as someone trying to simulate a postion update.
        //         // we log an info inside entityManager.UpdateEntityPosition just in case.
        //         continue;
        //     }
        //
        //     foreach (KeyValuePair<NitroxServer.Player, List<EntityTransformUpdates.EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
        //     {
        //         if (playerUpdates.Key.CanSee(worldEntity))
        //         {
        //             playerUpdates.Value.Add(update);
        //         }
        //     }
        // }
    }

    private void SendUpdatesToPlayers(Dictionary<PeerId, List<EntityTransformUpdates.EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        foreach (KeyValuePair<PeerId, List<EntityTransformUpdates.EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
        {
            PeerId player = playerUpdates.Key;
            List<EntityTransformUpdates.EntityTransformUpdate> updates = playerUpdates.Value;

            if (updates.Count > 0)
            {
                EntityTransformUpdates updatesPacket = new(updates);
                playerService.SendPacket(updatesPacket, player);
            }
        }
    }
}
