using HarmonyLib;
using System;

namespace StreamlinedToolbar
{
    // This patch allows apple trees and other hidden vanilla trees to be included in the Beautification > Trees toolbar.
    [HarmonyPatch(typeof(GeneratedScrollPanel), "IsPlacementRelevant", new Type[] { typeof(TreeInfo) })]
    class IsPlacementRelevantPatch
    {
        [HarmonyPrefix]
        static bool Prefix(TreeInfo info, ref bool __result)
        {
            // "Vanilla" cherry tree is missing a thumbnail so skip it.
            // It happens not to have a UI category (i.e. Default).
            if (Mod.IsInGame() && info.category != "Default")
            {
                __result = true;
                return false; // skip default implementation
            }

            return true;
        }
    }
}
