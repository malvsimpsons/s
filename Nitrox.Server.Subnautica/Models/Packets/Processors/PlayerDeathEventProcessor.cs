using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class PlayerDeathEventProcessor(PlayerService playerService, IOptions<SubnauticaServerOptions> optionsProvider) : IAuthPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<SubnauticaServerOptions> serverOptionsProvider = optionsProvider;

    public async Task Process(AuthProcessorContext context, PlayerDeathEvent packet)
    {
        // TODO: USE DATABASE
        // if (serverOptionsProvider.Value.IsHardcore())
        // {
        //     player.IsPermaDeath = true;
        //     PlayerKicked playerKicked = new("Permanent death from hardcore mode");
        //     player.SendPacket(playerKicked);
        // }
        //
        // player.LastStoredPosition = packet.DeathPosition;
        // player.LastStoredSubRootID = player.SubRootId;
        //
        // if (player.Permissions > Perms.MODERATOR)
        // {
        //     player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You can use /back to go to your death location"));
        // }
        //
        // context.ReplyToOthers(packet);
    }
}
