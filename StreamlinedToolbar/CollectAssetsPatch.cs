using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GeneratedGroupPanel;

namespace StreamlinedToolbar
{
    [HarmonyPatch(typeof(GeneratedGroupPanel), "CollectAssets")]
    class CollectAssetsPatch
    {
        //
        // These patches are a compatibility workaround for the Better Road Toolbar mod for non-Snowfall DLC owners.
        //
        // BetterRoadToolbar wipes all tabs from the Roads group, then creates
        // custom road tabs and re-creates tabs for any leftover assets.
        // When re-creating those tabs it uses the built-in asset category, so is not aware
        // of our custom override for car parks.
        //
        // Without this patch, the "RoadsMaintenance" tab (which we use for car parks,
        // and which are part of the base game) would not be created unless Snowfall DLC is enabled
        // (it adds the tab for the maintenance depot) or some Workshop assets hard-coded to that category
        // are present.

        // This postfix runs ahead of the Better Road Toolbar's
        [HarmonyPostfix]
        [HarmonyPriority(Priority.First)]
        public static void Postfix1(GroupFilter filter,
                           Comparison<GroupInfo> comparison,
                           ref PoolList<GroupInfo> __result,
                           GeneratedGroupPanel __instance,
                           out GroupInfo __state)
        {
            // Save the "RoadsMaintenance" tab if present.

            if (!filter.IsFlagSet(GroupFilter.Net) ||
                __instance.service != ItemClass.Service.Road ||
                !Mod.IsInGame())
            {
                __state = null;
                return;
            }

            foreach (var group in __result)
            {
                if (group.name == "RoadsMaintenance")
                {
                    __state = group;
                    return;
                }
            }

            // Not expecting to end up here
            __state = null;
        }

        // This postfix runs after the Better Road Toolbar's
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        public static void Postfix2(GroupFilter filter,
                                   Comparison<GroupInfo> comparison,
                                   ref PoolList<GroupInfo> __result,
                                   GeneratedGroupPanel __instance,
                                   GroupInfo __state)
        {
            // Restore the tab if it disappeared.

            if (__state == null)
            {
                return;
            }

            int indexToInsert = -1;

            int index = 1;
            foreach (var group in __result)
            {
                if (group.name == "RoadsMaintenance")
                {
                    return;
                }
                else if (group.name == "RoadsIntersection") // RoadsMaintenance should appear after this tab
                {
                    indexToInsert = index;
                }

                index += 1;
            }

            if (indexToInsert == -1)
            {
                // Not expecting to end up here - this would mean the RoadsIntersection tab is missing
                __result.Add(__state);
            }
            else
            {
                __result.Insert(indexToInsert, __state);
            }
        }
    }
}