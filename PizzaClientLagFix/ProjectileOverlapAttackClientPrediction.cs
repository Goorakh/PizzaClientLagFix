using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace PizzaClientLagFix
{
    class ProjectileOverlapAttackClientPrediction : NetworkBehaviour
    {
        GameObject _attacker;
        NetworkInstanceId _attackerNetId;

        const uint ATTACKER_DIRTY_BIT = 1 << 0;

        public GameObject Attacker
        {
            get
            {
                return _attacker;
            }
            set
            {
                SetSyncVarGameObject(value, ref _attacker, ATTACKER_DIRTY_BIT, ref _attackerNetId);
            }
        }

        GameObject _inflictor;
        NetworkInstanceId _inflictorNetId;

        const uint INFLICTOR_DIRTY_BIT = 1 << 1;

        public GameObject Inflictor
        {
            get
            {
                return _inflictor;
            }
            set
            {
                SetSyncVarGameObject(value, ref _inflictor, INFLICTOR_DIRTY_BIT, ref _inflictorNetId);
            }
        }

        ProcChainMask _procChainMask;
        const uint PROC_CHAIN_MASK_DIRTY_BIT = 1 << 2;
        public ProcChainMask ProcChainMask
        {
            get
            {
                return _procChainMask;
            }
            set
            {
                SetSyncVar(value, ref _procChainMask, PROC_CHAIN_MASK_DIRTY_BIT);
            }
        }

        float _procCoefficient;
        const uint PROC_COEFFICIENT_DIRTY_BIT = 1 << 3;
        public float ProcCoefficient
        {
            get
            {
                return _procCoefficient;
            }
            set
            {
                SetSyncVar(value, ref _procCoefficient, PROC_COEFFICIENT_DIRTY_BIT);
            }
        }

        sbyte _teamIndex;
        const uint TEAM_INDEX_DIRTY_BIT = 1 << 4;
        public TeamIndex TeamIndex
        {
            get
            {
                return (TeamIndex)_teamIndex;
            }
            set
            {
                SetSyncVar((sbyte)value, ref _teamIndex, TEAM_INDEX_DIRTY_BIT);
            }
        }

        float _damage;
        const uint DAMAGE_DIRTY_BIT = 1 << 5;
        public float Damage
        {
            get
            {
                return _damage;
            }
            set
            {
                SetSyncVar(value, ref _damage, DAMAGE_DIRTY_BIT);
            }
        }

        Vector3 _forceVector;
        const uint FORCE_VECTOR_DIRTY_BIT = 1 << 6;
        public Vector3 ForceVector
        {
            get
            {
                return _forceVector;
            }
            set
            {
                SetSyncVar(value, ref _forceVector, FORCE_VECTOR_DIRTY_BIT);
            }
        }

        bool _isCrit;
        const uint IS_CRIT_DIRTY_BIT = 1 << 7;
        public bool IsCrit
        {
            get
            {
                return _isCrit;
            }
            set
            {
                SetSyncVar(value, ref _isCrit, IS_CRIT_DIRTY_BIT);
            }
        }

        byte _damageColorIndex;
        const uint DAMAGE_COLOR_INDEX_DIRTY_BIT = 1 << 8;
        public DamageColorIndex DamageColorIndex
        {
            get
            {
                return (DamageColorIndex)_damageColorIndex;
            }
            set
            {
                SetSyncVar((byte)value, ref _damageColorIndex, DAMAGE_COLOR_INDEX_DIRTY_BIT);
            }
        }

        uint _damageType;
        const uint DAMAGE_TYPE_DIRTY_BIT = 1 << 9;
        public DamageType DamageType
        {
            get
            {
                return (DamageType)_damageType;
            }
            set
            {
                SetSyncVar((uint)value, ref _damageType, DAMAGE_TYPE_DIRTY_BIT);
            }
        }

        int _maximumOverlapTargets;
        const uint MAXIMUM_OVERLAP_TARGETS_DIRTY_BITS = 1 << 10;
        public int MaximumOverlapTargets
        {
            get
            {
                return _maximumOverlapTargets;
            }
            set
            {
                SetSyncVar(value, ref _maximumOverlapTargets, MAXIMUM_OVERLAP_TARGETS_DIRTY_BITS);
            }
        }

        ProjectileController _projectileController;
        ProjectileOverlapAttack _projectileOverlapAttack;
        ProjectileDamage _projectileDamage;

        void Awake()
        {
            _projectileController = GetComponent<ProjectileController>();
            _projectileOverlapAttack = GetComponent<ProjectileOverlapAttack>();
            _projectileDamage = GetComponent<ProjectileDamage>();
        }

        public override void PreStartClient()
        {
            base.PreStartClient();

            if (!_attackerNetId.IsEmpty())
            {
                _attacker = ClientScene.FindLocalObject(_attackerNetId);
            }

            if (!_inflictorNetId.IsEmpty())
            {
                _inflictor = ClientScene.FindLocalObject(_inflictorNetId);
            }
        }

        void Update()
        {
            if (!NetworkServer.active)
                return;

            if (_projectileController)
            {
                Attacker = _projectileController.owner;
                Inflictor = gameObject;
                ProcChainMask = _projectileController.procChainMask;
                TeamIndex = _projectileController.teamFilter.teamIndex;

                if (_projectileOverlapAttack)
                {
                    ProcCoefficient = _projectileController.procCoefficient * _projectileOverlapAttack.overlapProcCoefficient;
                }
            }

            if (_projectileOverlapAttack)
            {
                if (_projectileDamage)
                {
                    Damage = _projectileDamage.damage * _projectileOverlapAttack.damageCoefficient;
                    ForceVector = _projectileOverlapAttack.forceVector + (_projectileDamage.force * transform.forward);
                }

                MaximumOverlapTargets = _projectileOverlapAttack.maximumOverlapTargets;
            }

            if (_projectileDamage)
            {
                IsCrit = _projectileDamage.crit;
                DamageColorIndex = _projectileDamage.damageColorIndex;
                DamageType = _projectileDamage.damageType;
            }
        }

        public void SetOverlapAttackValuesFromServer(OverlapAttack overlapAttack)
        {
            overlapAttack.attacker = _attacker;
            overlapAttack.inflictor = _inflictor;
            overlapAttack.procChainMask = _procChainMask;
            overlapAttack.procCoefficient = _procCoefficient;
            overlapAttack.teamIndex = TeamIndex;
            overlapAttack.damage = _damage;
            overlapAttack.forceVector = _forceVector;
            overlapAttack.isCrit = _isCrit;
            overlapAttack.damageColorIndex = DamageColorIndex;
            overlapAttack.damageType = DamageType;
            overlapAttack.maximumOverlapTargets = _maximumOverlapTargets;
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            if (initialState)
            {
                writer.Write(_attacker);
                writer.Write(_inflictor);
                writer.Write(_procChainMask);
                writer.Write(_procCoefficient);
                writer.Write(_teamIndex);
                writer.Write(_damage);
                writer.Write(_forceVector);
                writer.Write(_isCrit);
                writer.Write(_damageColorIndex);
                writer.WritePackedUInt32(_damageType);
                writer.Write(_maximumOverlapTargets);

                return true;
            }

            uint dirtyBits = syncVarDirtyBits;
            writer.WritePackedUInt32(dirtyBits);

            bool anythingWritten = false;

            if ((dirtyBits & ATTACKER_DIRTY_BIT) != 0)
            {
                writer.Write(_attacker);
                anythingWritten = true;
            }

            if ((dirtyBits & INFLICTOR_DIRTY_BIT) != 0)
            {
                writer.Write(_inflictor);
                anythingWritten = true;
            }

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

            if ((dirtyBits & TEAM_INDEX_DIRTY_BIT) != 0)
            {
                writer.Write(_teamIndex);
                anythingWritten = true;
            }

            if ((dirtyBits & DAMAGE_DIRTY_BIT) != 0)
            {
                writer.Write(_damage);
                anythingWritten = true;
            }

            if ((dirtyBits & FORCE_VECTOR_DIRTY_BIT) != 0)
            {
                writer.Write(_forceVector);
                anythingWritten = true;
            }

            if ((dirtyBits & IS_CRIT_DIRTY_BIT) != 0)
            {
                writer.Write(_isCrit);
                anythingWritten = true;
            }

            if ((dirtyBits & DAMAGE_COLOR_INDEX_DIRTY_BIT) != 0)
            {
                writer.Write(_damageColorIndex);
                anythingWritten = true;
            }

            if ((dirtyBits & DAMAGE_TYPE_DIRTY_BIT) != 0)
            {
                writer.WritePackedUInt32(_damageType);
                anythingWritten = true;
            }

            if ((dirtyBits & MAXIMUM_OVERLAP_TARGETS_DIRTY_BITS) != 0)
            {
                writer.Write(_maximumOverlapTargets);
                anythingWritten = true;
            }

            return anythingWritten;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                _attackerNetId = reader.ReadNetworkId();
                _inflictorNetId = reader.ReadNetworkId();
                _procChainMask = reader.ReadProcChainMask();
                _procCoefficient = reader.ReadSingle();
                _teamIndex = reader.ReadSByte();
                _damage = reader.ReadSingle();
                _forceVector = reader.ReadVector3();
                _isCrit = reader.ReadBoolean();
                _damageColorIndex = reader.ReadByte();
                _damageType = reader.ReadPackedUInt32();
                _maximumOverlapTargets = reader.ReadInt32();

                return;
            }

            uint dirtyBits = reader.ReadPackedUInt32();

            if ((dirtyBits & ATTACKER_DIRTY_BIT) != 0)
            {
                _attacker = reader.ReadGameObject();
            }

            if ((dirtyBits & INFLICTOR_DIRTY_BIT) != 0)
            {
                _inflictor = reader.ReadGameObject();
            }

            if ((dirtyBits & PROC_CHAIN_MASK_DIRTY_BIT) != 0)
            {
                _procChainMask = reader.ReadProcChainMask();
            }

            if ((dirtyBits & PROC_COEFFICIENT_DIRTY_BIT) != 0)
            {
                _procCoefficient = reader.ReadSingle();
            }

            if ((dirtyBits & TEAM_INDEX_DIRTY_BIT) != 0)
            {
                _teamIndex = reader.ReadSByte();
            }

            if ((dirtyBits & DAMAGE_DIRTY_BIT) != 0)
            {
                _damage = reader.ReadSingle();
            }

            if ((dirtyBits & FORCE_VECTOR_DIRTY_BIT) != 0)
            {
                _forceVector = reader.ReadVector3();
            }

            if ((dirtyBits & IS_CRIT_DIRTY_BIT) != 0)
            {
                _isCrit = reader.ReadBoolean();
            }

            if ((dirtyBits & DAMAGE_COLOR_INDEX_DIRTY_BIT) != 0)
            {
                _damageColorIndex = reader.ReadByte();
            }

            if ((dirtyBits & DAMAGE_TYPE_DIRTY_BIT) != 0)
            {
                _damageType = reader.ReadPackedUInt32();
            }

            if ((dirtyBits & MAXIMUM_OVERLAP_TARGETS_DIRTY_BITS) != 0)
            {
                _maximumOverlapTargets = reader.ReadInt32();
            }
        }
    }
}
