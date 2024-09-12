using Mono.Cecil.Cil;
using MonoMod.Cil;
using PizzaClientLagFix.Networking;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace PizzaClientLagFix
{
    static class ProjectileOverlapAttackClientPredictionHooks
    {
        [SystemInitializer(typeof(ProjectileCatalog))]
        static void Init()
        {
            On.RoR2.Projectile.ProjectileOverlapAttack.MyFixedUpdate += ProjectileOverlapAttack_MyFixedUpdate_EnsurePatchDisabled;
            IL.RoR2.Projectile.ProjectileOverlapAttack.MyFixedUpdate += ProjectileOverlapAttack_MyFixedUpdate_AllowClientPrediction;

            tryAddToProjectile("BrotherUltLineProjectileRotateLeft");
            tryAddToProjectile("BrotherUltLineProjectileRotateRight");
            tryAddToProjectile("BrotherSunderWave");

            static void tryAddToProjectile(string projectileName)
            {
                int projectileIndex = ProjectileCatalog.FindProjectileIndex(projectileName);
                if (projectileIndex >= 0)
                {
                    GameObject projectilePrefab = ProjectileCatalog.GetProjectilePrefab(projectileIndex);
                    projectilePrefab.AddComponent<ProjectileOverlapAttackClientPrediction>();
                }
                else
                {
                    Log.Error($"Failed to find projectile '{projectileName}'");
                }
            }
        }

        static void ProjectileOverlapAttack_MyFixedUpdate_EnsurePatchDisabled(On.RoR2.Projectile.ProjectileOverlapAttack.orig_MyFixedUpdate orig, ProjectileOverlapAttack self, float deltaTime)
        {
            try
            {
                orig(self, deltaTime);
            }
            finally
            {
                OverlapAttackIgnoreNonAuthorityHitsPatch.Enabled = false;
            }
        }

        static void ProjectileOverlapAttack_MyFixedUpdate_AllowClientPrediction(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            VariableDefinition clientPredictionComponentVar = new VariableDefinition(il.Import(typeof(ProjectileOverlapAttackClientPrediction)));
            il.Method.Body.Variables.Add(clientPredictionComponentVar);

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate(getClientPrediction);
            static ProjectileOverlapAttackClientPrediction getClientPrediction(ProjectileOverlapAttack projectileOverlapAttack)
            {
                return !NetworkServer.active && NetworkClient.active ? projectileOverlapAttack.GetComponent<ProjectileOverlapAttackClientPrediction>() : null;
            }

            c.Emit(OpCodes.Stloc, clientPredictionComponentVar);

            if (c.TryGotoNext(MoveType.After,
                              x => x.MatchCallOrCallvirt<ProjectileController>(nameof(ProjectileController.CanProcessCollisionEvents))))
            {
                c.Emit(OpCodes.Ldloc, clientPredictionComponentVar);
                c.EmitDelegate(overrideShouldProcess);
                static bool overrideShouldProcess(bool canProcess, ProjectileOverlapAttackClientPrediction clientPrediction)
                {
                    return canProcess || clientPrediction;
                }
            }
            else
            {
                Log.Error("Failed to find process events patch location");
            }

            if (c.TryGotoNext(MoveType.Before,
                              x => x.MatchCallOrCallvirt<OverlapAttack>(nameof(OverlapAttack.Fire))))
            {
                c.Emit(OpCodes.Ldloc, clientPredictionComponentVar);
                c.EmitDelegate(preFire);
                static void preFire(ProjectileOverlapAttackClientPrediction clientPrediction)
                {
                    if (clientPrediction)
                    {
                        clientPrediction.UpdateClientAttackInfo();
                    }

                    OverlapAttackIgnoreNonAuthorityHitsPatch.Enabled = clientPrediction;
                }

                c.Index++;

                c.EmitDelegate(postFire);
                static void postFire()
                {
                    OverlapAttackIgnoreNonAuthorityHitsPatch.Enabled = false;
                }
            }
            else
            {
                Log.Error("Failed to find fire patch location");
            }
        }
    }
}
