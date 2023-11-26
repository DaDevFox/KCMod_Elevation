using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using System.Collections;

namespace Elevation
{
    /// <summary>
    /// Represents a class that will be attached to a type <typeparamref name="T"/> on creation (if configured properly)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Meta<T>
    {
        public Meta(T obj) { }
    }

    public class BuildingMeta : Meta<Building>
    {
        public Building building { get; private set; }
        public int level;

        public BuildingMeta(Building building) : base(building)
        {
            this.building = building;
            this.level = ElevationManager.ClampPosToTier(building.transform.position.y);
            OnCreate();
        }

        private void OnCreate()
        {
            BuildingFormatter.UpdateBuilding(building);
        }

    }

    public class CellMeta : Meta<Cell>
    {
        #region Cell Information

        public Cell cell 
        { 
            get; 
            private set; 
        }

        public Vector3 Center => new Vector3((float)cell.x + 0.5f, Elevation, (float)cell.z + 0.5f);    

        #endregion

        #region Elevation Information

        /// <summary>
        /// The unity-metric actual height of this cell
        /// </summary>
        public float Elevation => (float)elevationTier * ElevationManager.elevationInterval;
            
        /// <summary>
        /// The integer tier height of this cell
        /// </summary>
        public int elevationTier { get; set; } = 0;

        /// <summary>
        /// [Experimental Elevation] currently levels are type bool; may change in the future
        /// </summary>
        public Dictionary<int, bool> levels { get; private set; }
        //public List<BuildingMeta> OccupyingStructure 
        //{
        //    get
        //    {

        //    }
        //}


        // IDEA: Instead of storing new Cells and modifying the XZ Grid to become an XYZ Grid, simply have each XZ cell store an array (maybe dict?) of levels, like how cells store buildings, ex. Castleblock code. This change will be known as [Experimental Elevation]




        #endregion

        #region Neighbors

        /// <summary>
        /// all available cell metas of the cells neighbors (excluding diagonals)
        /// </summary>
        public CellMeta[] neighbors
        {
            get
            {
                Cell[] neighborCells = new Cell[4]; 
                World.inst.GetNeighborCells(cell, ref neighborCells);
                List<CellMeta> metas = new List<CellMeta>();
                
                foreach(Cell c in neighborCells)
                    if (Grid.Cells.Get(c))
                        metas.Add(Grid.Cells.Get(c));

                return metas.ToArray();
            }
        }

        /// <summary>
        /// all available cell metas of the cells neighbors (excluding diagonals)
        /// </summary>
        public CellMeta[] neighborsFast
        {
            get
            {
                CellMeta[] metas = new CellMeta[4];

                metas[0] = Grid.Cells.Get(cell.x + 1, cell.z);
                metas[1] = Grid.Cells.Get(cell.x, cell.z + 1);
                metas[2] = Grid.Cells.Get(cell.x - 1, cell.z);
                metas[3] = Grid.Cells.Get(cell.x, cell.z - 1);

                return metas;
            }
        }

        /// <summary>
        /// all available cell metas of the cells neighbors (including diagonals)
        /// </summary>
        public List<CellMeta> neighborsPlus
        {
            get
            {
                Cell[] neighborCells = new Cell[8];
                World.inst.GetNeighborCellsExtended(cell, ref neighborCells);
                List<CellMeta> metas = new List<CellMeta>();

                for (int i = 0; i < neighborCells.Length; i++)
                {
                    Cell c = neighborCells[i];
                    if (Grid.Cells.Get(c))
                        metas.Add(Grid.Cells.Get(c));
                }

                return metas;
            }
        }

        /// <summary>
        /// Returns an array of cellmeta neighbors quickly; some may be null
        /// </summary>
        public CellMeta[] neighborsPlusFast
        {
            get
            {
                CellMeta[] metas = new CellMeta[8];

                metas[0] = Grid.Cells.Get(cell.x + 1, cell.z);
                metas[1] = Grid.Cells.Get(cell.x + 1, cell.z + 1);
                metas[2] = Grid.Cells.Get(cell.x, cell.z + 1);
                metas[3] = Grid.Cells.Get(cell.x - 1, cell.z + 1);
                metas[4] = Grid.Cells.Get(cell.x - 1, cell.z);
                metas[5] = Grid.Cells.Get(cell.x - 1, cell.z - 1);
                metas[6] = Grid.Cells.Get(cell.x, cell.z - 1);
                metas[7] = Grid.Cells.Get(cell.x + 1, cell.z - 1);

                return metas;
            }
        }

        #endregion

        #region Meta Information

        /// <summary>
        /// Cell meta id used to access this meta by the ElevationManager
        /// </summary>
        public string id { get => CellMetadata.GetPositionalID(cell); }

        /// <summary>
        /// Contains data about the cell's visual mesh
        /// </summary>
        public MeshData mesh = new MeshData();

        #endregion

        
        public CellMeta(Cell cell) : base(cell)
        {
            this.cell = cell;
        }

        public void Reset()
        {
            this.elevationTier = 0;
            this.cell = null;
        }


        public void UpdateVisuals(bool forced = false)
        {           
            Rendering.Update(cell, forced);
            // TEMP
            BuildingFormatter.UpdateBuildingsOnCell(cell);
        }

        
        public void UpdatePathing()
        {
            if (!ElevationManager.ValidTileForElevation(cell))
                return;

            World.inst.GetPathCell(cell).Center = cell.Center;
            UpdatePathfinderCost();
        }

        private void UpdatePathfinderCost()
        {
            World.inst.GetPathCell(cell).ogreFootPathCost = elevationTier;
            
            if(Combat.smartUnitPathing)
                World.inst.GetPathCell(cell).unitBlockCost = Pathing.unitPathingCostBase - Pathing.unitPathingAnticost * elevationTier;

            //for(int i = 0; i < World.inst.GetPathCell(cell).villagerFootPathCost.Length; i++)
            //    World.inst.GetPathCell(cell).villagerFootPathCost[i] = Pathing.tierPathingCost;
        }

        public bool PathableTo(Cell cell)
        {
            if (cell.Type == ResourceType.Water)
                return false;


            CellMeta meta = Grid.Cells.Get(cell);
            if (meta != null)
                if (Math.Abs(meta.elevationTier - elevationTier) <= 1)
                    return true;
            return false;
        }

        public struct MeshData
        {
            public string system;

            public int matrix;
            public int id;
        }

        public override string ToString()
        {
            return $"{CellMetadata.GetPositionalID(cell)}_t:{elevationTier}";
        }

        public static implicit operator bool(CellMeta meta) => meta != null;
    }
}
