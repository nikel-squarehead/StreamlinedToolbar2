using ColossalFramework;
using HarmonyLib;
using System;
using System.Reflection;
using static GeneratedGroupPanel;

namespace StreamlinedToolbar
{
    // This patch skips creation of empty toolbar tabs.
    // (Only needed for those tabs which are created "forcefully". In most cases tabs are created based on there being at least 1 item in a given category.)
    //
    // See also: CreateGroupItemPatch prefix, which skips some other tabs.

    [HarmonyPatch(typeof(BeautificationGroupPanel), "AddCustomGroups")]
    class AddCustomGroupsPatch
    {
        // Need access to this protected method
        private static MethodInfo createGroupMethod = AccessTools.Method(typeof(GeneratedGroupPanel), "CreateGroupInfo", new Type[] { typeof(string), typeof(PrefabInfo) });

        [HarmonyPrefix]
        static bool Prefix(PoolList<GroupInfo> groupItems, BeautificationGroupPanel __instance)
        {
            // Same as default implementation, except we skip P&P Plazas (we always empty that tab).

            // Not sure why these tabs aren't created automatically, like other tabs, if an asset with the corresponding category is found.
            // Perhaps it's because the item's "service" and/or other properties do not match those normally expected of items placed in "BeautificationPanels".

            if (!ToolsModifierControl.toolController.m_mode.IsFlagSet(ItemClass.Availability.AssetEditor))
            {
                groupItems.Add((GroupInfo)createGroupMethod.Invoke(__instance, new[] { "BeautificationProps", null }));
                if (SteamHelper.IsDLCOwned(SteamHelper.DLC.PlazasAndPromenadesDLC))
                {
                    groupItems.Add((GroupInfo)createGroupMethod.Invoke(__instance, new[] { "BeautificationPedestrianZoneEssentials", null }));
                }
                if (SteamHelper.IsDLCOwned(SteamHelper.DLC.HotelDLC) || 
                    SteamHelper.IsDLCOwned(SteamHelper.DLC.ModderPack11)) // also create for the MCM CCP
                {
                    groupItems.Add((GroupInfo)createGroupMethod.Invoke(__instance, new[] { "BeautificationHotels", null }));
                }
            }

            return false; // skip default implementation
        }
    }
}
