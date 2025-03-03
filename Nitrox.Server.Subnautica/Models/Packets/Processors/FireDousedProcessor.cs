using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class FireDousedProcessor(PlayerManager playerManager) : AuthenticatedPacketProcessor<FireDoused>
{
    private readonly PlayerManager playerManager = playerManager;

    public override void Process(FireDoused packet, NitroxServer.Player simulatingPlayer)
    {
        playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
    }
}