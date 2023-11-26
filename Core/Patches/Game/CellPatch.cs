using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    
    [HarmonyPatch(typeof(Cell), "Center", MethodType.Getter)]
    //[HarmonyPatch("Center", PropertyMethod.Getter)]
    public class CellCenterPatch
    {
        static void Postfix(Cell __instance, ref Vector3 __result)
        {
            try
            {
                CellMeta meta = Grid.Cells.Get(__instance);
                if (meta != null)
                {
                    __result = new Vector3((float)__instance.x + 0.5f, meta.Elevation, (float)__instance.z + 0.5f);
                }
            }
            catch (Exception ex)
            {
                DebugExt.HandleException(ex);
            }
        }
    }


    [HarmonyPatch(typeof(TileInfoUI), "UpdateForNewCell")]
    public class TileInfoUIPatch
    {
        static void Postfix(TileInfoUI __instance, Cell cell)
        {
            if (WorldRegions.Unreachable.Contains(cell))
            {
                __instance.tipTextUI.text = "<color=yellow>Unreachable. May be made reachable by modifying surrounding terrain height. </color>";
            }
        }
    }

    //[HarmonyPatch(typeof(PathCell), "Center", MethodType.Getter)]
    ////[HarmonyPatch("Center", PropertyMethod.Getter)]
    //public class PathCellCenterPatch
    //{
    //    static void Postfix(PathCell __instance, ref Vector3 __result)
    //    {
    //        try
    //        {
    //            CellMeta meta = Grid.Cells.Get(__instance.x, __instance.z);
    //            if (meta != null)
    //            {
    //                __result = new Vector3((float)__instance.x + 0.5f, meta.Elevation, (float)__instance.z + 0.5f);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            DebugExt.HandleException(ex);
    //        }
    //    }
    //}

}
