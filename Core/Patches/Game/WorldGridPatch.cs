using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(World),"SnapToGrid2D")]
    class WorldGridPatch
    {
        static void Postfix(ref Vector3 __result)
        {
            Cell cell = World.inst.GetCellDataClamped(__result);
            if(cell != null)
            {
                // for [Experimental Elevation], clamp the y pos to an elevation tier/level; not jsut the highest
                CellMeta meta = Grid.Cells.Get(cell);
                if (meta != null)
                    __result.y = meta.Elevation;
            }
        }
    }

    [HarmonyPatch(typeof(GameUI), "GridPointerIntersection")]
    class PointerIntersectionPatch
    {
        static bool Prefix(ref Vector3 __result)
        {
            if (Physics.Raycast(Cam.inst.cam.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000f))
            {
                __result = World.inst.SnapToGrid2D(hit.point);
                return false;
            }
            else
                return true;
        }
    }
}
