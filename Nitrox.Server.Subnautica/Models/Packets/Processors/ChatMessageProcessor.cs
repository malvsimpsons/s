using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class ChatMessageProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<ChatMessage>
{
    private readonly PlayerService playerManager = playerService;

    public override void Process(ChatMessage packet, NitroxServer.Player player)
    {
        if (player.PlayerContext.IsMuted)
        {
            player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You're currently muted"));
            return;
        }
        Log.Info($"<{player.Name}>: {packet.Text}");
        playerManager.SendPacketToAllPlayers(packet);
    }
}
