using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static GeneratedGroupPanel;

namespace StreamlinedToolbar
{
    [HarmonyPatch(typeof(GeneratedGroupPanel), "CreateGroupItem")]
    class CreateGroupItemPatch
    {
        // This prefix skips creation of certain emptied tabs.
        // Normally, these tabs are created regardless of whether any items belong to them.
        [HarmonyPrefix]
        static bool Prefix(GroupInfo info, string localeID)
        {
            if (Mod.IsInGame())
            {
                if (info.name == "MonumentModderPack" || info.name == "LandscapingModderPackTrees")
                {
                    return false; // skip creation of these tab
                }
            }

            return true;
        }

        private static Dictionary<string, Dictionary<string, string>> translations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "DistrictSpecializationOffice", new Dictionary<string, string>
                {
                    {"en", "Office and High-Rise Buildings"},
                    {"de", "Büro- und Hochhäuser"},
                    {"es", "Edificios de oficinas y de gran altura"},
                    {"fr", "Bureaux et immeubles de grande hauteur"},
                    {"ko", "사무실 및 고층 건물"},
                    {"pl", "Biurowce i wieżowce"},
                    {"pt", "Edifícios de escritórios e arranha-céus"},
                    {"ru", "Офисные и многоэтажные здания"},
                    {"zh", "办公楼和高层建筑"},
                }
            },
            {
                "DistrictSpecializationCommercial", new Dictionary<string, string>
                {
                    {"en", "Retail Buildings"},
                    {"de", "Einzelhandelsgebäude"},
                    {"es", "Edificios comerciales"},
                    {"fr", "Bâtiments commerciaux"},
                    {"ko", "소매 건물"},
                    {"pl", "Budynki handlowe"},
                    {"pt", "Edifícios comerciais"},
                    {"ru", "Торговые здания"},
                    {"zh", "零售楼宇"},
                }
            },
            {
                "BeautificationOthers", new Dictionary<string, string>
                {
                    {"en", "Sports Grounds"},
                    {"de", "Freiluft-Sportstätten"},
                    {"es", "Instalaciones deportivas"},
                    {"fr", "Installations sportives"},
                    {"ko", "야외 스포츠 시설"},
                    {"pl", "Obiekty sportowe"},
                    {"pt", "Instalações desportivas"},
                    {"ru", "Спортивные площадки"},
                    {"zh", "户外运动设施"},
                }
            },
            {
                "MonumentFootball", new Dictionary<string, string>
                {
                    {"en", "Sports Grounds"},
                    {"de", "Freiluft-Sportstätten"},
                    {"es", "Instalaciones deportivas"},
                    {"fr", "Installations sportives"},
                    {"ko", "야외 스포츠 시설"},
                    {"pl", "Obiekty sportowe"},
                    {"pt", "Instalações desportivas"},
                    {"ru", "Спортивные площадки"},
                    {"zh", "户外运动设施"},
                }
            }
        };

        private static void SetTooltip(string category, string fallbackLocaleID, ref UIButton button)
        {
            string language = SingletonLite<LocaleManager>.instance.language;
            if (translations.ContainsKey(category) && translations[category].ContainsKey(language))
            {
                button.tooltip = translations[category][language];
            }
            else
            {
                // Fallback in case we don't have a translation. These shoul be good enough
                button.tooltip = Locale.Get(fallbackLocaleID, category);
            }
        }

        private static void SetIconAndTooltip(string spriteBase, string category, string fallbackLocaleID, ref UIButton button)
        {
            string buttonBaseName = spriteBase + category;
            button.normalFgSprite = buttonBaseName;
            button.focusedFgSprite = buttonBaseName + "Focused";
            button.hoveredFgSprite = buttonBaseName + "Hovered";
            button.pressedFgSprite = buttonBaseName + "Pressed";
            button.disabledFgSprite = buttonBaseName + "Disabled";

            SetTooltip(category, fallbackLocaleID, ref button);
        }

        //
        // This postfix assigns an icon and a name (tooltip) to our custom tabs.
        //
        // This patch would more naturally fit in as a postfix of GeneratedGroupPanel.SpawnButtonEntry, which is where button icons
        // and tooltip are assigned.
        //
        // However:
        // In the implementation of SpawnButtonEntry, there is a call to Type.GetType made with unqualified type names (e.g. Type.GetType("EducationPanel").
        // That call will only work within the same assembly. Patching a method with Harmony generates the patched method in a new assembly,
        // in which the call will fail. This manifests itself in some toolbar tabs not being populated with any items in certain circumstances.
        // In the output_log, it will show up as: "Assembly resolution failure. No assembly named '...Panel' was found."
        // Note that this will happen even if the mod which contains the patch is disabled.
        //
        // In conclusion, SpawnButtonEntry should never be patched, unless by Prefix that always skips the original implementation.
        //
        [HarmonyPostfix]
        static void Postfix(GroupInfo info, string localeID, GeneratedGroupPanel __instance, UITabstrip ___m_Strip, int ___m_ObjectIndex)
        {
            int buttonIndex = ___m_ObjectIndex - 1;

            if (___m_Strip.tabCount <= buttonIndex)
            {
                // Probably should never end up here
                return;
            }

            var button = ___m_Strip.tabs[buttonIndex] as UIButton;

            if (button == null)
            {
                // Probably should never end up here
                return;
            }

            switch (info.name) 
            {
                // Custom tabs
                case "BeautificationWaterStructures":
                    SetIconAndTooltip("SubBar", "LandscapingWaterStructures", localeID, ref button);
                    break;
                case "MonumentsCommercial":
                    SetIconAndTooltip("SubBar", "DistrictSpecializationCommercial", "DISTRICT_CATEGORY", ref button);
                    break;
                case "MonumentsOffice":
                    SetIconAndTooltip("SubBar", "DistrictSpecializationOffice", "DISTRICT_CATEGORY", ref button);
                    break;

                // Renamed vanilla tabs
                case "BeautificationOthers":
                case "MonumentFootball":
                    SetTooltip(info.name, localeID, ref button);
                    break;
                default:
                    return;
            }
        }
    }
}
