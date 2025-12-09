using HarmonyLib;
using Microsoft.Win32;
using UnityEngine;

namespace StreamlinedToolbar
{
    // This patch makes some changes to asset sorting:
    // (1) Trees are always sorted by height, shortest to tallest
    // (2) Buildings in certain panels (e.g. police, healthcare) are pre-sorted by their function.

    enum BuildingCategoryGroup
    {
        Healthcare,
        Education,
        Police,
        Fire,
        ParksAndRec,

        Unrecognized
    }

    enum BuildingCategory
    {
        Hospital,
        HospitalWithHeli,
        Cemetery,
        Childcare,
        Eldercare,
        Sauna,

        FireStation,
        FirewatchTower,
        FireStationWithHeli,

        PoliceStation,
        Prison,
        PoliceStationWithHeli,

        ElementarySchool,
        HighSchool,
        University,
        Library,

        Parks,
        Piers,

        Unrecognized
    }

    class SortUtils
    {
        private static int toComparisonInt(bool b)
        {
            return b ? -1 : 1;
        }

        public static BuildingCategoryGroup GetBuildingCategoryGroup(BuildingCategory cat)
        {
            switch (cat)
            {
                case BuildingCategory.Hospital:
                case BuildingCategory.HospitalWithHeli:
                case BuildingCategory.Cemetery:
                case BuildingCategory.Childcare:
                case BuildingCategory.Eldercare:
                case BuildingCategory.Sauna:
                    return BuildingCategoryGroup.Healthcare;
                case BuildingCategory.FireStation:
                case BuildingCategory.FirewatchTower:
                case BuildingCategory.FireStationWithHeli:
                    return BuildingCategoryGroup.Fire;
                case BuildingCategory.PoliceStation:
                case BuildingCategory.Prison:
                case BuildingCategory.PoliceStationWithHeli:
                    return BuildingCategoryGroup.Police;
                case BuildingCategory.ElementarySchool:
                case BuildingCategory.HighSchool:
                case BuildingCategory.University:
                case BuildingCategory.Library:
                    return BuildingCategoryGroup.Education;
                case BuildingCategory.Parks:
                case BuildingCategory.Piers:
                    return BuildingCategoryGroup.ParksAndRec;
                case BuildingCategory.Unrecognized:
                default:
                    return BuildingCategoryGroup.Unrecognized;
            }
        }

        public static BuildingCategory GetBuildingCategory(BuildingInfo info)
        {
            var ai = info.m_buildingAI;

            if (ai is HospitalAI)
            {
                return BuildingCategory.Hospital;
            }

            if (ai is CemeteryAI)
            {
                return BuildingCategory.Cemetery;
            }

            if (ai is ChildcareAI)
            {
                return BuildingCategory.Childcare;
            }

            if (ai is EldercareAI)
            {
                return BuildingCategory.Eldercare;
            }

            if (ai is SaunaAI)
            {
                return BuildingCategory.Sauna;
            }

            if (ai is FireStationAI)
            {
                return BuildingCategory.FireStation;
            }

            if (ai is FirewatchTowerAI)
            {
                return BuildingCategory.FirewatchTower;
            }

            if (ai is PoliceStationAI)
            {
                if (info.GetClassLevel() == ItemClass.Level.Level4)
                {
                    return BuildingCategory.Prison;
                }
                else
                {
                    return BuildingCategory.PoliceStation;
                }
            }

            if (ai is LibraryAI)
            {
                return BuildingCategory.Library;
            }

            if (ai is SchoolAI)
            {
                if (ai.GetEducationLevel1())
                {
                    return BuildingCategory.ElementarySchool;
                }
                else if (ai.GetEducationLevel2())
                {
                    return BuildingCategory.HighSchool;
                }
                else if (ai.GetEducationLevel3())
                {
                    return BuildingCategory.University;
                }
            }

            if (ai is HelicopterDepotAI)
            {
                switch (info.GetService())
                {
                    case ItemClass.Service.PoliceDepartment:
                        return BuildingCategory.PoliceStationWithHeli;

                    case ItemClass.Service.HealthCare:
                        return BuildingCategory.HospitalWithHeli;

                    case ItemClass.Service.FireDepartment:
                        return BuildingCategory.FireStationWithHeli;
                }
            }

            if (info.GetService() == ItemClass.Service.Beautification)
            {
                if (info.m_placementMode == BuildingInfo.PlacementMode.Shoreline)
                {
                    return BuildingCategory.Piers;
                }
                else
                {
                    return BuildingCategory.Parks;
                }
            }

            return BuildingCategory.Unrecognized;
        }

        //! returns 0 if items can't be sorted
        public static int SortBuildings(BuildingInfo first, BuildingInfo second)
        {
            BuildingCategory firstCat = GetBuildingCategory(first);
            BuildingCategory secondCat = GetBuildingCategory(second);

            if (firstCat == secondCat)
            {
                if (firstCat == BuildingCategory.Parks)
                {
                    // Sort parks by area
                    int firstArea = Utils.CalculateArea(first, true);
                    int secondArea = Utils.CalculateArea(second, true);

                    if (firstArea != secondArea)
                    {
                        return toComparisonInt(firstArea < secondArea);
                    }
                    // Equal area -> use default sorting
                }

                // Same category -> use default sorting
                return 0;
            }

            if (GetBuildingCategoryGroup(firstCat) != GetBuildingCategoryGroup(secondCat))
            {
                // Not comparable -> use default sorting
                return 0;
            }

            return toComparisonInt(firstCat < secondCat);
        }

        public static float GetAverageTreeHeight(TreeInfo info)
        {
            return info.m_generatedInfo.m_size.y * (info.m_maxScale + info.m_minScale) / 2.0f;
        }

        public static int SortTrees(TreeInfo first, TreeInfo second)
        {
            return toComparisonInt(GetAverageTreeHeight(first) < GetAverageTreeHeight(second));
        }
    }

    [HarmonyPatch(typeof(GeneratedScrollPanel), "ItemsGenericSort")]
    class ItemsGenericSortPatch
    {
        [HarmonyPrefix]
        static bool Prefix(PrefabInfo a, PrefabInfo b, ref int __result)
        {
            if (!Mod.IsInGame())
            {
                return true; // use default sorting
            }

            if (a.m_isCustomContent != b.m_isCustomContent)
            {
                return true; // use default sorting
            }

            var firstTreeInfo = a as TreeInfo;
            if (firstTreeInfo)
            {
                var secondTreeInfo = b as TreeInfo;
                if (secondTreeInfo)
                {
                    __result = SortUtils.SortTrees(firstTreeInfo, secondTreeInfo);
                    return false;
                }
            }

            var firstBuildingInfo = a as BuildingInfo;
            if (firstBuildingInfo)
            {
                var secondBuildingInfo = b as BuildingInfo;
                if (secondBuildingInfo)
                {
                    int result = SortUtils.SortBuildings(firstBuildingInfo, secondBuildingInfo);

                    if (result == 0)
                    {
                        return true; // use default sorting
                    }
                    else
                    {
                        __result = result;
                        return false;
                    }
                }
            }

            return true; // use default sorting
        }
    }
}