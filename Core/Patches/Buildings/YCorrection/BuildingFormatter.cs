using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace Elevation
{
    public static class BuildingFormatter
    {
        public static Dictionary<string, float> offsets = new Dictionary<string, float>()
        {
            { "road", 0.05f },
            { "stoneroad", 0.05f },
            { "garden", 0.1f },
            { "farm", 0.05f },
            { "townsquare", 0.01f },
            { "largefountain", 0.01f },
            { "cemetery", 0.01f },
            { "cemetery44", 0.01f },
            { "cemeteryCircle", 0.01f },
            { "cemeteryDiamond", 0.01f }
        };

        
        public static float defaultOffset = 0f;


        public static bool UnevenTerrain(this Building building)
        {
            Cell firstCell = building.GetCell();
            bool flag = false;
            CellMeta firstMeta = Grid.Cells.Get(firstCell);
            if (firstCell != null && firstMeta != null)
            {
                
                int elevationTier = firstMeta.elevationTier;

                building.ForEachTileInBounds(delegate (int x, int y, Cell cell)
                {
                    CellMeta meta = Grid.Cells.Get(cell);
                    if (meta != null)
                    {
                        if (meta.elevationTier != elevationTier)
                        {
                            flag = true;
                        }
                    }
                });
            }

            return flag;
        }



        public static int GetStackPosOfBuildingAtIndex(this Cell cell, int idx)
        {
            int count = 0;
            int i = 0;
            while (count < cell.OccupyingStructure.Count)
            {
                if (count == idx)
                {
                    return i;
                }

                i += cell.OccupyingStructure[i].StackHeight;
                count++;
            }
            return -1;
        }

        /// <summary>
        /// Returns the stack height of a building at an index on the cell, relative to the elevation of the building's position
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static float GetRelativeHeightOfBuildingAtIndex(this Cell cell, int idx)
        {
            int count = 0;
            float i = 0;
            while (count < cell.OccupyingStructure.Count)
            {
                float stackHeight = 0f;


                if (cell.OccupyingStructure[count].Stackable)
                    stackHeight = cell.OccupyingStructure[count].StackHeight * 0.25f;


                if (count == idx)
                {
                    return i;
                }


                i += stackHeight;
                count++;
            }
            return 0;
        }

        /// <summary>
        /// Gets the actual height of the building at the index on a cell
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static float GetAbsoluteHeightOfBuildingAtIndex(this Cell cell, int idx)
        {
            if (cell == null)
                return 0f;

            CellMeta meta = Grid.Cells.Get(cell);
            float height = 0;
            if (meta != null)
                height = meta.Elevation;

            for(int i = 0; i <  cell.OccupyingStructure.Count; i++)
            {
                float stackRealHeight = 0f;

                if (cell.OccupyingStructure[i].Stackable)
                    stackRealHeight += cell.OccupyingStructure[i].StackHeight * 0.25f;

                height += stackRealHeight;


                if (i == idx)
                {
                    DebugExt.dLog($"absolute stack height at idx {idx}: {height}", false, cell);
                    return height;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the height of the highest building on a cell relative to its position on an elevation block
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static float GetRelativeHeightTotal(this Cell cell)
        {
            int count = 0;
            float i = 0;
            while (count < cell.OccupyingStructure.Count)
            {
                float stackRealHeight = 0f;

                if (cell.OccupyingStructure[count].Stackable)
                {
                    stackRealHeight = (float)cell.OccupyingStructure[count].StackHeight * 0.25f;
                }

                i += stackRealHeight;
                count++;
            }
            return i;
        }

        /// <summary>
        /// Gets the real height of the highest buliding on a cell
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public static float GetAbsoluteHeightTotal(this Cell cell)
        {
            CellMeta meta = Grid.Cells.Get(cell);

            float height = 0;
            if (meta != null)
                height = meta.Elevation;

            bool flag = false;

            for (int i = 0; i < cell.OccupyingStructure.Count; i++)
            {
                float stackRealHeight = 0f;



                if (cell.OccupyingStructure[i].Stackable)
                {
                    stackRealHeight += cell.OccupyingStructure[i].StackHeight * 0.25f;
                    flag = true;
                }

                height += stackRealHeight;
            }
            if (flag)
                return height;
            else
                return 0;
        }

        public static void UpdateBuilding(Building building)
        {
            Vector3 pos = building.transform.position;
            Cell cell = building.GetCell();
            CellMeta meta = Grid.Cells.Get(cell);

            bool rubble = building.UniqueName == "rubble";
            if(rubble)
            {
                Rubble rubbleBuilding = building.GetComponent<Rubble>();
                List<Rubble.BuildingState> buildingStates = rubbleBuilding.GetBuildingStates();
                if (buildingStates != null && buildingStates.Count > 0 && GameState.inst.GetPlaceableByUniqueName(buildingStates[buildingStates.Count - 1].uniqueName) != null)
                    rubble &= GameState.inst.GetPlaceableByUniqueName(buildingStates[buildingStates.Count - 1].uniqueName).CategoryName == "projectiletopper";
            }

            float stackHeight = 0;
            if (building.Stackable)
            {
                stackHeight = GetRelativeHeightOfBuildingAtIndex(cell, cell.OccupyingStructure.IndexOf(building));
            }
            if (building.UniqueName == World.greekFireName || building.UniqueName == World.ballistaTowerName || building.UniqueName == World.archerTowerName || rubble)
            {
                stackHeight = GetRelativeHeightTotal(cell);
            }

            if (meta == null)
                return;

            float offset = offsets.ContainsKey(building.UniqueName) ? offsets[building.UniqueName] : defaultOffset;

            if (!Mathf.Approximately(building.transform.position.y, meta.Elevation + stackHeight + offset))
            {
                // Better solution for [Experimental Elevation] required in this case; different buildings will be on different levels; perhaps need a building meta?
                building.transform.position = new Vector3(pos.x, meta.Elevation + stackHeight + offset, pos.z);
                
                building.UpdateShaderHeight();
                //Renderer[] renderers = building.transform.GetComponentsInChildren<Renderer>(true);
                //if(renderers.Length > 0)
                //{
                //    typeof(Building).GetField("renderMinY", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(building, renderers[0].bounds.min.y);
                //    typeof(Building).GetField("renderMaxY", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(building, renderers[0].bounds.max.y);
                //}


                Vector3[] positions = Buildings.PrefabPersonPositions.ContainsKey(building.uniqueNameHash) ? Buildings.PrefabPersonPositions[building.uniqueNameHash] : new Vector3[0];
                for (int i = 0; i < positions.Length; i++)
                    building.personPositions[i].localPosition = positions[i];
                

                //FLAGGG; doesn't work for all jobs?
                for(int i = 0; i < building.jobs.Count; i++)
                    if (i < building.jobs.Count && building.jobs[i].HasEmployee())
                        building.jobs[i].Employee.MoveToDeferred(building.GetPositionForPerson(building.jobs[i].Employee));
                
                building.RefreshCachedValues();
            }

            HappinessBonuses.Update(building);
        }

        public static float GetBuildingVisualOffset(string buildingUniqueName) => offsets.ContainsKey(buildingUniqueName) ? offsets[buildingUniqueName] : defaultOffset;

        public static void UpdateBuildingsOnCell(Cell cell)
        {
            foreach (Building building in cell.SubStructure)
                UpdateBuilding(building);
            foreach(Building building in cell.OccupyingStructure)
                UpdateBuilding(building);
        }
    }
}
