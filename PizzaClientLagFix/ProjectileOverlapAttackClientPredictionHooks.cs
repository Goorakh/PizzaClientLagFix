using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PizzaClientLagFix.ModCompat;
using PizzaClientLagFix.Networking;
using RoR2;
using RoR2.Projectile;
using System;
using System.Reflection;
using UnityEngine;

namespace PizzaClientLagFix
{
    static class ProjectileOverlapAttackClientPredictionHooks
    {
        [SystemInitializer(typeof(ProjectileCatalog))]
        static void Init()
        {
            On.RoR2.Projectile.ProjectileOverlapAttack.MyFixedUpdate += ProjectileOverlapAttack_MyFixedUpdate_EnsurePatchDisabled;
            IL.RoR2.Projectile.ProjectileOverlapAttack.MyFixedUpdate += ProjectileOverlapAttack_MyFixedUpdate_AllowClientPrediction;

            addPredictionToProjectile("BrotherUltLineProjectileRotateLeft");
            addPredictionToProjectile("BrotherUltLineProjectileRotateRight");
            addPredictionToProjectile("BrotherUltLineProjectileStatic");
            addPredictionToProjectile("BrotherSunderWave");

            if (UmbralMithrixCompat.Enabled)
            {
                bool failedP4LineLeft = !tryAddPredictionToProjectile("P4UltLineLeft");
                bool failedP4LineRight = !tryAddPredictionToProjectile("P4UltLineRight");
                bool failedStaticUlt = !tryAddPredictionToProjectile("StaticUltLine");

                if (failedP4LineLeft || failedP4LineRight || failedStaticUlt)
                {
                    PluginInfo umbralPluginInfo = UmbralMithrixCompat.UmbralPluginInfo.Value;
                    if (umbralPluginInfo != null)
                    {
                        BaseUnityPlugin umbralPluginInstance = umbralPluginInfo.Instance;
                        if (umbralPluginInstance)
                        {
                            Type umbralPluginType = umbralPluginInstance.GetType();

                            if (failedP4LineLeft)
                                tryAddPredictionToUmbralField("leftP4Line");

                            if (failedP4LineRight)
                                tryAddPredictionToUmbralField("rightP4Line");

                            if (failedStaticUlt)
                                tryAddPredictionToUmbralField("staticUltLine");

                            void tryAddPredictionToUmbralField(string fieldName)
                            {
                                FieldInfo fieldInfo = umbralPluginType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
                                if (fieldInfo == null)
                                {
                                    Log.Error($"Failed to find umbral field '{fieldName}'");
                                    return;
                                }

                                object fieldValue = fieldInfo.GetValue(umbralPluginInstance);
                                if (fieldValue is not GameObject gameObjectFieldValue)
                                {
                                    string valueTypeName = "null";
                                    if (fieldValue != null)
                                    {
                                        valueTypeName = fieldValue.GetType().FullName;
                                    }

                                    Log.Error($"Unexpected type {valueTypeName} of umbral field '{fieldInfo.Name}'");
                                    return;
                                }

                                addPredictionToPrefab(gameObjectFieldValue);
                            }
                        }
                    }
                }
            }

            static void addPredictionToProjectile(string projectileName)
            {
                if (!tryAddPredictionToProjectile(projectileName))
                {
                    Log.Error($"Failed to find projectile '{projectileName}'");
                }
            }

            static bool tryAddPredictionToProjectile(string projectileName)
            {
                int projectileIndex = ProjectileCatalog.FindProjectileIndex(projectileName);
                if (projectileIndex < 0)
                    return false;

                GameObject projectilePrefab = ProjectileCatalog.GetProjectilePrefab(projectileIndex);
                addPredictionToPrefab(projectilePrefab);
                return true;
            }

            static void addPredictionToPrefab(GameObject prefab)
            {
                prefab.AddComponent<ProjectileOverlapAttackValueNetworker>();
                prefab.AddComponent<ProjectileOverlapAttackClientPrediction>();

                Log.Debug($"Added client prediction to {prefab}");
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
                return projectileOverlapAttack.GetComponent<ProjectileOverlapAttackClientPrediction>();
            }

            c.Emit(OpCodes.Stloc, clientPredictionComponentVar);

            int shouldProccessAttacksVar = -1;
            ILLabel afterCanProcessCheckLabel = null;
            if (!c.TryGotoNext(x => x.MatchCallOrCallvirt<ProjectileController>(nameof(ProjectileController.CanProcessCollisionEvents)),
                              x => x.MatchStloc(out shouldProccessAttacksVar),
                              x => x.MatchBr(out afterCanProcessCheckLabel)))
            {
                Log.Error("Failed to find initial patch location");
                return;
            }

            c.Goto(afterCanProcessCheckLabel.Target, MoveType.AfterLabel);

            c.Emit(OpCodes.Ldloc, shouldProccessAttacksVar);
            c.Emit(OpCodes.Ldloc, clientPredictionComponentVar);
            c.EmitDelegate(overrideShouldProcessAttack);
            static bool overrideShouldProcessAttack(bool shouldProccessAttacks, ProjectileOverlapAttackClientPrediction clientPrediction)
            {
                return shouldProccessAttacks || clientPrediction;
            }

            c.Emit(OpCodes.Stloc, shouldProccessAttacksVar);

            if (c.TryGotoNext(MoveType.Before,
                              x => x.MatchCallOrCallvirt<OverlapAttack>(nameof(OverlapAttack.Fire))))
            {
                c.Emit(OpCodes.Ldloc, clientPredictionComponentVar);
                c.EmitDelegate(preFire);
                static void preFire(ProjectileOverlapAttackClientPrediction clientPrediction)
                {
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
