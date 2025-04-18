using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class RemoveCreatureCorpseProcessor(PlayerService playerService, EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : AuthenticatedPacketProcessor<RemoveCreatureCorpse>
{
    private readonly PlayerService playerManager = playerService;
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public override void Process(RemoveCreatureCorpse packet, NitroxServer.Player destroyingPlayer)
    {
        // TODO: In the future, for more immersion (though that's a neglectable +), have a corpse entity on server-side or a dedicated metadata for this entity (CorpseMetadata)
        // So that even players rejoining can see it (before it despawns)
        entitySimulation.EntityDestroyed(packet.CreatureId);

        if (worldEntityManager.TryDestroyEntity(packet.CreatureId, out Entity entity))
        {
            foreach (NitroxServer.Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != destroyingPlayer;
                if (isOtherPlayer && player.CanSee(entity))
                {
                    player.SendPacket(packet);
                }
            }
        }
    }
}
