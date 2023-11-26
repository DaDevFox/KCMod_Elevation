using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(World), "CanPlace")]
    public class PlacementValidationPatch
    {

        public static bool CurrentPlacementOnBlockedCell { get; private set; } = false;
        public static bool ItemOnTerrainInvalid { get; private set; } = false;

        public static bool QuarryMineAboveResource { get; private set; } = false;

        public static bool InvalidScaffolding { get; private set; } = false;
        public static bool LimitForScaffolding { get; private set; } = false;
        public static bool InvalidDugout { get; private set; } = false;
        public static bool LimitForDugout { get; private set; } = false;


        static void Postfix(Building PendingObj, ref PlacementValidationResult __result)
        {
            Cell cell = World.inst.GetCellDataClamped(PendingObj.transform.position);
            if (cell == null)
                return;

            /// For [Experimental Elevation]: place building on different level depending on raycast target. 
            CellMeta meta = Grid.Cells.Get(cell);

            if (PendingObj.UnevenTerrain())
            {
                __result = PlacementValidationResult.MustBeOnFlatLand;
            }

            QuarryMineAboveResource = false;
            if(PendingObj.uniqueNameHash == World.quarryHash || PendingObj.uniqueNameHash == World.largeQuarryHash || 
                PendingObj.uniqueNameHash == World.ironMineHash || PendingObj.uniqueNameHash == World.largeIronMineHash)
            {
                int max = 0;
                PendingObj.ForEachTileInBounds((x, z, buildingCell) => 
                {
                    CellMeta buildingCellMeta = Grid.Cells.Get(buildingCell);
                    if (buildingCellMeta == null)
                        return;

                    if(buildingCellMeta.elevationTier > max)
                        max = buildingCellMeta.elevationTier;
                });

                ResourceType resource = PendingObj.uniqueNameHash == World.quarryHash || PendingObj.uniqueNameHash == World.largeQuarryHash ? ResourceType.Stone : ResourceType.IronDeposit;
                Cell adjacentResource = World.inst.GetCardinalAdjacentResource(cell.x, cell.z, resource);
                if (adjacentResource == null)
                    return;

                CellMeta adjacentResourceMeta = Grid.Cells.Get(adjacentResource);
                if(adjacentResourceMeta == null) 
                    return;

                if(adjacentResourceMeta.elevationTier < max)
                {
                    __result = PlacementValidationResult.QuarryMustFaceRock;
                    QuarryMineAboveResource = true;
                }
            }

            if (__result == PlacementValidationResult.Valid && Pathing.BlockedCompletely(World.inst.GetCellData(PendingObj.transform.position)) && 
                !((PendingObj.UniqueName == "destructioncrew" || PendingObj.UniqueName == "largequarry" || PendingObj.UniqueName == "largeironmine") 
                && (cell.Type == ResourceType.UnusableStone || cell.Type == ResourceType.Stone || cell.Type == ResourceType.IronDeposit || cell.Type == ResourceType.EmptyCave)))
            {
                __result = PlacementValidationResult.OutsideOfTerritory;
                CurrentPlacementOnBlockedCell = true;
            }
            else
            {
                CurrentPlacementOnBlockedCell = false;
            }

            if ((meta != null && meta.elevationTier > 0) &&
                (PendingObj.UniqueName == "aqueduct" || 
                PendingObj.UniqueName == "reservoir" || 
                PendingObj.UniqueName == "largereservoir" ||
                PendingObj.UniqueName == "noria" || 
                PendingObj.UniqueName == "dock" || 
                PendingObj.UniqueName == "moat"))
            {
                __result = PlacementValidationResult.WellOnGround;
                ItemOnTerrainInvalid = true;
            }
            else
                ItemOnTerrainInvalid = false;

            if (PendingObj.UniqueName == "scaffolding" && Scaffolding.scaffoldedCells.Contains($"{PendingObj.GetCell().x}_{PendingObj.GetCell().z}"))
            {
                __result = PlacementValidationResult.CastleBlockNoSupport;
                InvalidScaffolding = true;
            }
            else
                InvalidScaffolding = false;

            if (PendingObj.UniqueName == "dugout" && Dugout.dugoutCells.Contains($"{PendingObj.GetCell().x}_{PendingObj.GetCell().z}"))
            {
                __result = PlacementValidationResult.CastleBlockNoSupport;
                InvalidDugout = true;
            }
            else
                InvalidDugout = false;

            if((PendingObj.UniqueName == "scaffolding" || PendingObj.UniqueName == "dugout") && (cell.Type == ResourceType.Water || (cell.TopSubStructure && cell.TopSubStructure.UniqueName == "pier")))
            {
                __result = PlacementValidationResult.CannotBuildOnWater;
            }

            if (meta)
            {
                if (PendingObj.UniqueName == "scaffolding" && meta.elevationTier == ElevationManager.maxElevation)
                {
                    __result = PlacementValidationResult.CastleBlockNoSupport;
                    LimitForScaffolding = true;
                }
                else
                    LimitForScaffolding = false;

                if (PendingObj.UniqueName == "dugout" && meta.elevationTier == 0)
                {
                    __result = PlacementValidationResult.CastleBlockNoSupport;
                    LimitForDugout = true;
                }
                else
                    LimitForDugout = false;
            }
        }
    }

    [HarmonyPatch(typeof(BuildInfoUI), "Update")]
    class BuildInfoUIPatch
    {
        static void Postfix(BuildInfoUI __instance)
        {
            if (PlacementValidationPatch.ItemOnTerrainInvalid)
                __instance.ruleTextUI.text = "This building cannot be placed on elevation, highness. ";

            if (PlacementValidationPatch.CurrentPlacementOnBlockedCell)
                __instance.ruleTextUI.text = "Our builders cannot find a way to reach this tile, your highness. ";

            if(PlacementValidationPatch.QuarryMineAboveResource)
                __instance.ruleTextUI.text = "Quarries or mines must be below or level to their mineable resource, your highness.";

            if (PlacementValidationPatch.InvalidScaffolding)
                __instance.ruleTextUI.text = "Scaffolding has already been used on this tile, highness. It cannot safely be elevated any higher.";
            
            if(PlacementValidationPatch.LimitForScaffolding)
                __instance.ruleTextUI.text = "This area is too high to be further landscaped.";
            
            if(PlacementValidationPatch.InvalidDugout)
                __instance.ruleTextUI.text = "A dugout has already been used on this tile, highness. It cannot safely be dug into any further.";

            if (PlacementValidationPatch.LimitForDugout)
                __instance.ruleTextUI.text = "This area is already at sea level.";
        }
    }

}
