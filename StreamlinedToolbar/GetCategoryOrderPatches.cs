using HarmonyLib;

namespace StreamlinedToolbar
{
    // These patches determine the order in which our custom tabs appear in the toolbar.

    [HarmonyPatch(typeof(BeautificationGroupPanel), "GetCategoryOrder")]
    class GetCategoryOrderPatch
    {
        [HarmonyPostfix]
        public static void Postfix(string name, ref int __result)
        {
            if (name == "BeautificationWaterStructures")
            {
                __result = 4;
            }
        }
    }

    [HarmonyPatch(typeof(MonumentsGroupPanel), "GetCategoryOrder")]
    class GetCategoryOrderPatch2
    {
        [HarmonyPostfix]
        public static void Postfix(string name, ref int __result)
        {
            switch (name)
            {
                case "BeautificationHotels":
                    __result = 55;
                    break;
                case "MonumentsCommercial":
                    __result = 60;
                    break;
                case "MonumentsOffice":
                    __result = 65;
                    break;
                case "BeautificationParks":
                    __result = 70;
                    break;
                case "BeautificationPlazas":
                    __result = 71;
                    break;
                case "BeautificationWaterStructures":
                    __result = 75;
                    break;
            }
        }
    }
}
