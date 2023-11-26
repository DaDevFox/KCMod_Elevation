using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Elevation.Utils;

namespace Elevation
{
    public class MapGenerator
    {
        public struct CellData
        {
            public bool valid;
            public float y;
            public Cell cell;
        }

        public static List<TerrainFeature> TerrainFeatures = new List<TerrainFeature>()
        {
            //new Pillar()
        };

        public static int featurePlaceTriesPerLandmass = 5;
        static List<TerrainFeature> placedFeatures = new List<TerrainFeature>();

        private static Dictionary<Cell,CellData> cellsData = new Dictionary<Cell, CellData>();

        public static float generatorSeededState;
        public static float fertilityWeightage = 0.3f;

        public static int terracing = 1;

        public static float Amplitude {
            get;
            set;
        } = 0.7f;
        //{
        //    get
        //    {
        //        return Settings.inst.c_Generator.c_Advanced.c_Noise.Amplitude;
        //    }
        //}

        public static float Scale{
            get;
            set; 
        } = 50f;
        //{
        //    get
        //    {
        //        return Settings.inst.c_Generator.c_Advanced.c_Noise.Scale;
        //    }
        //}

        public static bool doSmoothing = true;
        //{
        //    get
        //    {
        //        return Settings.inst.c_Generator.c_Advanced.c_Noise.Smoothing;
        //    }
        //}

        public static void Generate()
        {
            // Set Seed
            SRand.SetSeed(World.inst.seed);
            generatorSeededState = SRand.value;

            // Generate
            GenerateBase();

            // Extra layers
            if (doSmoothing)
                ApplySmoothing();

            // Apply Changes
            ApplyTerrain();

            // TEMP: WIP
            TryPlaceTerrainFeatures();

            WorldRegions.Marked = false;
        }

        public static void Reset()
        {
            cellsData.Clear();
            placedFeatures.Clear();
        }

        public static void GenerateBase()
        {
            Reset();
            for (int landmass = 0; landmass < World.inst.NumLandMasses; landmass++)
            {
                foreach(Cell cell in World.inst.cellsToLandmass[landmass].data)
                {
                    CellData data = new CellData
                    {
                        valid = false
                    };
                    if (cell != null && ElevationManager.ValidTileForElevation(cell))
                    {
                        try
                        {
                            data.valid = true;
                            data.cell = cell;
                            float yValue = 0f;
                            float noiseValue = Mathf.PerlinNoise(cell.x/Scale + generatorSeededState, cell.z/Scale + generatorSeededState) * Amplitude;
                           
                            float weightage = GetFertilityDistanceWeightage(cell);
                            yValue = noiseValue * weightage * (ElevationManager.maxElevation - ElevationManager.minElevation) + ElevationManager.minElevation;

                            data.y = yValue;

                            int y = Clamp(yValue);

                            ElevationManager.SetElevation(cell, y);

                            cellsData.Add(cell, data);
                        }
                        catch(Exception ex)
                        {
                            DebugExt.HandleException(ex);
                        }
                        
                    }
                }
            }
            Mod.helper.Log("Base Noise Generated");
            
        }

        private static float GetFertilityDistanceWeightage(Cell origin)
        {
            int maxRadius = Mathf.CeilToInt(1f / fertilityWeightage) * 2;
            float min = maxRadius + 1;
            World.inst.ForEachTileInRadius(origin.x, origin.z, maxRadius, delegate (int x, int z, Cell cell)
            {
                if(cell.fertile > 0)
                {
                    if(Vector3.Distance(cell.Center,origin.Center) * ((3 - cell.fertile) * fertilityWeightage) < min)
                    {
                        min = Vector3.Distance(cell.Center, origin.Center) * ((3 - cell.fertile) * fertilityWeightage);
                    }
                }
            });
            return min;
        }

        public static void ApplySmoothing()
        {
            Dictionary<Cell, CellData> newData = new Dictionary<Cell, CellData>();
            foreach(Cell cell in cellsData.Keys)
            {
                CellData data = cellsData[cell];
                if (data.valid && cell != null) 
                {
                    CellMeta meta = Grid.Cells.Get(cell);
                    float[] surrounding = GetSurroundingTiles(data);
                    if (meta != null)
                    {
                        List<float> list = new List<float>();
                        foreach(float f in surrounding)
                        {
                            if(f >= 0f)
                            {
                                list.Add(f);
                            }
                        }
                        list.Add(data.y);

                        newData.Add(cell, new CellData { 
                            valid = true,
                            y = Average(list),
                            cell = cell
                        }) ;
                    }
                    

                }
            }
            cellsData = newData;
            
            Mod.helper.Log("Smoothing Applied");
        }
        
