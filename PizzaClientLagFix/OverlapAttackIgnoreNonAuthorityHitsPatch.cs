using RoR2;
using System;
using System.Collections.Generic;

namespace PizzaClientLagFix
{
    public static class OverlapAttackIgnoreNonAuthorityHitsPatch
    {
        public static bool Enabled;

        [SystemInitializer]
        static void Init()
        {
            On.RoR2.OverlapAttack.ProcessHits += OverlapAttack_ProcessHits;
        }

        static void OverlapAttack_ProcessHits(On.RoR2.OverlapAttack.orig_ProcessHits orig, OverlapAttack self, object boxedHitList)
        {
            if (!Enabled)
            {
                orig(self, boxedHitList);
                return;
            }

            try
            {
                List<OverlapAttack.OverlapInfo> hitList = (List<OverlapAttack.OverlapInfo>)boxedHitList;

                for (int i = hitList.Count - 1; i >= 0; i--)
                {
                    HurtBox hurtBox = hitList[i].hurtBox;
                    if (!hurtBox)
                        continue;

                    HealthComponent healthComponent = hurtBox.healthComponent;
                    if (!healthComponent)
                        continue;

                    if (!healthComponent.hasAuthority)
                    {
#if DEBUG
                        Log.Debug($"Removing hit {Util.GetBestBodyName(healthComponent.gameObject)} ({healthComponent.netId}): not authority");
#endif

                        hitList.RemoveAt(i);
                    }
                }

                orig(self, hitList);
            }
            catch (Exception e)
            {
                Log.Error_NoCallerPrefix(e);
                orig(self, boxedHitList);
            }
        }
    }
}
