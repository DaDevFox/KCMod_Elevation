using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    // y-adjustment patch sets y value with clean steps of
    // wolf position as it seeks a holistic 3-dimensional target pos (wanderPos).
    // This function sets the wanderPos y to the correct value even though the position
    // of wolves trying to get to this wanderPos is overwritten each frame, so that the
    // final check if the positions are near equal is satisfied, regardless of the path
    // taken to get there.

    [HarmonyPatch(typeof(WolfDen), "GetNewWanderPos")]
    class WanderPosPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(WolfDen).GetMethod("GetNewWanderPos", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        static void Postfix(WolfDen __instance, ref Vector3 ___wanderPos)
        {
            CellMeta meta = Grid.Cells.Get(___wanderPos);
            if (meta)
                ___wanderPos.y = meta.Elevation;
        }
    }

    [HarmonyPatch(typeof(WolfDen), "Tick")]
    class WolfYAdjustmentPatch
    {
        static void Postfix(WolfDen __instance, Vector3 ___wanderPos, ref ArrayExt<Vector3> ___wanderPositions)
        {
            if (__instance.wolfData == null)
                return;
            
            //for(int i = 0; i < ___wanderPositions.Count; i++)
            //{
            //    Vector3 pos = ___wanderPositions.data[i];
            //    if (pos == null) continue;

            //    CellMeta meta = Grid.Cells.Get(pos);
            //    if (meta)
            //        ___wanderPositions.data[i] = new Vector3(___wanderPositions.data[i].x, meta.Elevation, ___wanderPositions.data[i].z);
            //}

            for(int i = 0; i < __instance.wolfData.Count; i++) 
            {
                WolfDen.WolfData wolf = __instance.wolfData.data[i];
                if (wolf == null) 
                    continue;
                if (wolf.pos == null)
                    continue;

                if(wolf.status == WolfDen.Status.Wander && Mathff.DistSqrdXZ(___wanderPos, wolf.pos) < 0.2f)
                {
                    typeof(WolfDen).GetMethod("GetNewWanderPos", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(__instance, new object[0]);
                }

                Cell cell = World.inst.GetCellDataClamped(wolf.pos);

                if (cell == null)
                    continue;
                    
                CellMeta meta = Grid.Cells.Get(cell);
                if (meta != null)
                {
                    wolf.pos.y = YInterpolation.GetMidpointSlopedY(wolf.pos);
                }
            }
            
        }


    }
}
