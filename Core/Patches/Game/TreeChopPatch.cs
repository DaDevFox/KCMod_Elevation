using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using Assets.Code.Jobs;
using System.Reflection;

namespace Elevation.Patches
{
    // Tree chopping avoidance
    [HarmonyPatch(typeof(GameUI), "AddChopJobToCell")]
    public class TreeChopPatch
    {
        static bool Prefix(Cell cell, ref bool __result)
        {
            if (cell == null)
                return true;

            CellMeta meta = Grid.Cells.Get(cell);
            if (!meta)
                return true;

            if(WorldRegions.Unreachable.Contains(cell))
            {
                OneOffEffect oneOffEffect = EffectsMan.inst.TileActionEffect.Create(cell.Center + new Vector3(0f, 1f, 0f));
                oneOffEffect.Color = Color.red;
                oneOffEffect.AllowRelease = true;
                oneOffEffect.Play();

                __result = false;
                return false;
            }

            return true;
        }
    }

    // Forester growing avoidance
    [HarmonyPatch(typeof(TreeGrowth), "IsValidForTreePlanting")]
    public class TreeGrowthPatch
    {
        static bool Prefix(Cell cell, ref bool __result)
        {
            if (cell == null)
                return true;

            CellMeta meta = Grid.Cells.Get(cell);
            if (!meta)
                return true;

            if (WorldRegions.Unreachable.Contains(cell))
            {
                __result = false;
                return false;
            }

            return true;
        }

    }

    // Forester tree chopping avoidance
    [HarmonyPatch(typeof(WoodCutterJob), "GetNonBusyTreeCellInRadius")]
    public class WoodCutterPatch
    {
        static void Postfix(ref List<Cell> results)
        {
            List<Cell> toRemove = new List<Cell>();

            foreach (Cell cell in results)
            {
                if (cell == null)
                    continue;

                CellMeta meta = Grid.Cells.Get(cell);
                if (!meta)
                    continue;

                if (WorldRegions.Unreachable.Contains(cell))
                {
                    toRemove.Add(cell);
                }
            }

            foreach(Cell cell in toRemove)
            {
                results.Remove(cell);
            }
        }
    }
}
