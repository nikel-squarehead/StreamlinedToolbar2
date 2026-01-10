using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace StreamlinedToolbar
{
    public enum StationType
    {
        Metro,
        Train,
        MultiModal,
        Unrecognized
    }

    internal class Utils
    {
        private static Dictionary<string, bool> LooksLikeCarParkCache = new Dictionary<string, bool>();
        private static Dictionary<string, string> BuildingCategoryOverrideCache = new Dictionary<string, string>();

        public static string GetTreeCategoryOverride(TreeInfo info)
        {
            return "LandscapingTrees";
        }

        // Returns null if no override is requested (which usually means info.category is used).
        public static string GetNetCategoryOverride(NetInfo info)
        {
            if (info.category == "PublicTransportModderPack")
            {
                if (info.GetSubService() == ItemClass.SubService.PublicTransportTrain)
                {
                    return "PublicTransportTrain";
                }
            }
            else if (info.category == "LandscapingModderPack")
            {
                if (info.m_netAI is QuayAI)
                {
                    return "LandscapingWaterStructures";
                }
                else if (info.m_netAI is DecorationWallAI)
                {
                    return "LandscapingFences";
                }
                else
                {
                    return "LandscapingPaths";
                }
            }
            else if (info.category == "RoadsModderPack")
            {
                uint laneCount = GetCarLaneCount(info);

                if ((info.m_netAI as RoadBaseAI).m_highwayRules && !info.m_hasPedestrianLanes)
                {
                    return "RoadsHighway";
                }
                else if (laneCount <= 2)
                {
                    return "RoadsSmall";
                }
                else if (laneCount <= 4)
                {
                    return "RoadsMedium";
                }
                else
                {
                    return "RoadsLarge";
                }
            }

            return null;
        }

        // Returns null if no override is requested (which usually means info.category is used).
        public static string GetPropCategoryOverride(PropInfo info)
        {
            if (info.category == "BeautificationProps")
            {
                // Parks & Plazas -> Props contains some rocks, move those to Landscaping -> Rocks.
                if (info.editorCategory == "PropsRocks")
                {
                    return "LandscapingRocks";
                }
            }

            // All props normally shown in the different Beautification tabs to the "Props" tab
            return "BeautificationProps";
        }

        // Returns null if no override is requested (which usually means info.category is used).
        public static string GetBuildingCategoryOverride(BuildingInfo info)
        {
            if (BuildingCategoryOverrideCache.ContainsKey(info.name))
            {
                return BuildingCategoryOverrideCache[info.name];
            }
            else
            {
                string cat = GetBuildingCategoryOverrideInternal(info);
                BuildingCategoryOverrideCache.Add(info.name, cat);
                return cat;
            }
        }

        // Has the following category sorting logic:
        // 1. Parking lots -> road maintenance
        // 2. Parks & unique buildings on water -> water structures
        // 3. Parks & plazas
        // 4. Unique buildings
        // 5. Service buildings: electricity, garbage, hospital, cemetery, fire station, police station, school, library
        // 6. Roads & intersections
        // 7. Public transportations
        private static string GetBuildingCategoryOverrideInternal(BuildingInfo info)
        {
            // 1. Parking lots -> road maintenance
            if (info.GetService() == ItemClass.Service.Beautification && LooksLikeCarPark(info))
            {
                return "RoadsMaintenance";
            }

            // 2. Parks & unique buildings on water -> water structures
            if (info.GetService() == ItemClass.Service.Beautification || info.GetService() == ItemClass.Service.Monument)
            {
                if (info.category != "BeautificationCityPark" &&
                    info.category != "BeautificationAmusementPark" &&
                    info.category != "BeautificationNatureReserve" &&
                    info.category != "BeautificationZoo")
                {
                    // Shoreside parks and landmarks receive their own tab
                    // except those belonging to the specialised parks - these are kept in their respective tabs.
                    if (info.m_placementMode == BuildingInfo.PlacementMode.Shoreline ||
                        info.m_placementMode == BuildingInfo.PlacementMode.ShorelineOrGround ||
                        info.m_placementMode == BuildingInfo.PlacementMode.OnWater)
                    {
                        return "BeautificationWaterStructures";
                    }
                }
            }

            // 3. Parks & plazas
            if (info.GetService() == ItemClass.Service.Beautification)
            {
                if (info.category == "BeautificationPedestrianZonePlazas")
                {
                    return "BeautificationPlazas";
                }

                if (info.category == "MonumentModderPack")
                {
                    switch (info.m_requiredModderPack)
                    {
                        case SteamHelper.ModderPackBitMask.Pack2: // High-Tech Buildings
                        case SteamHelper.ModderPackBitMask.Pack22: // Mountain Village
                            return "BeautificationExpansion1";
                        case SteamHelper.ModderPackBitMask.Pack17: // Sports Venues
                            return "BeautificationOthers";
                        case SteamHelper.ModderPackBitMask.Pack11: // Mid-century Modern
                            if (info.name.Contains("otel"))
                            {
                                // Hotels and a motel
                                return "BeautificationHotels";
                            }
                            return "BeautificationExpansion1";

                        default:
                            switch (info.name)
                            {
                                // Plazas
                                case "MP26_Park_Concrete":
                                case "MP26_Park_QueenOfHearts":
                                case "ROJ Large Station Front Plaza":
                                case "ROJ Small Station Front Plaza":
                                    return "BeautificationPlazas";

                                // Tourism & leisure
                                case "Botanical Experience 8x5":
                                case "ROJ Small Station Market":
                                    return "BeautificationExpansion1";

                                default:
                                    return "BeautificationProps";
                            }
                    }
                }

                // Various vanilla parks individually re-assigned by name
                switch (info.name)
                {
                    // Parks
                    case "Birch Park 01":
                    case "Palm Park 01":
                    case "Park Pond 01":
                    case "Park Pond 02":
                    case "Tiny Park 01":
                    case "Tiny Playground 01":
                        return "BeautificationParks";

                    // Plazas
                    case "Birthday Plaza 01":
                    case "MerryGoRound":
                    case "Tourist Park 01":
                        return "BeautificationPlazas";

                    // Tourism & leisure
                    case "10thAnniversary Park":
                    case "Botanical garden":
                    case "ChirpyBirthday Balloon Tours":
                    case "Industry Park":
                    case "Transport Park":
                        return "BeautificationExpansion1";

                    // Sports grounds
                    case "Beachvolley Court":
                        return "BeautificationOthers";
                }
            }

            // 4. Unique buildings
            if (info.m_buildingAI is MonumentAI)
            {
                switch (info.m_requiredModderPack)
                {
                    case SteamHelper.ModderPackBitMask.Pack17: // Stadiums
                        return "MonumentFootball";
                    case SteamHelper.ModderPackBitMask.Pack16: // Shopping malls
                        return "MonumentsCommercial";
                    case SteamHelper.ModderPackBitMask.Pack12: // Seaside resorts
                    case SteamHelper.ModderPackBitMask.Pack22: // Mountain Village
                        return "BeautificationHotels";
                    case SteamHelper.ModderPackBitMask.Pack1: // Art Deco
                    case SteamHelper.ModderPackBitMask.Pack13: // Skyscrapers
                    case SteamHelper.ModderPackBitMask.Pack19: // Railroads of Japan
                        return "MonumentsOffice";

                    default:
                        switch (info.name)
                        {
                            // Landmarks
                            case "BNBN_35": // Sahel Monument
                            case "BNBN_37": // Unity Pyramid
                            case "Business Park":
                            case "Cathedral of Plentitude":
                            case "City Arch":
                            case "City Hall":
                            case "Clock Tower":
                            case "Court House":
                            case "Korean Style Temple":
                            case "Monument Landmark Static 2x3": // The Unification Monument
                            case "Monument Landmark Static 8x8": // The Monument of Colossal Heroes
                            case "MP24_UniqueBuilding01_TownHall":
                            case "Observation Tower":
                            case "Oppression Office":
                            case "PDX17_Five Story Pagora":
                            case "Pyramid Of Safety":
                            case "Space Shuttle Launch Site":
                            case "Sphinx Of Scenarios":
                            case "Unique Building 9x9": // The Temple of the Sahel
                            case "Unique Building Level 6 3x2": // Sanctum of Oduduwa
                            case "Ziggurat Garden":
                                return "MonumentLandmarks";

                            // Tourism & leisure
                            case "Academic Library 01":
                            case "Aquarium":
                            case "Aviation Club 01 A":
                            case "BNBN_13": // Bantu Art Museum
                            case "cinema":
                            case "ExpoCenter":
                            case "Festival Area 1":
                            case "Festival Fan Zone":
                            case "Landmark Museum of Post-Modern Art 01":
                            case "Library":
                            case "Live Music Venue":
                            case "Modern Art Museum":
                            case "MP26_Unique_ArtMuseum":
                            case "MP26_Unique_ConcertHall":
                            case "Observatory":
                            case "Opera House":
                            case "Panda Sanctuary":
                            case "ScienceCenter":
                            case "SeaWorld":
                            case "Steam Train":
                            case "Theater of Wonders":
                            case "theatre":
                            case "Traffic Park":
                            case "Unique Building 32x16": // Conference Center
                            case "Unique Building Museum 7x6": // Royal Museum
                            case "Youjoy Entertainment Agency":
                                return "MonumentExpansion1";

                            // Sports grounds
                            case "arena":
                            case "DrivingRange":
                            case "Stadium":
                                return "MonumentFootball";

                            // Hotels
                            case "BNBN_7": // Luxury Hotel #2
                            case "Luxury Hotel 4x4": // Luxury Hotel #1
                            case "LuxuryHotel":
                            case "PDX11_Hotel_kikyo":
                            case "PDX12_CityHotel":
                                return "BeautificationHotels";

                            // Retail buildings
                            case "department_store":
                            case "Dosan Square Center":
                            case "Grand Mall":
                            case "hypermarket":
                            case "Landmark Commercial High 01":
                            case "Landmark Market Hall 01":
                            case "Landmark Shopping Mall 01":
                            case "Mirae Department Store":
                            case "MP24_UniqueBuilding02_PublicMarket":
                            case "MP24_UniqueBuilding03_GeneralStore":
                            case "MP26_Unique_DeptStore":
                            case "Old Market Street":
                            case "PDX01_driveinn_taiheiyo":
                            case "PDX02_driveinn_natori":
                            case "PDX03_Soba Restaurant":
                            case "PDX04_Udon Shop":
                            case "PDX05_Ramen Shop":
                            case "PDX06_Driveinn Large":
                            case "PDX13_Hiroshima_sta":
                            case "PDX14_Shizuoka_Station":
                            case "Posh Mall":
                            case "shopping_center":
                            case "Trash Mall":
                            case "Unique Building Level 1 7x4": // Ego City Market
                                return "MonumentsCommercial";

                            // Office and high-rise buildings
                            case "Acrocastle Apartment Complex":
                            case "BNBN_29": // Communications Center
                            case "Broadcasting Studios":
                            case "Colossal Offices":
                            case "Electric Car Factory":
                            case "High Interest Tower":
                            case "International Trade Building":
                            case "JANGBEESOFT RD Center":
                            case "Landmark Office High 01":
                            case "Landmark Residential High 01":
                            case "Nanotechnology Center":
                            case "PDX07_Cityoffice_M":
                            case "PDX08_Cityoffice_L":
                            case "PDX09_medium_office":
                            case "PDX10_JA_BLDG":
                            case "PDX18_Shinjuku_bldg":
                            case "PDX19_Yokohama-bldg":
                            case "PDX20_Shin-maru":
                            case "Research Center":
                            case "Robotics Institute":
                            case "SeaAndSky Scraper":
                            case "Semiconductor Plant":
                            case "Servicing Services":
                            case "Software Development Studio":
                            case "Television Station":
                            case "Transport Tower":
                            case "Unique Building 24x24": // Saharan Research Institute
                            case "Unique Building 5x5": // The Gold Tower
                            case "Unique Building Level 4 13x8": // Ọrunmila Towers
                            case "Unique Building Level 4 9x5": // The Silver Tower
                                return "MonumentsOffice";

                            // Parks
                            case "Bird and Bee Haven":
                            case "Central Park":
                            case "Climate Research Station":
                            case "Fountain of LifeDeath":
                            case "Friendly Neighborhood":
                            case "Lungs of the City":
                            case "Sparkly Unicorn Rainbow Park":
                                return "BeautificationParks";

                            // Plazas
                            case "Bronze Cow":
                            case "Bronze Panda":
                            case "Chirps Thumbs Up Plaza":
                            case "Disaster Memorial":
                            case "Fancy Fountain":
                            case "Financial Plaza 01":
                            case "Financial Plaza 02":
                            case "Helicopter Park":
                            case "Korean Food Alley":
                            case "Lazaret Plaza":
                            case "Meteorite Park":
                            case "Official Park":
                            case "Plaza of the Dead":
                            case "Statue of Industry":
                            case "Statue of Shopping":
                            case "StatueOfWealth":
                            case "Winter Market 01":
                                return "BeautificationPlazas";

                            default:
                                break;
                        }
                        break;
                }
            }

            // 5. Service buildings: electricity, water pump, garbage, hospital, cemetery, fire station, police station, school, library
            if (info.category == "MonumentModderPack")
            {
                if (info.m_buildingAI is PowerPlantAI)
                {
                    return "ElectricityDefault";
                }
                else if (info.m_buildingAI is WaterFacilityAI)
                {
                    return "WaterServices";
                }
                else if (info.m_buildingAI is LandfillSiteAI)
                {
                    return "Default";
                }
                else if (info.m_buildingAI is HospitalAI)
                {
                    return "HealthcareDefault";
                }
                else if (info.m_buildingAI is CemeteryAI)
                {
                    return "HealthcareDefault";
                }
                else if (info.m_buildingAI is FireStationAI)
                {
                    return "FireDepartmentFire";
                }
                else if (info.m_buildingAI is PoliceStationAI)
                {
                    return "PoliceDefault";
                }
                else if (info.m_buildingAI is SchoolAI)
                {
                    return "EducationDefault";
                }
                else if (info.m_buildingAI is LibraryAI)
                {
                    return "EducationDefault";
                }
            }

            // 6. Roads & intersections
            if (info.category == "RoadsModderPack")
            {
                if (info.m_requiredModderPack == SteamHelper.ModderPackBitMask.Pack27 ||
                    info.m_requiredModderPack == SteamHelper.ModderPackBitMask.Pack23)
                {
                    return "RoadsIntersection";
                }
            }

            // 7. Public transportations
            if (info.category == "PublicTransportModderPack")
            {
                if (info.m_buildingAI is TransportStationAI)
                {
                    StationType? stationType = GetStationType(info);
                    switch (stationType)
                    {
                        case StationType.Metro:
                            return "PublicTransportMetro";
                        case StationType.Train:
                            return "PublicTransportTrain";
                        case StationType.MultiModal:
                            return "PublicTransportHubs";
                        case StationType.Unrecognized:
                        case null:
                            break;
                    }
                }
                else if (info.m_buildingAI is DepotAI)
                {
                    // Railroads of Japan bus depot
                    var depotAi = info.m_buildingAI as DepotAI;
                    switch (depotAi.m_transportInfo.vehicleCategory)
                    {
                        case VehicleInfo.VehicleCategory.Bus:
                            return "PublicTransportBus";
                    }
                }
            }

            return null;
        }

        // Returns null if no override is requested (which usually means info.category is used).
        public static string GetCategoryOverride(PrefabInfo info)
        {
            if (info is BuildingInfo)
            {
                return GetBuildingCategoryOverride(info as BuildingInfo);
            }
            else if (info is PropInfo)
            {
                return GetPropCategoryOverride(info as PropInfo);
            }
            else if (info is NetInfo)
            {
                return GetNetCategoryOverride(info as NetInfo);
            }

            return null;
        }

        private static float APPROX_PARKING_SPACE_LENGTH = 5f; // in metres
        private static float APPROX_PARKING_SPACE_WIDTH = 2.5f; // in metres
        private static float APPROX_PARKING_SPACE_AREA = APPROX_PARKING_SPACE_LENGTH * APPROX_PARKING_SPACE_WIDTH / 64f; // in 'grid units' (which are 8x8m)

        // If 25% of the total park area is taken up by parking spaces, we assume it is a car park.
        // Anything less, we assume it's a normal park with some parking spaces.
        private static float PARKING_AREA_FRACTION_THRESHOLD = 0.25f;

        // Calculates building area (footprint), in grid squares (8x8m each).
        public static int CalculateArea(BuildingInfo info, bool includeSubbuildings)
        {
            int area = info.m_cellLength * info.m_cellWidth;

            if (includeSubbuildings && (info.m_subBuildings != null))
            {
                foreach (var sub in info.m_subBuildings)
                {
                    var subBuildingInfo = sub.m_buildingInfo;
                    if (subBuildingInfo)
                    {
                        // Note: The sub-buildings might overlap, which we don't check, but this should be good enough.
                        area += subBuildingInfo.m_cellLength * subBuildingInfo.m_cellWidth;
                    }
                }
            }

            return area;
        }

        public static bool LooksLikeCarPark(BuildingInfo info)
        {
            if (LooksLikeCarParkCache.ContainsKey(info.name))
            {
                return LooksLikeCarParkCache[info.name];
            }
            else
            {
                bool result = LooksLikeCarParkInternal(info);
                LooksLikeCarParkCache.Add(info.name, result);
                return result;
            }
        }

        private static bool LooksLikeCarParkInternal(BuildingInfo info)
        {
            if (!(info.m_buildingAI is ParkAI || info.m_buildingAI is ParkBuildingAI) || info.m_props == null)
            {
                return false;
            }

            int numParkingSpaces = 0;

            foreach (var prop in info.m_props)
            {
                if (prop.m_probability != 100)
                {
                    continue;
                }

                if (prop.m_prop == null || prop.m_prop.m_parkingSpaces == null)
                {
                    // Tree "props" have null m_props.
                    continue;
                }

                if (prop.m_prop.m_parkingSpaces.Length == 0)
                {
                    continue;
                }

                if (prop.m_position.y > 0.5f)
                {
                    // Parking space 0.5m above ground level -> assume it's a multi-storey car park
                    return true;
                }

                foreach (var parkingSpace in prop.m_prop.m_parkingSpaces)
                {
                    numParkingSpaces += 1;
                }
            }

            if (numParkingSpaces > 0)
            {
                float parkingArea = numParkingSpaces * APPROX_PARKING_SPACE_AREA;
                float parkArea = CalculateArea(info, false);

                return parkingArea / parkArea >= PARKING_AREA_FRACTION_THRESHOLD;
            }

            return false;
        }

        private static VehicleInfo.VehicleType GetVehicleTypesUsingStation(in BuildingInfo buildingInfo)
        {
            VehicleInfo.VehicleType vehicleTypes = 0;

            var stationAI = buildingInfo.m_buildingAI as TransportStationAI;
            if (stationAI && stationAI.m_info != null && stationAI.m_info.m_paths != null)
            {
                foreach (var path in stationAI.m_info.m_paths)
                {
                    vehicleTypes |= path.m_netInfo.m_vehicleTypes;
                }
            }

            return vehicleTypes;
        }

        public static StationType? GetStationType(in BuildingInfo buildingInfo)
        {
            VehicleInfo.VehicleType vehicleTypes = GetVehicleTypesUsingStation(buildingInfo);

            // Some stations are composed of sub-buildings
            if (buildingInfo.m_subBuildings != null)
            {
                foreach (var subBuilding in buildingInfo.m_subBuildings)
                {
                    vehicleTypes |= GetVehicleTypesUsingStation(subBuilding.m_buildingInfo);
                }
            }

            bool hasTrain = (vehicleTypes & VehicleInfo.VehicleType.Train) != 0;
            bool hasMetro = (vehicleTypes & VehicleInfo.VehicleType.Metro) != 0;

            if (hasTrain)
            {
                if (hasMetro)
                {
                    return StationType.MultiModal;
                }

                return StationType.Train;
            }
            else if (hasMetro)
            {
                return StationType.Metro;
            }

            return StationType.Unrecognized;
        }

        public static uint GetCarLaneCount(NetInfo info)
        {
            uint count = 0;

            foreach (var lane in info.m_lanes)
            {
                if (IsCarLane(lane))
                {
                    count++; 
                }
            }

            return count;
        }

        private static bool IsCarLane(NetInfo.Lane lane)
        {
            var laneType = NetInfo.LaneType.Vehicle;
            var vehicleType = VehicleInfo.VehicleType.Car;
            var vehicleCategory = VehicleInfo.VehicleCategoryPart1.PassengerCar;

            return (lane.m_laneType & laneType) != 0 &&
                   (lane.m_vehicleType & vehicleType) != 0 &&
                   (lane.m_vehicleCategoryPart1 & vehicleCategory) != 0;
        }
    }
}
