using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Serialization;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class PlayerDeathEventProcessor(PlayerService playerService, SubnauticaServerConfig config) : AuthenticatedPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerService playerService = playerService;
    private readonly SubnauticaServerConfig serverConfig = config;

    public override void Process(PlayerDeathEvent packet, NitroxServer.Player player)
    {
        if (serverConfig.IsHardcore())
        {
            player.IsPermaDeath = true;
            PlayerKicked playerKicked = new("Permanent death from hardcore mode");
            player.SendPacket(playerKicked);
        }

        player.LastStoredPosition = packet.DeathPosition;
        player.LastStoredSubRootID = player.SubRootId;

        if (player.Permissions > Perms.MODERATOR)
        {
            player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You can use /back to go to your death location"));
        }

        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
