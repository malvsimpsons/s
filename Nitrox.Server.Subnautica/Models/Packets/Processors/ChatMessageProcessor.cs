using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class ChatMessageProcessor(PlayerManager playerManager) : AuthenticatedPacketProcessor<ChatMessage>
{
    private readonly PlayerManager playerManager = playerManager;

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