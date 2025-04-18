using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PvpAttackProcessor(PlayerService playerService, IOptions<SubnauticaServerOptions> configProvider) : AuthenticatedPacketProcessor<PvPAttack>
{
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<SubnauticaServerOptions> configProvider = configProvider;

    // TODO: In the future, do a whole config for damage sources
    private static readonly Dictionary<PvPAttack.AttackType, float> damageMultiplierByType = new()
    {
        { PvPAttack.AttackType.KnifeHit, 0.5f },
        { PvPAttack.AttackType.HeatbladeHit, 1f }
    };

    public override void Process(PvPAttack packet, NitroxServer.Player player)
    {
        if (!configProvider.Value.PvpEnabled)
        {
            return;
        }
        if (!playerService.TryGetPlayerById(packet.TargetPlayerId, out NitroxServer.Player targetPlayer))
        {
            return;
        }
        if (!damageMultiplierByType.TryGetValue(packet.Type, out float multiplier))
        {
            return;
        }

        packet.Damage *= multiplier;
        targetPlayer.SendPacket(packet);
    }
}
