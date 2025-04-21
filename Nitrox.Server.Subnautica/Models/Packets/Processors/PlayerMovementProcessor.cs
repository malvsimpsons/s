using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerMovementProcessor(PlayerService playerService, EntityRegistry entityRegistry) : IAuthPacketProcessor<PlayerMovement>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, PlayerMovement packet)
    {
        // TODO: USE DATABASE
        // Optional<PlayerWorldEntity> playerEntity = entityRegistry.GetEntityById<PlayerWorldEntity>(player.PlayerContext.PlayerNitroxId);
        //
        // if (playerEntity.HasValue)
        // {
        //     playerEntity.Value.Transform.Position = packet.Position;
        //     playerEntity.Value.Transform.Rotation = packet.BodyRotation;
        // }
        //
        // player.Position = packet.Position;
        // player.Rotation = packet.BodyRotation;
        // context.ReplyToOthers(packet);
    }
}
