using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Entities;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class SubRootChangedPacketProcessor(PlayerService playerService, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<SubRootChanged>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public override void Process(SubRootChanged packet, NitroxServer.Player player)
    {
        entityRegistry.ReparentEntity(player.GameObjectId, packet.SubRootId.OrNull());
        player.SubRootId = packet.SubRootId;
        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
