using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerStatsProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<PlayerStats>
{
    private readonly PlayerService playerService = playerService;

    public override void Process(PlayerStats packet, NitroxServer.Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            Log.WarnOnce($"[{nameof(PlayerStatsProcessor)}] Player ID mismatch (received: {packet.PlayerId}, real: {player.Id})");
            packet.PlayerId = player.Id;
        }
        player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        playerService.SendPacketToOtherPlayers(packet, player);
    }
}
