using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elevation
{
    //TODO: Test pillar terrain feature
    //IDEA: Canyon and maybe waterfall terrain features
    public class Crater : TerrainFeature
    {
        private static int minElevationTier = 2;
        public static MinMax radius = new MinMax(9, 15);
        public static MinMax mutation = new MinMax(0, 3);
        public static float mutationChance = 0.2f;
        public static float excludeChance = 0.05f;


        public override bool TestPlacement(Cell considering)
        {
            CellMeta meta = Grid.Cells.Get(considering);
            
            if (meta == null)
                return false;

            if (meta.elevationTier < minElevationTier)
                return false;

            Mod.dLog("Cliff Spawning: " + considering.Center.ToString());

            

            return false;
        }

        public override TerrainFeature Create(Cell origin)
        {
            Pillar pillar = new Pillar();

            pillar.origin = origin;

            Direction dir = new Direction[4]
            {
                Direction.North,
                Direction.South,
                Direction.East,
                Direction.West,
            }
            [SRand.Range(0, 3)];

            Cell startCell = Pathing.GetCardinal(origin, dir);
            CellMeta start = Grid.Cells.Get(startCell);
            if (start)
            {
                int tier = ElevationManager.maxElevation;

                World.inst.ForEachTileInRadius(start.cell.x, start.cell.z, (int)radius.Rand(), (x, z, cell) =>
                {
                    CellMeta meta = Grid.Cells.Get(cell);
                    if (meta == null)
                        return;

                    meta.elevationTier = tier + (SRand.Range(0f, 1f) < mutationChance ? (int)mutation.Rand() : 0);
                    if (SRand.Range(0f, 1f) < excludeChance)
                        meta.elevationTier = 0;

                    affected.Add(cell);
                });
            }

            return pillar;
        }




    }
}
