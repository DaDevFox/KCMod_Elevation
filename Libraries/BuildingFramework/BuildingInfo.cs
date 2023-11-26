using System;
using System.Collections.Generic;
using Assets;
using Assets.Code;
using UnityEngine;

public class BuildingInfo
{
    public GameObject buildingPrefab;
    public Building building;

    public GameObject displayModel;
    public Building.DragPlacementMode dragPlacementMode = Building.DragPlacementMode.None;

    public String uniqueName;
    public String customName = "Define Custom Name";
    public String descOverride = "Define Custom Description";
    public String preqBuilding = "keep";

    public ResourceAmount buildingCost = default(ResourceAmount);
    public Transform[] personPositions;

    public ResourceAmount yield;
    public float yieldInterval = 5f;
    public int workersForFullYield = 1;
    public float yieldIntervalMod = 1f;

    public int buildAllowedWorkers = 4;
    public JobCategory jobCategory = JobCategory.Undefined;
    public string categoryName = "<INSERT CATEGORY, IF ANY>";
    public string skillUsed = "";

    public bool subStructure = false;
    public bool stackable = false;
    public int stackHeight = 0;
    public bool ignoreRoadCoverageForPlacement = false;
    public bool showHappinessOverlay = false;
    public bool doBuildAnimation = true;
    public float fireChance = 0.5f;
    public bool buildersRequiredOnLocation = false;
    public bool allowOverAndUnderAqueducts = false;
    public float maxLife = 1f;

    public string[] placementSounds = new string[] {"defaultplacement"};
    public string[] selectionSounds = new string[] {"ui_openbuildmenu"};
    public SfxHandle buildSound = new SfxHandle(null, 1f);

    public List<OneOffEffect> buildEffects = new List<OneOffEffect>(10);

    public Vector3 buildingSize = new Vector3(1f,1f,1f);
    public Vector3 buildingButtonSize = new Vector3(1f,1f,1f);
    public String tabCategory = "Castle";

    public BuildingInfo(GameObject buildingPrefab, String uniqueName)
    {
        this.buildingPrefab = buildingPrefab;
        this.uniqueName = uniqueName;
    }

}