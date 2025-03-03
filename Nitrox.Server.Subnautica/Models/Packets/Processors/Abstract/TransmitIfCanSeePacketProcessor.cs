using System;
using System.Buffers;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;

internal abstract class TransmitIfCanSeePacketProcessor<T>(PlayerService playerService, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<T>
    where T : Packet
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerService playerService = playerService;

    /// <summary>
    ///     Transmits the provided <paramref name="packet" /> to all other players (excluding <paramref name="senderPlayer" />)
    ///     who can see (<see cref="NitroxServer.Player.CanSee" />) entities corresponding to the provided
    ///     <paramref name="entityIds" /> only if all those entities are registered.
    /// </summary>
    protected void TransmitIfCanSeeEntities(Packet packet, NitroxServer.Player senderPlayer, params Span<NitroxId> entityIds)
    {
        Entity[] entities = ArrayPool<Entity>.Shared.Rent(entityIds.Length);
        try
        {
            int knownEntityCount = 0;
            foreach (NitroxId entityId in entityIds)
            {
                if (entityRegistry.TryGetEntityById(entityId, out Entity entity))
                {
                    entities[knownEntityCount++] = entity;
                }
                else
                {
                    return;
                }
            }

            foreach (NitroxServer.Player player in playerService.GetConnectedPlayersExcept(senderPlayer))
            {
                bool canSeeAll = true;
                foreach (Entity entity in entities.AsSpan()[..knownEntityCount])
                {
                    if (!player.CanSee(entity))
                    {
                        canSeeAll = false;
                        break;
                    }
                }

                if (canSeeAll)
                {
                    player.SendPacket(packet);
                }
            }
        }
        finally
        {
            ArrayPool<Entity>.Shared.Return(entities);
        }
    }
}
