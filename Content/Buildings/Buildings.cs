using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code;
using Harmony;
using UnityEngine;
using Elevation.AssetManagement;
using System.Reflection;
using I2.Loc;
using Fox.Localization;
using Elevation.Patches;

namespace Elevation
{
    public class BuildingAssets
    {
        public static GameObject ScaffoldingPrefab { get; private set; }
        public static GameObject DugoutPrefab { get; private set; }

        public static void LoadAll()
        {
            ScaffoldingPrefab = ModAssets.DB.GetByName<GameObject>("Scaffolding");
            DugoutPrefab = ModAssets.DB.GetByName<GameObject>("Dugout");
        }
    }

    public class Buildings
    {

        public static Dictionary<int, Vector3[]> PrefabPersonPositions { get; private set; } = new Dictionary<int, Vector3[]>();

        public static GameObject ScaffoldingPrefab { get; private set; }
        public static Building placeable_scaffoldingBuilding { get; private set; }

        public static GameObject DugoutPrefab { get; private set; }
        public static Building placeable_dugoutBuilding { get; private set; }

        public static void Init()
        {
            //Broadcast.BuildingBuilt.ListenAny(
            //    (sender, data) =>
            //    {
            //        Mod.dLog($"Sender: {sender}");
            //        BuildFXPatch.Correct(data.targetBuilding);
            //    });
            Register();
        }

