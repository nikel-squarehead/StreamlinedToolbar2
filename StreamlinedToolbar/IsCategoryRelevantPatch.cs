using HarmonyLib;

namespace StreamlinedToolbar
{
    // This patch ensures our new tabs are added to the toolbar.
    // (This is only needed for those panel classes where the default implementation explicitly lists permitted panels).

    [HarmonyPatch(typeof(BeautificationGroupPanel), "IsCategoryRelevant")]
    class IsCategoryRelevantPatch
    {
        [HarmonyPostfix]
        public static void Postfix(string category, ref bool __result)
        {
            if (!__result && category == "BeautificationWaterStructures")
            {
                __result = true;
            }
        }
    }
}
