using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace Elevation.Patches
{


    //Patch stack height to match elevation
    [HarmonyPatch(typeof(Cell), "CurrentStackHeight")]
    public class StackHeightPatch
    {
        static void Postfix(Cell __instance, ref int __result)
        {
            if (__instance != null && __instance.GetTopMostCastlePiece() != null)
            {
                //if (__instance.TopStructure.Stackable)
                //{
                    CellMeta meta = Grid.Cells.Get(__instance);
                    if (meta != null)
                    {
                        // 0.25f is the stacking constant (1 stack height = 0.25 unity metric)
                        // In this case, [Experimental Elevation] will need meta.elevationTier to be the tier of the highest elevation level on the cell
                        __result += meta.elevationTier * (int)(ElevationManager.elevationInterval / 0.25f);
                    }
                //}
            }
        }
    }

    //Patch method to find building at stack height
    [HarmonyPatch(typeof(CastleBlock), "GetBuildingAtStackHeight")]
    public class CastleBlockRelativityPatch
    {
        static bool Prefix(Cell c, ref int stackHeight)
        {
            if (c != null)
            {
                CellMeta meta = Grid.Cells.Get(c);
                if (meta != null)
                {
                    if (meta.elevationTier > 0)
                    {
                        // In this case, [Experimental Elevation] will need meta.elevationTier to be the tier of the highest elevation level on the cell
                        stackHeight -= meta.elevationTier * (int)(ElevationManager.elevationInterval / 0.25f);
                    }
                }
            }
            return true;
        }
    }







}
