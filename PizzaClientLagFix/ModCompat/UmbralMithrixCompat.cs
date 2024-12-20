using BepInEx;
using BepInEx.Bootstrap;
using System;

namespace PizzaClientLagFix.ModCompat
{
    static class UmbralMithrixCompat
    {
        const string UMBRAL_GUID = "com.Nuxlar.UmbralMithrix";

        public static bool Enabled => Chainloader.PluginInfos.ContainsKey(UMBRAL_GUID);

        public static readonly Lazy<PluginInfo> UmbralPluginInfo = new Lazy<PluginInfo>(() =>
        {
            return Chainloader.PluginInfos.TryGetValue(UMBRAL_GUID, out PluginInfo pluginInfo) ? pluginInfo : null;
        });
    }
}
