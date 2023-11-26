using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using System.Reflection;

namespace Elevation.Patches
{

	[HarmonyPatch(typeof(TreeSystem), "SetTree")]
	class TreeSystemPatch
	{
		static void Prefix(Cell cell, ref Vector3 pos)
		{
			CellMeta meta = Grid.Cells.Get(cell);
			if(meta != null)
			{
				pos.y = meta.Elevation;
			}
		}

		public static void UpdateTrees()
		{
			List<Cell> tracked = new List<Cell>();
			foreach (ArrayExt<Cell> landmass in World.inst.cellsToLandmass)
			{
				foreach (Cell cell in landmass.data)
				{
					if (cell != null)
					{
						for (int i = 0; i < cell.TreeAmount; i++)
						{
							tracked.Add(cell);
						}

						TreeSystem.inst.DeleteTreesAt(cell);
					}
				}
			}
			foreach (Cell cell in tracked)
			{
				TreeSystem.inst.PlaceTree(cell);
			}
		}

		public static void UpdateCell(Cell cell)
		{
			int numTrees = cell.TreeAmount;

			TreeSystem.inst.DeleteTreesAt(cell);
			for (int i = 0; i < numTrees; i++)
				TreeSystem.inst.PlaceTree(cell);
		}


	}

	[HarmonyPatch(typeof(TreeSystem), "UpdateAnimMatrixFor")]
	static class TreeSystemAnimPatch
	{
		static void Prefix(ref TreeSystem.AnimTreeData[] ___animTreeData, int i)
		{
			Debug.Assert(___animTreeData != null);

			Debug.Assert(___animTreeData[i].cell != null);

			if (___animTreeData[i].cell == null)
				return;

			CellMeta meta = Grid.Cells.Get(___animTreeData[i].cell);

			if (meta == null)
				return;

			___animTreeData[i].pos = new Vector3(___animTreeData[i].pos.x, meta.Elevation, ___animTreeData[i].pos.z);
		}
	}



}
