using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(Fire), "Init")]
    static class FirePatch
    {
        static void Postfix(Fire __instance, Cell ___cell)
        {
            if(___cell.OccupyingStructure.Count == 0)
            {
                CellMeta meta = Grid.Cells.Get(___cell);
                if (meta == null)
                    return;

                __instance.transform.position = new Vector3(__instance.transform.position.x, meta.Elevation, __instance.transform.position.z);
            }
        }
    }
}
