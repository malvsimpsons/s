using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class SubRootChangedPacketProcessor(PlayerService playerService, EntityRegistry entityRegistry) : AuthenticatedPacketProcessor<SubRootChanged>
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
