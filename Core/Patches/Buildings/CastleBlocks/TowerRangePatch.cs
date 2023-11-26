using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(ProjectileDefense), "GetHeight")]
    class ProjectileDefenseRangePatch
    {
        static void Postfix(ProjectileDefense __instance, ref int __result)
        {
            try
            {
                Cell cell = World.inst.GetCellDataClamped(__instance.transform.position);
                if (cell != null)
                {
                    CellMeta meta = Grid.Cells.Get(cell);
                    if (meta != null)
                    {
                        // In this case, [Experimental Elevation] will need meta.elevationTier to be the tier of the highest elevation level on the cell
                        __result += meta.elevationTier;
                    }
                }
            }
            catch(Exception ex)
            {
                DebugExt.HandleException(ex);
            }
        }
    }

    [HarmonyPatch(typeof(ProjectileDefense), "Update")]
    class ProjectileDefenseDebugPatch
    {
        static void Postfix(ProjectileDefense __instance)
        {
            if(__instance.trackingTarget != null && !__instance.trackingTarget.Equals(null))
            {
                DebugExt.dLog($"Ballista has target {__instance.trackingTarget.GetType()}; attacking: {__instance.attackTarget != null}\n\nBallista rotate parent position: {__instance.GetComponent<Ballista>().RotateParent.position}, target position: {__instance.trackingTarget?.GetPredictedPosition(1f)}", false, __instance.trackingTarget);
            }
        }
    }

    [HarmonyPatch(typeof(ArcherTower), "GetHeight")]
    class ArcherTowerRangePatch
    {
        static void Postfix(ref int __result, Vector3 pos, int maxHeight)
        {
            try
            {
                Cell cell = World.inst.GetCellData(pos);
                if (cell != null)
                {
                    CellMeta meta = Grid.Cells.Get(cell);
                    if (meta != null)
                    {
                        // In this case, [Experimental Elevation] will need meta.elevationTier to be the tier of the highest elevation level on the cell
                        __result += meta.elevationTier;
                    }
                }
            }
            catch(Exception ex)
            {
                DebugExt.HandleException(ex);
            }
        }
    }

}