        static void ApplyTerrain()
        {
            foreach(CellData data in cellsData.Values)
            {
                if (data.valid)
                {
                    ElevationManager.SetElevation(data.cell, Clamp(data.y));
                }
            }
        }


        static int Clamp(float num)
        {
            int rounded = 0;
            

            switch (Settings.elevationBiasType)
            {
                case ElevationBiasType.Rounded: 
                   rounded = (int)Utils.Util.RoundToFactor(num, terracing);
                    break;
                case ElevationBiasType.Max:
                    rounded = (int)Utils.Util.CeilToFactor(num, terracing);
                    break;
                case ElevationBiasType.Min:
                    rounded = (int)Utils.Util.FloorToFactor(num, terracing);
                    break;
                default:
                    break;
            }

            return Mathf.Clamp(rounded,ElevationManager.minElevation,ElevationManager.maxElevation);
        }




        private static CellData GetCellData(Cell cell)
        {
            if (cell != null)
            {
                return cellsData.ContainsKey(cell) ? cellsData[cell] : new CellData() { valid = false };
            }
            return new CellData()
            {
                valid = false
            };
        }


        private static float Average(List<float> values)
        {
            float sum = 0f;
            foreach(float f in values)
            {
                sum += f;
            }
            return sum / values.Count;
        }


        private static float[] GetSurroundingTiles(CellData data)
        {
            float[] values = new float[4];

            Cell cell = data.cell;

            Cell cell1 = World.inst.GetCellData(cell.x - 1, cell.z);
            Cell cell2 = World.inst.GetCellData(cell.x, cell.z - 1);
            Cell cell3 = World.inst.GetCellData(cell.x + 1, cell.z );
            Cell cell4 = World.inst.GetCellData(cell.x, cell.z + 1);

            CellData data1 = GetCellData(cell1);
            CellData data2 = GetCellData(cell2);
            CellData data3 = GetCellData(cell3);
            CellData data4 = GetCellData(cell4);

            values[0] = data1.valid ? data1.y : -1f;
            values[1] = data2.valid ? data2.y : -1f;
            values[2] = data3.valid ? data1.y : -1f;
            values[3] = data4.valid ? data4.y : -1f;

            return values;
        }


        public static void TryPlaceTerrainFeatures()
        {
            foreach(TerrainFeature terrainFeature in TerrainFeatures)
            {
                for (int landmass = 0; landmass < World.inst.NumLandMasses; landmass++)
                {
                    for(int place = 0; place < featurePlaceTriesPerLandmass; place++)
                    {
                        Cell rand = World.inst.cellsToLandmass[landmass].data[SRand.Range(0, World.inst.cellsToLandmass[landmass].Count)];

                        if (ElevationManager.ValidTileForElevation(rand))
                            if (terrainFeature.TestPlacement(rand))
                                placedFeatures.Add(terrainFeature.Create(rand));
                    }
                }
            }
        }


        public static void DoTerrainFeatureEffects()
        {
            foreach(TerrainFeature feature in placedFeatures)
            {
                foreach (Cell cell in feature.affected)
                {
                    if (ElevationManager.ValidTileForElevation(cell))
                    {
                        CellMeta meta = Grid.Cells.Get(cell);
                        if (meta != null)
                        {
                            meta.elevationTier = feature.Get(cell);
                        }
                    }
                }
            }
        }




        

        #region Fertility Noise Values Transpiler

        //[HarmonyPatch(typeof(TerrainGen), "GenerateFertileTiles")]
        class FertileNoisePatch
        {

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                try
                {

                    // Fertility value 1

                    //Push desired destination var's memory address onto stack
                    FieldInfo f1 = typeof(MapGenerator).
                        GetField("fertility_value1", BindingFlags.Static | BindingFlags.Public);
                    codes.Add(new CodeInstruction(OpCodes.Ldflda, f1));

                    //Push the value of value1 local onto stack
                    codes.Add(new CodeInstruction(OpCodes.Ldloc_0));

                    //Store value into fertility_value1
                    codes.Add(new CodeInstruction(OpCodes.Stind_R4));


                    // Fertility value 2

                    //Push desired destination var's memory address onto stack
                    FieldInfo f2 = typeof(MapGenerator).
                        GetField("fertility_value2", BindingFlags.Static | BindingFlags.Public);
                    codes.Add(new CodeInstruction(OpCodes.Ldflda, f2));

                    //Push the value of value2 local onto stack
                    codes.Add(new CodeInstruction(OpCodes.Ldloc_1));

                    //Store value into fertility_value2
                    codes.Add(new CodeInstruction(OpCodes.Stind_R4));
                }
                catch (Exception ex)
                {
                    DebugExt.HandleException(ex);
                }

                return codes.AsEnumerable();
            }
        }

        #endregion
        
    }
}
