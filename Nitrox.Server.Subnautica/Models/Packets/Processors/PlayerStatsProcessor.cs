using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerStatsProcessor(PlayerService playerService) : IAuthPacketProcessor<PlayerStats>
{
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, PlayerStats packet)
    {
        if (packet.PlayerId != context.Sender)
        {
            Log.WarnOnce($"[{nameof(PlayerStatsProcessor)}] Player ID mismatch (received: {packet.PlayerId}, real: {context.Sender})");
            packet.PlayerId = context.Sender;
        }
        // TODO: USE DATABASE
        // player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        context.ReplyToOthers(packet);
    }
}
