using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(TransportCart), "MoveUpdate")]
    public class TransportCartPatch
    {
        static void Prefix(float timeStep, Vector3 targetPos, Vector3 ___pos, ref float ___yAdjust)
        {
            float yAdjust = 0f;
            Cell cell = World.inst.GetCellDataClamped(___pos);
            if (cell.OccupyingStructure.Count > 0)
            {
                Building building = cell.OccupyingStructure[0];
                if (building.IsBuilt())
                {
                    if (building.uniqueNameHash == World.dockHash)
                    {
                        yAdjust = 0.06f;
                    }
                    else if (building.categoryHash == World.pathHash)
                    {
                        yAdjust = 0.1f;
                    }
                }
            }

            CellMeta meta = Grid.Cells.Get(___pos);
            if (meta != null)
            {
                ___yAdjust = yAdjust + YInterpolation.GetMidpointSlopedY(___pos, ElevationManager.slopingRadius * 3f); ;
            }
        }

        static void Finalizer(Exception ex)
        {
            Mod.dLog(ex);
        }
    }
}
