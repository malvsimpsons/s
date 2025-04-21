using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public class PvpAttack : Packet
{
    public PeerId TargetPlayerId { get; }
    public float Damage { get; set; }
    public AttackType Type { get; }

    public PvpAttack(PeerId targetPlayerId, float damage, AttackType type)
    {
        TargetPlayerId = targetPlayerId;
        Damage = damage;
        Type = type;
    }

    public enum AttackType : byte
    {
        KnifeHit,
        HeatbladeHit
    }
}
