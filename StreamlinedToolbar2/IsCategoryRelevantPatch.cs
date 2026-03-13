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
    
    // This patch prevents the Central Library from Modern History CCP from showing up under Unique Buildings tab.
    // It should only show up under Educations tab.
    [HarmonyPatch(typeof(MonumentsGroupPanel), "IsCategoryRelevant")]
    class IsCategoryRelevantForMonumentsPatch
    {
        [HarmonyPostfix]
        public static void Postfix(string category, ref bool __result)
        {
            if (__result && category == "EducationDefault")
            {
                __result = false;
            }
        }
    }

    // EXPERIMENTAL!
    // This patch handles parking lot custom assets from not showing up in Public Transport group which is haphazardly categorized by asset creators
    [HarmonyPatch(typeof(PublicTransportGroupPanel), "IsCategoryRelevant")]
    class IsCategoryRelevantForPublicTransportPatch
    {
        [HarmonyPostfix]
        public static void Postfix(string category, ref bool __result)
        {
            if (__result && category == "RoadsMaintenance")
            {
                __result = false;
            }
        }
    }
}
