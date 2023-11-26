
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Assets;
using Assets.Code;
using Elevation;


public class BuildingHelper
{
    public static List<BuildingInfo> buildingsToRegister = new List<BuildingInfo>();


    public static void RegisterBuilding(BuildingInfo info)
    {
        Building b = info.building;

        if(b == null)
        {
             b = info.buildingPrefab.AddComponent<Building>();
             info.building = b;
        }
        
        b.UniqueName = info.uniqueName;
        b.DisplayModel = info.displayModel;
        b.dragPlacementMode = info.dragPlacementMode;
        b.customName = info.customName;
        typeof(Building).GetField("descOverride", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(b, info.descOverride);
        typeof(Building).GetField("Cost", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(b, info.buildingCost);
        b.personPositions = info.personPositions;
        //b.Yield = info.yield;
        //b.YieldInterval = info.yieldInterval;
        b.WorkersForFullYield = info.workersForFullYield;
        //b.YieldIntervalMod = info.yieldIntervalMod;
        //b.YieldIntervalMod = info.yieldIntervalMod;
        b.BuildAllowedWorkers = info.buildAllowedWorkers;
        b.JobCategory = info.jobCategory;
        b.CategoryName = info.categoryName;
        b.skillUsed = info.skillUsed;
        b.SubStructure = info.subStructure;
        b.Stackable = info.stackable;
        b.StackHeight = info.stackHeight;
        b.ignoreRoadCoverageForPlacement = info.ignoreRoadCoverageForPlacement;
        b.showHappinessOverlay = info.showHappinessOverlay;
        b.doBuildAnimation = info.doBuildAnimation;
        b.fireChance = info.fireChance;
        b.BuildersRequiredOnLocation = info.buildersRequiredOnLocation;
        b.allowOverAndUnderAqueducts = info.allowOverAndUnderAqueducts;
        b.MaxLife = info.maxLife;
        b.placementSounds = info.placementSounds;
        b.SelectionSounds = info.selectionSounds;
        typeof(Building).GetField("buildSound", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(b, info.buildSound);
        typeof(Building).GetField("buildEffects", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(b, info.buildEffects);
        b.size = info.buildingSize;
        
        BuildingHelper.buildingsToRegister.Add(info);
    }

    public static object[] getTabByName(String tab)
    {
        object[] tabs = new object[2];
        switch(tab)
        {
            case "Castle":
                tabs[0] = BuildUI.inst.CastleTab;
                tabs[1] = BuildUI.inst.CastleTabVR;
                return tabs;
            case "Town":
                tabs[0] = BuildUI.inst.TownTab;
                tabs[1] = BuildUI.inst.TownTabVR;
                return tabs;
            case "AdvTown":
                tabs[0] = BuildUI.inst.AdvTownTab;
                tabs[1] = BuildUI.inst.AdvTownTabVR;
                return tabs;
            case "Industry":
                tabs[0] = BuildUI.inst.IndustryTab;
                tabs[1] = BuildUI.inst.IndustryTabVR;
                return tabs;
            case "Maritime":
                tabs[0] = BuildUI.inst.MaritimeTab;
                tabs[1] = BuildUI.inst.MaritimeTabVR;
                return tabs;
            case "Cemetery":
                tabs[0] = BuildUI.inst.CemeteryTab;
                tabs[1] = BuildUI.inst.CemeteryTabVR;
                return tabs;
        }
        tabs[0] = BuildUI.inst.CastleTab;
        tabs[1] = BuildUI.inst.CastleTabVR;
        return tabs;
    }



    //[HarmonyPatch(typeof(GameState))]
    //[HarmonyPatch("Awake")]
    public static class BuildingHelperInternalPrefabsPatch
    {
        static void Postfix(GameState __instance)
        {   
            try
            {
                
                
            }
            catch (Exception err)
            {
                //helper.Log(err.ToString());
            }
        }
    }


    //[HarmonyPatch(typeof(BuildUI))]
    //[HarmonyPatch("Start")]
    public static class BuildingHelperBuildUIPatch
    {
        //    static bool Prefix(BuildUI __instance)
        //    {
        //        try
        //        {
        //                BuildUI.inst = __instance;

        //                __instance.CastleTab = typeof(BuildUI).GetMethod("AddTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Castle" }) as BuildTab;
        //                __instance.CastleTabVR = typeof(BuildUI).GetMethod("AddTabVR", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Castle" }) as BuildTabVR;

        //                __instance.TownTab = typeof(BuildUI).GetMethod("AddTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Town" }) as BuildTab;
        //                __instance.TownTabVR = typeof(BuildUI).GetMethod("AddTabVR", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Town" }) as BuildTabVR;

        //                __instance.AdvTownTab = typeof(BuildUI).GetMethod("AddTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "AdvTown" }) as BuildTab;
        //                __instance.AdvTownTabVR = typeof(BuildUI).GetMethod("AddTabVR", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "AdvTown" }) as BuildTabVR;

        //                __instance.FoodTab = typeof(BuildUI).GetMethod("AddTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Food" }) as BuildTab;
        //                __instance.FoodTabVR = typeof(BuildUI).GetMethod("AddTabVR", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Food" }) as BuildTabVR;

        //                __instance.IndustryTab = typeof(BuildUI).GetMethod("AddTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Industry" }) as BuildTab;
        //                __instance.IndustryTabVR = typeof(BuildUI).GetMethod("AddTabVR", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Industry" }) as BuildTabVR;

        //                __instance.MaritimeTab = typeof(BuildUI).GetMethod("AddTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Maritime" }) as BuildTab;
        //                __instance.MaritimeTabVR = typeof(BuildUI).GetMethod("AddTabVR", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Maritime" }) as BuildTabVR;

        //                __instance.CemeteryTab = typeof(BuildUI).GetMethod("AddTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Cemetery" }) as BuildTab;
        //                __instance.CemeteryTabVR = typeof(BuildUI).GetMethod("AddTabVR", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { "Cemetery" }) as BuildTabVR;

        //                // Castle
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.keepName, null, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.woodcastleblockName, World.keepName, new Vector3(0.8f, 0.8f, 0.8f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.woodengateName, World.keepName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.castleblockName, World.keepName, new Vector3(0.8f, 0.8f, 0.8f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.gateName, World.keepName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.castlestairsName, World.keepName, new Vector3(0.8f, 0.8f, 0.8f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.archerTowerName, World.keepName, new Vector3(1f, 1f, 1f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.ballistaTowerName, World.chamberOfWarName, new Vector3(1f, 1f, 1f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.throneRoomName, World.keepName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.chamberOfWarName, World.keepName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.greatHallName, World.keepName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.moatName, World.chamberOfWarName, new Vector3(1f, 1f, 1f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.barracksName, World.chamberOfWarName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CastleTab, __instance.CastleTabVR, World.archerSchoolName, World.chamberOfWarName, new Vector3(0.4f, 0.4f, 0.4f)});

        //                // Town
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.roadName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.stoneRoadName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.smallHouseName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.wellName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.largeHouseName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.manorHouseName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.townsquareName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.tavernName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.TownTab, __instance.TownTabVR, World.fireHouseName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});

        //                // Advance Town
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.cemeteryDummyName, World.cemeteryKeeperName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.cemeteryKeeperName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.gardenName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.churchName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.libraryName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.clinicName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.hospitalName, World.clinicName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.fountainName, World.reservoirName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.largefountainName, World.reservoirName, new Vector3(0.55f, 0.55f, 0.55f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.bathhouseName, World.noriaName, new Vector3(0.33f, 0.33f, 0.33f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.statueLeviName, World.keepName, new Vector3(0.65f, 0.65f, 0.65f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.statueBarbaraName, World.keepName, new Vector3(0.65f, 0.65f, 0.65f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.statueSamName, World.keepName, new Vector3(0.33f, 0.33f, 0.33f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.greatLibraryName, World.libraryName, new Vector3(0.33f, 0.33f, 0.33f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.AdvTownTab, __instance.AdvTownTabVR, World.cathedralName, World.churchName, new Vector3(0.35f, 0.35f, 0.35f)});

        //                // Food
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.farmName, World.keepName, new Vector3(0.8f, 0.8f, 0.8f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.smallGranaryName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.granaryName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.windmillName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.bakerName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.orchardName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.produceStandName, World.keepName, new Vector3(0.75f, 0.75f, 0.75f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.fishinghutName, World.keepName, new Vector3(0.6f, 0.6f, 0.6f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.fishMongerName, World.fishinghutName, new Vector3(0.7f, 0.7f, 0.7f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.swineherdName, World.farmName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.butcherName, World.farmName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.smallMarketName, World.keepName, new Vector3(0.9f, 0.9f, 0.9f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.metaetName, World.keepName, new Vector3(0.6f, 0.6f, 0.6f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.noriaName, World.keepName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.aqueductName, World.noriaName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.FoodTab, __instance.FoodTabVR, World.reservoirName, World.noriaName, new Vector3(0.5f, 0.5f, 0.5f)});

        //                // Industry
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.quarryName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.foresterName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.smallstockpileName, World.keepName, new Vector3(0.7f, 0.7f, 0.7f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.largeStockpileName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.charcoalMakerName, World.foresterName, new Vector3(0.75f, 0.75f, 0.75f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.ironMineName, World.keepName, new Vector3(0.85f, 0.85f, 0.85f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.blacksmithName, World.keepName, new Vector3(0.6f, 0.6f, 0.6f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.masonName, World.keepName, new Vector3(0.6f, 0.6f, 0.6f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.IndustryTab, __instance.IndustryTabVR, World.stoneRemovalName, World.keepName, new Vector3(0.6f, 0.6f, 0.6f)});

        //                // Maritime
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.MaritimeTab, __instance.MaritimeTabVR, World.outpostName, World.keepName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.MaritimeTab, __instance.MaritimeTabVR, World.dockName, World.keepName, new Vector3(0.4f, 0.4f, 0.4f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.MaritimeTab, __instance.MaritimeTabVR, World.transportShipName, World.dockName, new Vector3(0.6f, 0.6f, 0.6f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.MaritimeTab, __instance.MaritimeTabVR, World.troopTransportShipName, World.keepName, new Vector3(0.6f, 0.6f, 0.6f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.MaritimeTab, __instance.MaritimeTabVR, World.pierName, World.keepName, new Vector3(0.5f, 0.5f, 0.5f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.MaritimeTab, __instance.MaritimeTabVR, World.bridgeName, World.keepName, Vector3.one});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.MaritimeTab, __instance.MaritimeTabVR, World.drawBridgeName, World.keepName, new Vector3(1f, 1f, 1f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.MaritimeTab, __instance.MaritimeTabVR, World.stoneBridgeName, World.keepName, Vector3.one});

        //                // Cemetery
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CemeteryTab, __instance.CemeteryTabVR, World.cemetery3x3Name, World.cemeteryKeeperName, new Vector3(1f, 1f, 1f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CemeteryTab, __instance.CemeteryTabVR, World.cemeteryCircleName, World.cemeteryKeeperName, new Vector3(1f, 1f, 1f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CemeteryTab, __instance.CemeteryTabVR, World.cemeteryDiamondName, World.cemeteryKeeperName, new Vector3(1f, 1f, 1f)});
        //                typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { __instance.CemeteryTab, __instance.CemeteryTabVR, World.cemetery4x4Name, World.cemeteryKeeperName, new Vector3(1f, 1f, 1f)});

        //                // Custom Building
        //                foreach(BuildingInfo info in BuildingHelper.buildingsToRegister)
        //                {
        //                    object[] tabs = BuildingHelper.getTabByName(info.tabCategory);
        //                    typeof(BuildUI).GetMethod("AddBuilding", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public ).Invoke(BuildUI.inst, new object[] { (BuildTab)(tabs[0]), (BuildTabVR)(tabs[1]), info.uniqueName, info.preqBuilding, info.buildingButtonSize});
        //                }


        //                __instance.RefreshButtonLayout();

        //                __instance.IndustryTab.Visible = false;
        //                __instance.FoodTab.Visible = false;
        //                __instance.CastleTab.Visible = false;
        //                __instance.TownTab.Visible = false;
        //                __instance.AdvTownTab.Visible = false;
        //                __instance.MaritimeTab.Visible = false;
        //                __instance.CemeteryTab.Visible = false;
        //                __instance.IndustryTabVR.Visible = false;
        //                __instance.FoodTabVR.Visible = false;
        //                __instance.CastleTabVR.Visible = false;
        //                __instance.TownTabVR.Visible = false;
        //                __instance.AdvTownTabVR.Visible = false;
        //                __instance.MaritimeTabVR.Visible = false;
        //                __instance.CemeteryTabVR.Visible = false;

        //        }catch (Exception err){
        //            //helper.Log(err.ToString());
        //        }
        //        return false;
        //    }


        static void Postfix(BuildUI __instance)
        {
            foreach (BuildingInfo info in BuildingHelper.buildingsToRegister)
            {
                Mod.dLog($"registering {info.uniqueName}");
                GameState.inst.internalPrefabs.Add(info.building);
            }


            foreach (BuildingInfo info in buildingsToRegister) 
            {
                object[] tabs = getTabByName(info.tabCategory);
                BuildTab normal = tabs[0] as BuildTab;
                BuildTabVR vr = tabs[1] as BuildTabVR;

                //__instance.AddBuilding(normal, vr, info.uniqueName, info.preqBuilding, Vector3.one);
            }

            __instance.RefreshButtonLayout();
        }


    }


}