        public static void Register()
        {
            #region Scaffolding Building

            ScaffoldingPrefab = BuildingAssets.ScaffoldingPrefab;
            if (ScaffoldingPrefab == null)
                Mod.Log("Scaffolding prefab not loaded");
            placeable_scaffoldingBuilding = ScaffoldingPrefab.AddComponent<Building>();

            Scaffolding scaffolding = ScaffoldingPrefab.AddComponent<Scaffolding>();

            BuildingCollider scaffoldingCollider = scaffolding.transform.Find("Offset").Find("Scaffolding").gameObject.AddComponent<BuildingCollider>();
            scaffoldingCollider.Building = placeable_scaffoldingBuilding;

            // Initial Configuration    
            placeable_scaffoldingBuilding.UniqueName = "scaffolding";
            placeable_scaffoldingBuilding.customName = "Scaffolding";

            placeable_scaffoldingBuilding.BuildAllowedWorkers = 12;
            placeable_scaffoldingBuilding.WorkersForFullYield = 0;
            placeable_scaffoldingBuilding.WorkersAllocated = 0;
            placeable_scaffoldingBuilding.jobs = new List<Job>();

            placeable_scaffoldingBuilding.placementSounds = new string[] { "castleplacement" };
            placeable_scaffoldingBuilding.SelectionSounds = new string[] { "Building_Select_Road_1" };

            placeable_scaffoldingBuilding.ignoreRoadCoverageForPlacement = true;
            placeable_scaffoldingBuilding.dragPlacementMode = Building.DragPlacementMode.Rectangle;

            // Resource Cost
            ResourceAmount cost = new ResourceAmount();
            cost.Set(FreeResourceType.Tree, 150);
            cost.Set(FreeResourceType.Stone, 50);
            cost.Set(FreeResourceType.Tools, 15);
            cost.Set(FreeResourceType.Gold, 50);

            typeof(Building).GetField("Cost", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(placeable_scaffoldingBuilding, cost);

            // Person Positions
            Transform[] newPositions = new Transform[12];
            for(int i = 1; i <= 12; i++)
                newPositions[i - 1] = scaffolding.transform.Find($"pp{i}");
            
            placeable_scaffoldingBuilding.personPositions = newPositions;

            #endregion

            #region Dugout

            DugoutPrefab = BuildingAssets.DugoutPrefab;
            if (DugoutPrefab == null)
                Mod.Log("Dugout prefab not loaded");
            placeable_dugoutBuilding = DugoutPrefab.AddComponent<Building>();

            Dugout dugout = DugoutPrefab.AddComponent<Dugout>();

            BuildingCollider dugoutCollider = dugout.transform.Find("Offset").Find("ColliderCube").gameObject.AddComponent<BuildingCollider>();
            dugoutCollider.Building = placeable_dugoutBuilding;

            // Initial Configuration
            placeable_dugoutBuilding.UniqueName = "dugout";
            placeable_dugoutBuilding.customName = "Dugout";

            placeable_dugoutBuilding.BuildAllowedWorkers = 5;
            placeable_dugoutBuilding.WorkersForFullYield = 0;
            placeable_dugoutBuilding.WorkersAllocated = 0;
            placeable_dugoutBuilding.jobs = new List<Job>();

            placeable_dugoutBuilding.placementSounds = new string[] { "castleplacement" };
            placeable_dugoutBuilding.SelectionSounds = new string[] { "Building_Select_Road_1" };

            placeable_dugoutBuilding.ignoreRoadCoverageForPlacement = true;
            placeable_dugoutBuilding.dragPlacementMode = Building.DragPlacementMode.Rectangle;

            // Resource Cost
            ResourceAmount dugoutCost = new ResourceAmount();
            dugoutCost.Set(FreeResourceType.Tree, 75);
            dugoutCost.Set(FreeResourceType.Stone, 25);
            dugoutCost.Set(FreeResourceType.Tools, 30);
            dugoutCost.Set(FreeResourceType.Gold, 75);

            typeof(Building).GetField("Cost", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(placeable_dugoutBuilding, dugoutCost);

            // Person Positions
            Transform[] newDugoutPositions = new Transform[12];
            for (int i = 1; i <= 12; i++)
                newDugoutPositions[i - 1] = dugout.transform.Find($"pp{i}");

            placeable_dugoutBuilding.personPositions = newDugoutPositions;

            #endregion
        }

        public static void Setup()
        {
            RoadStairs.Reset();
        }



    }

    [HarmonyPatch(typeof(GameState))]
    [HarmonyPatch("Awake")]
    static class InternalPrefabsPatch
    {
        static void Postfix(GameState __instance)
        {
            __instance.internalPrefabs.Add(Buildings.placeable_scaffoldingBuilding);
            __instance.internalPrefabs.Add(Buildings.placeable_dugoutBuilding);

            HappinessBonuses.Init();
        }
    }

    [HarmonyPatch(typeof(BuildUI))]
    [HarmonyPatch("Start")]
    static class BuildUIPatch
    {
        static void Postfix(BuildUI __instance)
        {
            //if (!GameState.inst.internalPrefabs.Contains(Buildings.placeable_scaffoldingBuilding))
            //    GameState.inst.internalPrefabs.Add(Buildings.placeable_scaffoldingBuilding);
            __instance.AddBuilding(__instance.IndustryTab, __instance.IndustryTabVR, __instance.IndustryTabConsole,
                "scaffolding", "blacksmith", Vector3.one);
            __instance.AddBuilding(__instance.IndustryTab, __instance.IndustryTabVR, __instance.IndustryTabConsole,
                "dugout", "blacksmith", Vector3.one);

            ReadPersonPositions();
        }

        static void ReadPersonPositions()
        {
            foreach(Building building in GameState.inst.internalPrefabs)
            {
                if (building.personPositions == null)
                    continue;

                Vector3[] array = new Vector3[building.personPositions.Length];
                for(int i = 0; i < array.Length; i++)
                {
                    array[i] = building.personPositions[i] != null ? building.personPositions[i].localPosition : Vector3.zero;
                }

                if (!Buildings.PrefabPersonPositions.ContainsKey(building.uniqueNameHash))
                    Buildings.PrefabPersonPositions.Add(building.uniqueNameHash, array);
                else
                    Buildings.PrefabPersonPositions[building.uniqueNameHash] = array; // throw an exception???
            }
        }
    }


    [HarmonyPatch(typeof(LocalizationManager))]
    [HarmonyPatch("GetTranslation")]
    public static class LocalizationManagerPatch
    {
        static void Postfix(string Term, ref string __result)
        {
            // Scaffolding
            if (Term == "Building scaffolding FriendlyName")
                __result = Localization.Get("scaffolding_friendly_name");
            else if (Term == "Building scaffolding Description")
                __result = Localization.Get("scaffolding_description");
            else if (Term == "Building scaffolding ThoughtOnBuilt")
                __result = Localization.Get("scaffolding_buildingthought");

            // Dugout
            if (Term == "Building dugout FriendlyName")
                __result = Localization.Get("dugout_friendly_name");
            else if (Term == "Building dugout Description")
                __result = Localization.Get("dugout_description");
            else if (Term == "Building dugout ThoughtOnBuilt")
                __result = Localization.Get("dugout_buildingthought"); ;
        }
    }
}
