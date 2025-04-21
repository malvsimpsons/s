using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Configuration;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Services;
using NitroxModel.Dto;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PvpAttackProcessor(PlayerService playerService, IOptions<SubnauticaServerOptions> configProvider) : IAuthPacketProcessor<PvpAttack>
{
    private readonly PlayerService playerService = playerService;
    private readonly IOptions<SubnauticaServerOptions> configProvider = configProvider;

    // TODO: In the future, do a whole config for damage sources
    private static readonly Dictionary<PvpAttack.AttackType, float> damageMultiplierByType = new()
    {
        { PvpAttack.AttackType.KnifeHit, 0.5f },
        { PvpAttack.AttackType.HeatbladeHit, 1f }
    };

    public async Task Process(AuthProcessorContext context, PvpAttack packet)
    {
        if (!configProvider.Value.PvpEnabled)
        {
            return;
        }
        if (!damageMultiplierByType.TryGetValue(packet.Type, out float multiplier))
        {
            return;
        }

        packet.Damage *= multiplier;
        playerService.SendPacket(packet, packet.TargetPlayerId);
    }
}
