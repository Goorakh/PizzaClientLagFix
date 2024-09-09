using RoR2;
using System;

namespace PizzaClientLagFix.Networking.Wrappers
{
    public struct ProcChainMaskWrapper : IEquatable<ProcChainMaskWrapper>
    {
        public uint Mask;

        public ProcChainMaskWrapper(ProcChainMask procChainMask)
        {
            Mask = procChainMask.mask;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is ProcChainMaskWrapper wrapper && Equals(wrapper);
        }

        public readonly bool Equals(ProcChainMaskWrapper other)
        {
            return Mask == other.Mask;
        }

        public override readonly int GetHashCode()
        {
            return Mask.GetHashCode();
        }

        public static bool operator ==(ProcChainMaskWrapper left, ProcChainMaskWrapper right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProcChainMaskWrapper left, ProcChainMaskWrapper right)
        {
            return !(left == right);
        }

        public static implicit operator ProcChainMask(ProcChainMaskWrapper wrapper)
        {
            return new ProcChainMask { mask = wrapper.Mask };
        }

        public static implicit operator ProcChainMaskWrapper(ProcChainMask mask)
        {
            return new ProcChainMaskWrapper(mask);
        }
    }
}
