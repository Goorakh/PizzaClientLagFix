using RoR2;
using RoR2.Projectile;
using System;
using UnityEngine.Networking;

namespace PizzaClientLagFix.Networking
{
    public sealed class ProjectileOverlapAttackValueNetworker : NetworkBehaviour
    {
        ProjectileController _projectileController;
        ProjectileDamage _projectileDamage;

        const uint PROC_CHAIN_MASK_DIRTY_BIT = 1 << 0;
        ProcChainMask _procChainMask;

        const uint PROC_COEFFICIENT_DIRTY_BIT = 1 << 1;
        float _procCoefficient = 1f;

        const uint DAMAGE_DIRTY_BIT = 1 << 2;
        float _damage;

        const uint FORCE_DIRTY_BIT = 1 << 3;
        float _force;

        const uint CRIT_DIRTY_BIT = 1 << 4;
        bool _crit;

        const uint DAMAGE_COLOR_INDEX_DIRTY_BIT = 1 << 5;
        DamageColorIndex _damageColorIndex;

        const uint DAMAGE_TYPE_DIRTY_BIT = 1 << 6;
        DamageTypeCombo _damageType = DamageTypeCombo.Generic;

        void Awake()
        {
            _projectileController = GetComponent<ProjectileController>();
            _projectileDamage = GetComponent<ProjectileDamage>();

            _projectileController.onInitialized += onProjectileInitialized;
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            refreshValues();
        }

        void FixedUpdate()
        {
            refreshValues();
        }

        void onProjectileInitialized(ProjectileController projectileController)
        {
            refreshValues();
        }

        void refreshValues()
        {
            bool checkValue<T>(ref T authorityValue, ref T currentValue, uint dirtyBit, Func<T, T, bool> comparison)
            {
                if (comparison(authorityValue, currentValue))
                    return false;
                
                if (NetworkServer.active)
                {
                    authorityValue = currentValue;
                    SetDirtyBit(dirtyBit);
                }
                else
                {
                    currentValue = authorityValue;
                }

                return true;
            }

            bool checkValueEquatable<T>(ref T authorityValue, ref T currentValue, uint dirtyBit) where T : IEquatable<T>
            {
                return checkValue(ref authorityValue, ref currentValue, dirtyBit, (a, b) => a.Equals(b));
            }

            uint oldDirtyBits = syncVarDirtyBits;

            if (_projectileController)
            {
                ProcChainMask procChainMask = _projectileController.procChainMask;
                checkValueEquatable(ref _procChainMask, ref procChainMask, PROC_CHAIN_MASK_DIRTY_BIT);
                _projectileController.procChainMask = procChainMask;

                checkValueEquatable(ref _procCoefficient, ref _projectileController.procCoefficient, PROC_COEFFICIENT_DIRTY_BIT);
            }

            if (_projectileDamage)
            {
                checkValueEquatable(ref _damage, ref _projectileDamage.damage, DAMAGE_DIRTY_BIT);

                checkValueEquatable(ref _force, ref _projectileDamage.force, FORCE_DIRTY_BIT);

                checkValueEquatable(ref _crit, ref _projectileDamage.crit, CRIT_DIRTY_BIT);

                checkValue(ref _damageColorIndex, ref _projectileDamage.damageColorIndex, DAMAGE_COLOR_INDEX_DIRTY_BIT, (a, b) => a == b);

                checkValue(ref _damageType, ref _projectileDamage.damageType, DAMAGE_TYPE_DIRTY_BIT, (a, b) => a.Equals(b));
            }
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            uint dirtyBits;
            if (initialState)
            {
                dirtyBits = ~0u;
            }
            else
            {
                dirtyBits = syncVarDirtyBits;
                writer.WritePackedUInt32(dirtyBits);
            }

            bool anythingWritten = false;

            if ((dirtyBits & PROC_CHAIN_MASK_DIRTY_BIT) != 0)
            {
                writer.Write(_procChainMask);
                anythingWritten = true;
            }

            if ((dirtyBits & PROC_COEFFICIENT_DIRTY_BIT) != 0)
            {
                writer.Write(_procCoefficient);
                anythingWritten = true;
            }

            if ((dirtyBits & DAMAGE_DIRTY_BIT) != 0)
            {
                writer.Write(_damage);
                anythingWritten = true;
            }

            if ((dirtyBits & FORCE_DIRTY_BIT) != 0)
            {
                writer.Write(_force);
                anythingWritten = true;
            }

            if ((dirtyBits & CRIT_DIRTY_BIT) != 0)
            {
                writer.Write(_crit);
                anythingWritten = true;
            }

            if ((dirtyBits & DAMAGE_COLOR_INDEX_DIRTY_BIT) != 0)
            {
                writer.Write(_damageColorIndex);
                anythingWritten = true;
            }

            if ((dirtyBits & DAMAGE_TYPE_DIRTY_BIT) != 0)
            {
                writer.WriteDamageType(_damageType);
                anythingWritten = true;
            }

            return anythingWritten || initialState;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            uint dirtyBits = initialState ? ~0u : reader.ReadPackedUInt32();
            
            if ((dirtyBits & PROC_CHAIN_MASK_DIRTY_BIT) != 0)
            {
                _procChainMask = reader.ReadProcChainMask();
            }

            if ((dirtyBits & PROC_COEFFICIENT_DIRTY_BIT) != 0)
            {
                _procCoefficient = reader.ReadSingle();
            }

            if ((dirtyBits & DAMAGE_DIRTY_BIT) != 0)
            {
                _damage = reader.ReadSingle();
            }

            if ((dirtyBits & FORCE_DIRTY_BIT) != 0)
            {
                _force = reader.ReadSingle();
            }

            if ((dirtyBits & CRIT_DIRTY_BIT) != 0)
            {
                _crit = reader.ReadBoolean();
            }

            if ((dirtyBits & DAMAGE_COLOR_INDEX_DIRTY_BIT) != 0)
            {
                _damageColorIndex = reader.ReadDamageColorIndex();
            }

            if ((dirtyBits & DAMAGE_TYPE_DIRTY_BIT) != 0)
            {
                _damageType = reader.ReadDamageType();
            }

            refreshValues();
        }
    }
}
