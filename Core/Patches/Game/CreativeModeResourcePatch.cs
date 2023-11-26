using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(CreativeModeOptions), "AddResource")]
    public class CreativeModeResourcePatch
    {
        static void Prefix(ref Cell cell)
        {
            if (WorldRegions.Unreachable.Contains(cell))
                cell = Pathing.FindNearestUnblocked(cell, 5) ?? cell;
        }
    }
}
