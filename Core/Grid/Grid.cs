using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using Newtonsoft.Json;
using Elevation.Patches;

namespace Elevation
{
    
    /// <summary>
    /// Stores metadata about a type TSource in a type TMetadata
    /// </summary>
    /// <typeparam name="TSource">The type to keep metadata on</typeparam>
    /// <typeparam name="TMetadata">The type that will store metaddata</typeparam>
    public abstract class Metadata<TSource, TMetadata>
        : IEnumerable
        where TMetadata : Meta<TSource>

    {
        protected static Dictionary<object, TMetadata> lookup;

        public int Count => lookup.Count;

        public Metadata()
        {
            lookup = new Dictionary<object, TMetadata>();
        }

        public Metadata(int capacity)
        {
            lookup = new Dictionary<object, TMetadata>(capacity);
        }

        /// <summary>
        /// Gets a <typeparamref name="TMetadata"/> by a <typeparamref name="TSource"/> object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public TMetadata Get(TSource obj) => lookup.ContainsKey(GetKey(obj)) ? lookup[GetKey(obj)] : default(TMetadata);

        /// <summary>
        /// Gets a <typeparamref name="TMetadata"/> by key. Make sure you know how keys are stored in this metadata to reference one by key. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TMetadata Get(object key) => lookup.ContainsKey(key) ? lookup[key] : default(TMetadata);

        /// <summary>
        /// Adds a <typeparamref name="TSource"/>, <typeparamref name="TMetadata"/> pair
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="meta"></param>
        public void Add(TSource obj, TMetadata meta)
        {
            lookup.Add(GetKey(obj), meta);
        }

        public TMetadata Add(TSource obj) => Activator.CreateInstance(typeof(TMetadata), obj) as TMetadata;

        /// <summary>
        /// Removes a <typeparamref name="TMetadata"/> by a <typeparamref name="TSource"/> object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void Remove(TSource obj)
        {
            if (lookup.ContainsKey(GetKey(obj)))
                lookup.Remove(GetKey(obj));
        }


        /// <summary>
        /// Removes a <typeparamref name="TMetadata"/> by key, make sure you know how keys are stored in this metadata to reference one by key. 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void Remove(object key)
        {
            if (lookup.ContainsKey(key))
                lookup.Remove(key);
        }
        public List<TMetadata> GetAll()
        {
            return lookup.Values.ToList();
        }

        /// <summary>
        /// Resets the metadata
        /// </summary>
        public virtual void Reset()
        {
            lookup.Clear();
        }

        /// <summary>
        /// Override behaviour for key storing; default uses object.GetHashCode
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public virtual object GetKey(TSource obj)
        {
            return obj.GetHashCode();
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)lookup.Values).GetEnumerator();
        }
    }

    public class CellMetadata : Metadata<Cell, CellMeta>
    {
        public CellMetadata() : base()
        {

        }

        public CellMetadata(int capacity) : base(capacity)
        {

        }

        public override object GetKey(Cell obj)
        {
            return GetPositionalID(obj);
        }

        public override void Reset()
        {
            base.Reset();

            foreach (CellMeta meta in lookup.Values)
            {
                meta.Reset();
            }

            lookup = new Dictionary<object, CellMeta>(World.inst.GridWidth * World.inst.GridHeight);
        }


        public CellMeta Get(int x, int z) => lookup.ContainsKey(GetPositionalID(x, z)) ? lookup[GetPositionalID(x, z)] : null;
        
        public CellMeta Get(Vector3 position) => Get((int)position.x, (int)position.z);

        public static string GetPositionalID(Cell cell)
        {
            if (cell == null)
                return null;

            return cell.x.ToString() + "_" + cell.z.ToString();
        }

        public static string GetPositionalID(int x, int z)
        {
            return x.ToString() + "_" + z.ToString();
        }
    }

    public class BuildingMetadata : Metadata<Building, BuildingMeta>
    {
        public BuildingMetadata() : base()
        {

        }

        public BuildingMetadata(int capacity) : base(capacity)
        {

        }

        public override object GetKey(Building obj)
        {
            return obj.guid;
        }
    }

    public static class Grid
    {
        public static CellMetadata Cells { get; } = new CellMetadata();
        public static BuildingMetadata Buildings { get; } = new BuildingMetadata();

        #region Initialization

        /// <summary>
        /// Call after world init but before generation; resets data and sets up cells
        /// </summary>
        public static void Setup()
        {
            Cells.Reset();
            Buildings.Reset();

            Broadcast.BuildingAddRemove.ListenAny(new OnActionListener<OnBuildingAddRemove>(HandleBuildingAddRemove));

            for (int x = 0; x < World.inst.GridWidth; x++)
            {
                for (int z = 0; z < World.inst.GridHeight; z++)
                {
                    Cell cell = World.inst.GetCellData(x, z);
                    if (cell != null)
                    {
                        CellMeta meta = new CellMeta(cell);
                        Cells.Add(cell, meta);
                    }
                }
            }
        }

        #endregion

        public static void HandleBuildingAddRemove(object sender, OnBuildingAddRemove @event)
        {
            // Handle roads
            Roads.HandleBuildingChange(@event);
        }

        #region Save/Load


        // Cells
        public static string SaveCells()
        {
            SerializableDictionary<string, int> all = new SerializableDictionary<string, int>();

            foreach (CellMeta meta in Cells)
                all.Add(CellMetadata.GetPositionalID(meta.cell), meta.elevationTier);

            return JsonConvert.SerializeObject(all);
        }

        public static void LoadCells(string json)
        {
            Cells.Reset();
            try
            {
                if (json != null)
                {
                    SerializableDictionary<string, int> all = JsonConvert.DeserializeObject<SerializableDictionary<string, int>>(json);

                    foreach (string id in all.Keys)
                    {
                        string[] split = id.Split(new char[]
                            {
                        '_'
                            });

                        Vector3 pos = new Vector3(float.Parse(split[0]), 0f, float.Parse(split[1]));
                        Cell cell = World.inst.GetCellData(pos);
                        
                        if (cell == null || cell.deepWater)
                            continue;

                        CellMeta meta = new CellMeta(cell);
                        meta.elevationTier = all[id];
                        Cells.Add(cell, meta);
                    }
                }

                ElevationManager.RefreshTerrain();
            }
            catch (Exception ex)
            {
                Mod.Log($"Load save exception: \n\t{ex}");
            }   
        }

        // Buildings
        public static string SaveBuildings()
        {
            return JsonConvert.SerializeObject(BuildingsSaveData.Save());
        }

        public static void LoadBuildings(string json)
        {
            BuildingsSaveData.Load(JsonConvert.DeserializeObject<BuildingsSaveData>(json));
        }

        public class BuildingsSaveData
        {
            public List<string> scaffoldedCells;
            public List<string> dugoutCells;

            public static BuildingsSaveData Save()
            {
                BuildingsSaveData data = new BuildingsSaveData();

                data.scaffoldedCells = Scaffolding.scaffoldedCells;
                data.dugoutCells = Dugout.dugoutCells;

                return data;
            }

            public static void Load(BuildingsSaveData data)
            {
                if (data == null)
                    return;

                Scaffolding.scaffoldedCells = data.scaffoldedCells;
                Dugout.dugoutCells = data.dugoutCells;
            }
        }

        #endregion
    }

    public static class GridUtils 
    {
        public static CellMeta GetMeta(this Cell cell) => cell != null ? Grid.Cells.Get(cell) : null;

        public static BuildingMeta GetMeta(this Building building) => Grid.Buildings.Get(building);
    }

}
