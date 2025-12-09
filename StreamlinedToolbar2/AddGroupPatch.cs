using ColossalFramework;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using static GeneratedGroupPanel;

namespace StreamlinedToolbar
{
    // This patch ensures the tabs in the toolbar match our items' (modified) categories.

    [HarmonyPatch(typeof(GeneratedGroupPanel), "AddGroup")]
    class AddGroupPatch
    {
        // Need access to these protected methods
        private static MethodInfo isCategoryRelevantMethod = AccessTools.Method(typeof(GeneratedGroupPanel), "IsCategoryRelevant");
        private static MethodInfo createGroupInfoMethod = AccessTools.Method(typeof(GeneratedGroupPanel), "CreateGroupInfo", new Type[] { typeof(string), typeof(PrefabInfo) });
        private static MethodInfo updateGroupInfoMethod = AccessTools.Method(typeof(GeneratedGroupPanel), "UpdateGroupInfo", new Type[] { typeof(GroupInfo), typeof(PrefabInfo) });

        [HarmonyPrefix]
        public static bool Prefix(ref PoolList<GroupInfo> groupItems, PrefabInfo info, GeneratedGroupPanel __instance)
        {
            string categoryOverride = Utils.GetCategoryOverride(info);
            if (categoryOverride == null)
            {
                return true; // use default implementaion
            }

            string category = categoryOverride != null ? categoryOverride : info.category;

            //  Equivalent call:
            //  bool isRelevant =  __instance.isCategoryRelevant(info.category);
            bool isRelevant = (bool)isCategoryRelevantMethod.Invoke(__instance, new object[] { category });
            if (!isRelevant)
            {
                return false;
            }

            GroupInfo groupInfo = null;
            for (int i = 0; i < groupItems.Count; i++)
            {
                if (groupItems[i].name == category)
                {
                    groupInfo = groupItems[i];
                    break;
                }
            }

            if (groupInfo == null)
            {
                //  Equivalent call:
                // groupItems.Add(__instance.CreateGroupInfo(category, info));
                groupItems.Add((GroupInfo)createGroupInfoMethod.Invoke(__instance, new object[] { category, info }));
            }
            else
            {
                //  Equivalent call:
                // __instance.UpdateGroupInfo(groupInfo, info);
                updateGroupInfoMethod.Invoke(__instance, new object[] { groupInfo, info });
            }

            return false; // skip default implementation
        }
    }
}
