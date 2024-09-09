using RoR2;
using System;

namespace PizzaClientLagFix.Networking.Wrappers
{
    public struct DamageTypeComboWrapper : IEquatable<DamageTypeComboWrapper>
    {
        public ulong DamageTypeMask;

        public DamageTypeComboWrapper(DamageTypeCombo damageType)
        {
            DamageTypeMask = damageType.damageTypeCombined;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is DamageTypeComboWrapper wrapper && Equals(wrapper);
        }

        public readonly bool Equals(DamageTypeComboWrapper other)
        {
            return DamageTypeMask == other.DamageTypeMask;
        }

        public override readonly int GetHashCode()
        {
            return DamageTypeMask.GetHashCode();
        }

        public static bool operator ==(DamageTypeComboWrapper left, DamageTypeComboWrapper right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DamageTypeComboWrapper left, DamageTypeComboWrapper right)
        {
            return !(left == right);
        }

        public static implicit operator DamageTypeCombo(DamageTypeComboWrapper wrapper)
        {
            return wrapper.DamageTypeMask;
        }

        public static implicit operator DamageTypeComboWrapper(DamageTypeCombo damageType)
        {
            return new DamageTypeComboWrapper(damageType);
        }
    }
}
