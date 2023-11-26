using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace Elevation.Patches
{
    [HarmonyPatch(typeof(PlacementMode), "UpdateBuildingAtPosition")]
    class BuildingPlacementPositionPatch
    {
        static void Postfix(Building b)
        {
            // TODO: Make Building Placement use Raycasting and intersection with the Elevation mesh (requires minecraft face culling)
            
            Cell cell = World.inst.GetCellData(b.transform.position);
            CellMeta meta = Grid.Cells.Get(cell);
            float leveling = GetLevellingForBuilding(b);
            if (cell != null && meta != null)
            {
                float offset = meta.elevationTier > 0 ? BuildingFormatter.GetBuildingVisualOffset(b.UniqueName) : 0f;
                b.transform.position = new Vector3(b.transform.position.x, leveling + offset, b.transform.position.z);
            }
            b.UpdateShaderHeight();
        }

        public static float GetLevellingForBuilding(Building b)
        {
            float max = 0f;
            b.ForEachTileInBounds(delegate(int x, int z, Cell cell)
            {
                // Better solution for [Experimental Elevation] required in this case; different buildings will be on different levels; perhaps need a building meta?
                float height = 0f;
                CellMeta meta = Grid.Cells.Get(cell);
                if(meta != null)
                    height = meta.Elevation;
                
                height += BuildingFormatter.GetRelativeHeightTotal(cell);
                if (height > max)
                    max = height;
                

                

            });
            return max;
        }

    }
}
