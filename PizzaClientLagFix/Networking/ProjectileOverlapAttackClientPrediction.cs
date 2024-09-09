using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace PizzaClientLagFix.Networking
{
    [DisallowMultipleComponent]
    public class ProjectileOverlapAttackClientPrediction : NetworkBehaviour
    {
        ProjectileController _projectileController;

        ProjectileOverlapAttack _projectileOverlapAttack;

        [SyncVar]
        bool _overlapAttackInfoHasValue;

        [SyncVar(hook = nameof(syncOverlapAttackInfo))]
        OverlapAttackInfo _overlapAttackInfo;

        void Awake()
        {
            _projectileController = GetComponent<ProjectileController>();
            _projectileOverlapAttack = GetComponent<ProjectileOverlapAttack>();

            _projectileController.onInitialized += onProjectileInitialized;
        }

        void OnEnable()
        {
            if (NetworkServer.active)
            {
                updateServerAttackInfo();
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (_overlapAttackInfoHasValue)
            {
                UpdateClientAttackInfo();
            }
        }

        void onProjectileInitialized(ProjectileController projectileController)
        {
            if (NetworkServer.active)
            {
                if (_projectileOverlapAttack.attack == null && !_projectileOverlapAttack.enabled)
                {
                    // !=============! EVIL HACK !=============!
                    // For some projectiles, Start isn't called
                    // yet since the component starts out inactive.
                    // But we need the OverlapAttack right now
                    // in order to send it to clients in time
                    _projectileOverlapAttack.Start();
                }

                updateServerAttackInfo();
            }
        }

        void Update()
        {
            if (NetworkServer.active)
            {
                updateServerAttackInfo();
            }
        }

        [Server]
        void updateServerAttackInfo()
        {
            if (!_projectileOverlapAttack || _projectileOverlapAttack.attack == null)
                return;

            _overlapAttackInfo = new OverlapAttackInfo(_projectileOverlapAttack.attack);
            _overlapAttackInfoHasValue = true;
        }

        [Client]
        public void UpdateClientAttackInfo()
        {
            if (NetworkServer.active || !_projectileOverlapAttack)
                return;

            if (!_overlapAttackInfoHasValue)
            {
                Log.Warning($"Cannot update client attack info: Nothing has been received from the server");
                return;
            }

            _projectileOverlapAttack.attack ??= new OverlapAttack();
            _overlapAttackInfo.ApplyTo(_projectileOverlapAttack.attack);

#if DEBUG
            Log.Debug($"Updated client overlap attack, damage={_projectileOverlapAttack.attack.damage}");
#endif
        }

        void syncOverlapAttackInfo(OverlapAttackInfo overlapAttackInfo)
        {
            _overlapAttackInfo = overlapAttackInfo;

            if (!NetworkServer.active)
            {
                _overlapAttackInfoHasValue = true;
                UpdateClientAttackInfo();
            }
        }
    }
}
