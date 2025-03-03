using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using static NitroxModel.Packets.EntityTransformUpdates;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class EntityTransformUpdatesProcessor(PlayerManager playerManager, WorldEntityManager worldEntityManager, SimulationOwnershipData simulationOwnershipData) : AuthenticatedPacketProcessor<EntityTransformUpdates>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly SimulationOwnershipData simulationOwnershipData = simulationOwnershipData;

    public override void Process(EntityTransformUpdates packet, NitroxServer.Player simulatingPlayer)
    {
        Dictionary<NitroxServer.Player, List<EntityTransformUpdate>> visibleUpdatesByPlayer = InitializeVisibleUpdateMapWithOtherPlayers(simulatingPlayer);
        AssignVisibleUpdatesToPlayers(simulatingPlayer, packet.Updates, visibleUpdatesByPlayer);
        SendUpdatesToPlayers(visibleUpdatesByPlayer);
    }

    private Dictionary<NitroxServer.Player, List<EntityTransformUpdate>> InitializeVisibleUpdateMapWithOtherPlayers(NitroxServer.Player simulatingPlayer)
    {
        Dictionary<NitroxServer.Player, List<EntityTransformUpdate>> visibleUpdatesByPlayer = new Dictionary<NitroxServer.Player, List<EntityTransformUpdate>>();

        foreach (NitroxServer.Player player in playerManager.GetConnectedPlayers())
        {
            if (!player.Equals(simulatingPlayer))
            {
                visibleUpdatesByPlayer[player] = new List<EntityTransformUpdate>();
            }
        }

        return visibleUpdatesByPlayer;
    }

    private void AssignVisibleUpdatesToPlayers(NitroxServer.Player sendingPlayer, List<EntityTransformUpdate> updates, Dictionary<NitroxServer.Player, List<EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        foreach (EntityTransformUpdate update in updates)
        {
            if (!simulationOwnershipData.TryGetLock(update.Id, out SimulationOwnershipData.PlayerLock playerLock) || playerLock.Player != sendingPlayer)
            {
                // This will happen pretty frequently when a player moves very fast (swimfast or maybe some more edge cases) so we can just ignore this
                continue;
            }

            if (!worldEntityManager.TryUpdateEntityPosition(update.Id, update.Position, update.Rotation, out AbsoluteEntityCell currentCell, out WorldEntity worldEntity))
            {
                // Normal behaviour if the entity was removed at the same time as someone trying to simulate a postion update.
                // we log an info inside entityManager.UpdateEntityPosition just in case.
                continue;
            }

            foreach (KeyValuePair<NitroxServer.Player, List<EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
            {
                if (playerUpdates.Key.CanSee(worldEntity))
                {
                    playerUpdates.Value.Add(update);
                }
            }
        }
    }

    private void SendUpdatesToPlayers(Dictionary<NitroxServer.Player, List<EntityTransformUpdate>> visibleUpdatesByPlayer)
    {
        foreach (KeyValuePair<NitroxServer.Player, List<EntityTransformUpdate>> playerUpdates in visibleUpdatesByPlayer)
        {
            NitroxServer.Player player = playerUpdates.Key;
            List<EntityTransformUpdate> updates = playerUpdates.Value;

            if (updates.Count > 0)
            {
                EntityTransformUpdates updatesPacket = new EntityTransformUpdates(updates);
                player.SendPacket(updatesPacket);
            }
        }
    }
}