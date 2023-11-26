using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using System.Reflection;

namespace Elevation.Patches
{
    /// <summary>
    /// Replaces the GetLandings method of <see cref="RaiderSystem"/> to avoid cells with an elevation tier > 0
    /// </summary>
    [HarmonyPatch(typeof(RaiderSystem), "GetLandings")]
    public class VikingLandingPatch
    {
        static bool Prefix(int targetLandmass, int minVillagers, ref List<Cell> __result)
        {
            List<Cell> list = new List<Cell>();
            if (Player.inst.keep == null)
            {
                __result = list;
                return false;
            }
            Cell[] array = new Cell[4];
            ArrayExt<Cell> arrayExt = World.inst.cellsToLandmass[targetLandmass];
            for (int i = 0; i < arrayExt.Count; i++)
            {
                Cell cell = arrayExt.data[i];

                CellMeta meta = Grid.Cells.Get(cell);
                if (meta && meta.elevationTier == 0) 
                { 
                    if (cell.buildingAccess && cell.Type != ResourceType.Stone && cell.Type != ResourceType.UnusableStone && cell.Type != ResourceType.IronDeposit && !cell.deepWater)
                    {
                        bool flag = false;
                        World.inst.GetNeighborCells(cell, ref array);
                        foreach (Cell cell2 in array)
                        {
                            if (cell2 != null && cell2.partOfOcean && !PathCell.GetBlocksWaterPath(World.inst.GetPathCell(cell2), 1))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            list.Add(cell);
                        }
                    }
                }
            }

            __result = list;
            return false;
        }
    }
}
