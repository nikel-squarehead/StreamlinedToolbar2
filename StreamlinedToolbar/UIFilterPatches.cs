using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StreamlinedToolbar
{
    // This patch disables the 'Metro', 'Train' and 'Hub' filters that are displayed in all tabs (normally the "CCP" tab)
    // containing a station from Train Stations DLC.
    [HarmonyPatch]
    public static class UIFilterPatches
    {
        // GeneratedScrollPanel.UIFilterType is a private nested type, so the usual patching approach can't be used.
        public static MethodBase TargetMethod()
        {
            Type UIFilterType = typeof(GeneratedScrollPanel).GetNestedType("UIFilterType", BindingFlags.NonPublic);
            var c = AccessTools.GetDeclaredConstructors(UIFilterType);
            return c[0];
        }

        [HarmonyPrefix]
        public static void ConstructorPrefix(string name, ref object filterTypeMethod)
        {
            if (name.StartsWith("CCP7"))
            {
                filterTypeMethod = new Func<object, bool>(obj => false);
            }
        }
    }

}
