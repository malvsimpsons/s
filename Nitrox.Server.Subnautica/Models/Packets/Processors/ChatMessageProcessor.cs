using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class ChatMessageProcessor : IAuthPacketProcessor<ChatMessage>
{
    public async Task Process(AuthProcessorContext context, ChatMessage packet)
    {
        // TODO: FIX - need to get context by PeerId from a service.
        // if (player.PlayerContext.IsMuted)
        // {
        //     player.SendPacket(new ChatMessage(ChatMessage.SERVER_ID, "You're currently muted"));
        //     return;
        // }
        // Log.Info($"<{player.Name}>: {packet.Text}");
        context.ReplyToAll(packet);
    }
}
