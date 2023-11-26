using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Code;
using Assets;
using System.Reflection;

namespace Elevation.Patches
{
    // Called every frame
    [HarmonyPatch(typeof(FreeResourceSystem), "AddFreeResource")]
    static class StackVisualCorrection
    {
        static void Prefix(FreeResourceSystem __instance, FreeResource prefab, ref Vector3 pos)
        {
            Cell cell = World.inst.GetCellDataClamped(pos);
            if (cell == null)
                return;

            CellMeta meta = Grid.Cells.Get(cell);
            if (meta == null)
                return;

            pos.y += meta.Elevation;
        }
    }
}
