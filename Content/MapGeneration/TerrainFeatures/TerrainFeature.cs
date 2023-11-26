using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation
{
    public abstract class TerrainFeature
    {

        public Cell origin { get; set; }
        public List<Cell> affected { get; set; }

        public abstract TerrainFeature Create(Cell origin);

        public virtual bool TestPlacement(Cell considering)
        {
            return false;
        }

        public virtual int Get(Cell cell) 
        {
            return 0;
        }
    }
}
