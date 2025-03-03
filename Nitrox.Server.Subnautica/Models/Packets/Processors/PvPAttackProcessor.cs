using System.Collections.Generic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxModel.Serialization;
using NitroxServer.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PvPAttackProcessor(SubnauticaServerConfig serverConfig, PlayerManager playerManager) : AuthenticatedPacketProcessor<PvPAttack>
{
    private readonly SubnauticaServerConfig serverConfig = serverConfig;
    private readonly PlayerManager playerManager = playerManager;

    // TODO: In the future, do a whole config for damage sources
    private static readonly Dictionary<PvPAttack.AttackType, float> damageMultiplierByType = new()
    {
        { PvPAttack.AttackType.KnifeHit, 0.5f },
        { PvPAttack.AttackType.HeatbladeHit, 1f }
    };

    public override void Process(PvPAttack packet, NitroxServer.Player player)
    {
        if (!serverConfig.PvPEnabled)
        {
            return;
        }
        if (!playerManager.TryGetPlayerById(packet.TargetPlayerId, out NitroxServer.Player targetPlayer))
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
