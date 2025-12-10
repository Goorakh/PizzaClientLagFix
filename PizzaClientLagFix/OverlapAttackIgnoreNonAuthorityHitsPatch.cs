using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace PizzaClientLagFix
{
    public static class OverlapAttackIgnoreNonAuthorityHitsPatch
    {
#if DEBUG
        enum DebugMode
        {
            None,
            IgnoreServer,
            IgnoreClient
        }

        static DebugMode _debugMode = DebugMode.None;

        [ConCommand(commandName = "overlap_authority_debug_mode")]
        static void CCSetDebugMode(ConCommandArgs args)
        {
            _debugMode = args.GetArgEnum<DebugMode>(0);

            UnityEngine.Debug.Log($"Overlap authority debug mode: {_debugMode}");
        }
#endif

        public static bool Enabled;

        [SystemInitializer]
        static void Init()
        {
            On.RoR2.OverlapAttack.ProcessHits += OverlapAttack_ProcessHits;
        }

        static void OverlapAttack_ProcessHits(On.RoR2.OverlapAttack.orig_ProcessHits orig, OverlapAttack self, List<OverlapAttack.OverlapInfo> hitList)
        {
            if (!Enabled)
            {
                orig(self, hitList);
                return;
            }
            
#if DEBUG
            switch (_debugMode)
            {
                case DebugMode.None:
                    break;
                case DebugMode.IgnoreServer:
                    if (NetworkServer.active)
                        return;

                    break;
                case DebugMode.IgnoreClient:
                    if (!NetworkServer.active)
                        return;

                    break;
                default:
                    throw new NotImplementedException($"Debug mode {_debugMode} is not implemented");
            }
#endif

            try
            {
                if (hitList.Count > 0)
                {
                    Log.Debug($"OverlapAttack: attacker={self.attacker}, inflictor={self.inflictor}, damage={self.damage}, damageType={self.damageType}");
                }

                for (int i = hitList.Count - 1; i >= 0; i--)
                {
                    HurtBox hurtBox = hitList[i].hurtBox;
                    if (!hurtBox)
                        continue;

                    HealthComponent healthComponent = hurtBox.healthComponent;
                    if (!healthComponent)
                        continue;

                    if (!Util.HasEffectiveAuthority(healthComponent.gameObject))
                    {
#if DEBUG
                        LocalUser localUser = null;
                        NetworkUser networkUser = Util.LookUpBodyNetworkUser(healthComponent.gameObject);
                        if (networkUser)
                        {
                            localUser = networkUser.localUser;
                        }

                        Log.Debug($"Removing hit {Util.GetBestBodyName(healthComponent.gameObject)} ({healthComponent.netId}) (localUser={localUser?.id}): not authority");
#endif

                        hitList.RemoveAt(i);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error_NoCallerPrefix(e);
            }

            orig(self, hitList);
        }
    }
}
