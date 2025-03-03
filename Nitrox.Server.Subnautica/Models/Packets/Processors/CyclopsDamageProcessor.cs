using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel_Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     This is the absolute damage state. The current simulation owner is the only one who sends this packet to the server
/// </summary>
internal sealed class CyclopsDamageProcessor(PlayerService playerService) : AuthenticatedPacketProcessor<CyclopsDamage>
{
    private readonly PlayerService playerService = playerService;

    public override void Process(CyclopsDamage packet, NitroxServer.Player simulatingPlayer)
    {
        Log.Debug($"New cyclops damage from {simulatingPlayer.Id} {packet}");

        playerService.SendPacketToOtherPlayers(packet, simulatingPlayer);
    }
}
