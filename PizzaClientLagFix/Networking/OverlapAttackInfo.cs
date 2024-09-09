using PizzaClientLagFix.Networking.Wrappers;
using RoR2;
using RoR2.Audio;
using System;
using UnityEngine;

namespace PizzaClientLagFix.Networking
{
    public struct OverlapAttackInfo : IEquatable<OverlapAttackInfo>
    {
        public GameObject Attacker;

        public GameObject Inflictor;

        public TeamIndex TeamIndex;

        public AttackerFiltering AttackerFiltering;

        public Vector3 ForceVector;

        public float PushAwayForce;

        public float Damage;

        public bool IsCrit;

        public ProcChainMaskWrapper ProcChainMask;

        public float ProcCoefficient;

        public NetworkSoundEventIndex ImpactSound;

        public DamageColorIndex DamageColorIndex;

        public DamageTypeComboWrapper DamageType;

        public int MaximumOverlapTargets;

        public float RetriggerTimeout;

        public OverlapAttackInfo(OverlapAttack src)
        {
            Attacker = src.attacker;
            Inflictor = src.inflictor;
            TeamIndex = src.teamIndex;
            AttackerFiltering = src.attackerFiltering;
            ForceVector = src.forceVector;
            PushAwayForce = src.pushAwayForce;
            Damage = src.damage;
            IsCrit = src.isCrit;
            ProcChainMask = src.procChainMask;
            ProcCoefficient = src.procCoefficient;
            ImpactSound = src.impactSound;
            DamageColorIndex = src.damageColorIndex;
            DamageType = src.damageType;
            MaximumOverlapTargets = src.maximumOverlapTargets;
            RetriggerTimeout = src.retriggerTimeout;
        }

        public readonly void ApplyTo(OverlapAttack attack)
        {
            attack.attacker = Attacker;
            attack.inflictor = Inflictor;
            attack.teamIndex = TeamIndex;
            attack.attackerFiltering = AttackerFiltering;
            attack.forceVector = ForceVector;
            attack.pushAwayForce = PushAwayForce;
            attack.damage = Damage;
            attack.isCrit = IsCrit;
            attack.procChainMask = ProcChainMask;
            attack.procCoefficient = ProcCoefficient;
            attack.impactSound = ImpactSound;
            attack.damageColorIndex = DamageColorIndex;
            attack.damageType = DamageType;
            attack.maximumOverlapTargets = MaximumOverlapTargets;
            attack.retriggerTimeout = RetriggerTimeout;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is OverlapAttackInfo info && Equals(info);
        }

        public readonly bool Equals(OverlapAttackInfo other)
        {
            return Attacker == other.Attacker &&
                   Inflictor == other.Inflictor &&
                   TeamIndex == other.TeamIndex &&
                   AttackerFiltering == other.AttackerFiltering &&
                   ForceVector == other.ForceVector &&
                   PushAwayForce == other.PushAwayForce &&
                   Damage == other.Damage &&
                   IsCrit == other.IsCrit &&
                   ProcChainMask == other.ProcChainMask &&
                   ProcCoefficient == other.ProcCoefficient &&
                   ImpactSound == other.ImpactSound &&
                   DamageColorIndex == other.DamageColorIndex &&
                   DamageType == other.DamageType &&
                   MaximumOverlapTargets == other.MaximumOverlapTargets &&
                   RetriggerTimeout == other.RetriggerTimeout;
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Attacker);
            hash.Add(Inflictor);
            hash.Add(TeamIndex);
            hash.Add(AttackerFiltering);
            hash.Add(ForceVector);
            hash.Add(PushAwayForce);
            hash.Add(Damage);
            hash.Add(IsCrit);
            hash.Add(ProcChainMask);
            hash.Add(ProcCoefficient);
            hash.Add(ImpactSound);
            hash.Add(DamageColorIndex);
            hash.Add(DamageType);
            hash.Add(MaximumOverlapTargets);
            hash.Add(RetriggerTimeout);
            return hash.ToHashCode();
        }

        public static bool operator ==(OverlapAttackInfo left, OverlapAttackInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(OverlapAttackInfo left, OverlapAttackInfo right)
        {
            return !(left == right);
        }
    }
}
