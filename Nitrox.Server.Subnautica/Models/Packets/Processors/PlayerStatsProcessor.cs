using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlayerStatsProcessor(PlayerManager playerManager) : AuthenticatedPacketProcessor<PlayerStats>
{
    private readonly PlayerManager playerManager = playerManager;

    public override void Process(PlayerStats packet, NitroxServer.Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            Log.WarnOnce($"[{nameof(PlayerStatsProcessor)}] Player ID mismatch (received: {packet.PlayerId}, real: {player.Id})");
            packet.PlayerId = player.Id;
        }
        player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
