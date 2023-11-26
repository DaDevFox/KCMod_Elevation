using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(Field), "Tick")]
    public class FieldPatch
    {
        static void Postfix(Field __instance, float ___time)
        {
            CellMeta meta = Grid.Cells.Get(World.inst.GetCellDataClamped(__instance.transform.position));
            if (meta != null)
            {
                __instance.currGrowHeight = -0.2f + Mathff.Clamp(___time * 0.33f, 0f, 0.33f) + meta.Elevation;
            }
        }
    }
}
