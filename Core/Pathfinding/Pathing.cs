using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation
{
	public enum Diagonal
	{
		NorthEast,
		NorthWest,
		SouthEast,
		SouthWest,
		None
	}

	public static class Pathing
    {
		public static int unitPathingCostBase = 50;
		public static int unitPathingAnticost = 1;

		public static bool IsDiagonalXZ(Cell from, Cell to)
		{
			return IsDiagonalXZ(from.Center, to.Center);
		}

		public static bool IsDiagonalXZ(Vector3 from, Vector3 to)
		{
			Vector3 difference = (to - from).xz();
			int count = 0;
			float epsilon = 0.1f;

			if (Mathf.Abs(difference.x) >= epsilon)
				count++;
			if (Mathf.Abs(difference.z) >= epsilon)
				count++;

			return count == 2;
		}

		public static bool GetDiagonal(Cell from, Cell to, out Diagonal diagonal)
		{
			Dictionary<Vector3, Diagonal> diagonals = new Dictionary<Vector3, Diagonal>()
			{
				{ new Vector3(1f,0f,1f), Diagonal.SouthEast },
				{ new Vector3(1f,0f,-1f), Diagonal.NorthEast },
				{ new Vector3(-1f,0f,1f), Diagonal.SouthWest },
				{ new Vector3(-1f,0f,-1f), Diagonal.NorthWest },
			};

			diagonal = Diagonal.NorthEast;

			if (from == null)
				throw new ArgumentNullException("from");
			if (to == null)
				throw new ArgumentNullException("to");

			Vector3 diff = from.Center - to.Center;
			Vector3 diffNormalized = Vector3.Normalize(new Vector3(diff.x, 0f, diff.z));

			bool validDiagonal = false;


			if (diagonals.ContainsKey(diffNormalized))
			{
				validDiagonal = true;
				diagonal = diagonals[diffNormalized];
			}

			return validDiagonal;
		}

        public static CellMeta GetCardinal(CellMeta source, Direction direction)
        {
            Cell cell = Pathing.GetCardinal(source.cell, direction);
            if (cell != null)
                return Grid.Cells.Get(cell);
            return null;
        }

        public static bool GetCardinal(Cell from, Cell to, out Direction direction)
		{
			Dictionary<Vector3, Direction> dirs = new Dictionary<Vector3, Direction>()
			{
				{ new Vector3(1f, 0f, 0f), Direction.East },
				{ new Vector3(0f, 0f, 1f), Direction.South },
				{ new Vector3(-1f, 0f, 0f), Direction.West },
				{ new Vector3(0f, 0f, -1f), Direction.North },
			};

			Vector3 diff = from.Center.xz() - to.Center.xz();
			diff = Vector3.ClampMagnitude(diff, 1f);

			if (dirs.ContainsKey(diff))
			{
				direction = dirs[diff];
				return true;
			}
			direction = Direction.North;
			return false;
		}

		public static Cell GetCardinal(Cell from, Direction direction)
		{
			if (from == null)
				return null;

			Dictionary<Direction, Vector3> dirs = new Dictionary<Direction, Vector3>()
				{
					{ Direction.East,  new Vector3(1f, 0f, 0f) },
					{ Direction.South, new Vector3(0f, 0f, 1f) },
					{ Direction.West, new Vector3(-1f, 0f, 0f)},
					{ Direction.North, new Vector3(0f, 0f, -1f)},
				};

			if (dirs.ContainsKey(direction))
			{
				Vector3 vec = dirs[direction];
				return World.inst.GetCellData(from.Center + vec);
			}
			return null;
		}

		public static Vector3 GetUnitVector(Direction @for)
		{
			Dictionary<Direction, Vector3> dirs = new Dictionary<Direction, Vector3>()
				{
					{ Direction.East,  new Vector3(1f, 0f, 0f) },
					{ Direction.South, new Vector3(0f, 0f, 1f) },
					{ Direction.West, new Vector3(-1f, 0f, 0f)},
					{ Direction.North, new Vector3(0f, 0f, -1f)},
				};
			return dirs[@for];
		}

		public static bool BlockedCompletely(Cell cell)
		{
			bool blocked = cell == null;
			if (cell != null)
			{
				if (WorldRegions.Marked)
				{
					if (WorldRegions.Unreachable.Contains(cell))
						blocked = true;
				}

				if (BlocksForBuilding(cell))
					blocked = true;

				if (cell.deepWater)
					blocked = true;
			}
			return blocked;
		}

		public static bool BlocksForBuilding(Cell c)
		{
			if (c == null) return false;

			return c.Type == ResourceType.IronDeposit || 
				c.Type == ResourceType.Stone || 
				c.Type == ResourceType.UnusableStone || 
				c.Type == ResourceType.WolfDen || 
				c.Type == ResourceType.EmptyCave || 
				c.Type == ResourceType.WitchHut;
		}

		public static bool Connected(Cell from, Cell to)
        {
			if (from == null || to == null)
				return false;

			CellMeta fromMeta = Grid.Cells.Get(from);
			CellMeta toMeta = Grid.Cells.Get(to);
			if (fromMeta && toMeta)
				if (Math.Abs(fromMeta.elevationTier - toMeta.elevationTier) <= 1)
					return true;
			return false;
        }

		public static Cell FindNearUnblockedFast(Cell origin, int radius)
		{
			if (origin == null || !WorldRegions.Unreachable.Contains(origin))
				return origin;

			Cell selected = null;
			World.inst.ForEachTileInRadiusOrDone(origin.x, origin.z, radius, (x, z, cell) =>
			{
				if (!WorldRegions.Unreachable.Contains(cell))
				{
					selected = cell;
					return true;
				}
				return false;
			});
			return selected;
		}

        public static Cell FindNearestUnblocked(Cell origin, int radius)
        {
            if (origin == null || !WorldRegions.Unreachable.Contains(origin))
                return origin;

            Cell selected = null;
			float minDistance = float.MaxValue;
			World.inst.ForEachTileInRadius(origin.x, origin.z, radius, (x, z, cell) =>
            {
				float distanceSquared = Mathff.DistSqrdXZ(origin.Center, cell.Center);
                if (!WorldRegions.Unreachable.Contains(cell) && distanceSquared < minDistance)
                {
                    selected = cell;
					minDistance = distanceSquared;
                }
            });
            return selected;
        }

    }
}
