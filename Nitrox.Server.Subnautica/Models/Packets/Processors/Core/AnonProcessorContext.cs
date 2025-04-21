using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;
using NitroxModel.Networking.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

/// <summary>
///     Context used by <see cref="IAuthPacketProcessor{TPacket}" />.
/// </summary>
internal record AnonProcessorContext : IPacketProcessContext<SessionId>
{
    private readonly PlayerService playerService;
    public SessionId Sender { get; set; }

    public AnonProcessorContext(SessionId sender, PlayerService playerService)
    {
        this.playerService = playerService;
        Sender = sender;
    }

    // TODO: Allow session id to send packets as well.
    // public void ReplyToSender<T>(T packet) where T : Packet => playerService.SendPacket(packet, Sender);
    //
    // public void ReplyToAll<T>(T packet) where T : Packet => playerService.SendPacketToAllPlayers(packet);
    //
    // public void ReplyToOthers<T>(T packet) where T : Packet => playerService.SendPacketToOtherPlayers(packet, Sender);
    public void Reply<T>(T packet) where T : Packet
    {
        playerService.SendPacket(packet, Sender);
    }
}
