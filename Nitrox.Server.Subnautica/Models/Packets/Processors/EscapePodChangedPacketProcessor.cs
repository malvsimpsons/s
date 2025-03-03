using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class EscapePodChangedPacketProcessor(PlayerManager playerManager) : AuthenticatedPacketProcessor<EscapePodChanged>
{
    private readonly PlayerManager playerManager = playerManager;

    public override void Process(EscapePodChanged packet, NitroxServer.Player player)
    {
        Log.Debug(packet);
        player.SubRootId = packet.EscapePodId;
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}