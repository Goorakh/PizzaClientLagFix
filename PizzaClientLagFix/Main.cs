using BepInEx;
using Rewired.ComponentControls.Effects;
using RoR2.Projectile;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;

namespace PizzaClientLagFix
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "PizzaClientLagFix";
        public const string PluginVersion = "1.0.0";

        internal static Main Instance { get; private set; }

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            Instance = SingletonHelper.Assign(Instance, this);

            On.RoR2.Projectile.ProjectileSimple.Awake += ProjectileSimple_Awake;
            On.RoR2.Projectile.ProjectileOverlapAttack.FixedUpdate += ProjectileOverlapAttack_FixedUpdate;

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        void OnDestroy()
        {
            Instance = SingletonHelper.Unassign(Instance, this);

            On.RoR2.Projectile.ProjectileSimple.Awake -= ProjectileSimple_Awake;
            On.RoR2.Projectile.ProjectileOverlapAttack.FixedUpdate -= ProjectileOverlapAttack_FixedUpdate;
        }

        static void ProjectileSimple_Awake(On.RoR2.Projectile.ProjectileSimple.orig_Awake orig, ProjectileSimple self)
        {
            orig(self);

            if (self.GetComponent<ProjectileOverlapAttack>() && self.GetComponent<RotateAroundAxis>())
            {
                self.gameObject.AddComponent<ProjectileOverlapAttackClientPrediction>();
            }
        }

        static void ProjectileOverlapAttack_FixedUpdate(On.RoR2.Projectile.ProjectileOverlapAttack.orig_FixedUpdate orig, ProjectileOverlapAttack self)
        {
            ProjectileOverlapAttackClientPrediction clientPrediction = self.GetComponent<ProjectileOverlapAttackClientPrediction>();

            OverlapAttackIgnoreNonAuthorityHitsPatch.Enabled = clientPrediction;
            try
            {
                orig(self);
            }
            finally
            {
                OverlapAttackIgnoreNonAuthorityHitsPatch.Enabled = false;
            }

            if (clientPrediction && !NetworkServer.active && NetworkClient.active)
            {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                if (self.resetInterval >= 0f)
                {
                    self.resetTimer -= Time.fixedDeltaTime;
                    if (self.resetTimer <= 0f)
                    {
                        self.resetTimer = self.resetInterval;
                        self.ResetOverlapAttack();
                    }
                }

                self.fireTimer -= Time.fixedDeltaTime;

                if (self.fireTimer <= 0f)
                {
                    self.fireTimer = 1f / self.fireFrequency;

                    clientPrediction.SetOverlapAttackValuesFromServer(self.attack);

                    OverlapAttackIgnoreNonAuthorityHitsPatch.Enabled = true;
                    try
                    {
                        self.attack.Fire(null);
                    }
                    finally
                    {
                        OverlapAttackIgnoreNonAuthorityHitsPatch.Enabled = false;
                    }
                }
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
            }
        }
    }
}
