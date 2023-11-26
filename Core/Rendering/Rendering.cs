using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using Fox.Rendering;
using System.Reflection;
//using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
//using Language.Lua;
using Elevation.Utils;

namespace Elevation
{
    public static class Rendering
    {
        public static void Init()
        {
            RenderingMode.current.Init();
        }

        public static void Setup()
        {
            RenderingMode.current.Setup();
        }

        public static void Tick()
        {
            RenderingMode.current.Tick();
        }

        public static void Update(Cell cell, bool forced = false)
        {
            RenderingMode.current.Update(cell, forced);
        }

        public static void UpdateAll(bool forced = false)
        {
            RenderingMode.current.UpdateAll(forced);
        }
    }

    public abstract class RenderingMode
    {
        public static RenderingMode current { get; } = new VoxelRenderingMode();

        /// <summary>
        /// Called once when the game is first loaded
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// Called after a world is generated
        /// </summary>
        public virtual void Setup()
        {

        }

        /// <summary>
        /// called every frame
        /// </summary>
        public virtual void Tick()
        {

        }

        /// <summary>
        /// Request to update a cell, passing in an optional 'forced' parameter
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="forced"></param>
        public virtual void Update(Cell cell, bool forced = false)
        {

        }

        /// <summary>
        /// Request to update all cells
        /// </summary>
        /// <param name="forced"></param>
        public virtual void UpdateAll(bool forced = false)
        {

        }
    }

    public class InstanceRenderingMode : RenderingMode
    {
        public static float LowerBuffer { get; } = 0.02f;
        public static float TierColorBuffer { get; } = 0.001f;

        public static Material baseMaterial = ColorManager.terrainMat;

        public static MultiMeshSystem meshSystem { get; } = new MultiMeshSystem();

        #region Mesh Correction

        private static Vector2[] GetCorrectedElevationUVFromMesh(Vector3[] points)
        {
            Vector2[] uv = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                uv[i] = new Vector2(1f, points[i].y > -0.5f ? 1f : 0f);
            }
            return uv;
        }


        private static Vector3[] GetCorrectedElevationNormalsFromMesh(Vector3[] points, Vector3[] prevNormals)
        {
            Vector3[] normals = new Vector3[prevNormals.Length];
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].y > 0f)
                    normals[i] = new Vector3(0f, 1f, 0f);
                else
                    normals[i] = prevNormals[i];
            }
            return normals;
        }

        #endregion

        #region Instances

        private static string SystemKey(CellMeta meta)
        {
            //TerrainChunkSelection selection = (TerrainChunkSelection) typeof(TerrainGen).GetMethod("GlobalSpaceToChunkLocal", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(TerrainGen.inst, new object[] { meta.cell.x, meta.cell.z });
            return SystemKey(meta.elevationTier);
        }

        private static string SystemKey(int elevationTier) => $"{elevationTier}";
        
        //private static string SystemKey(int elevationTier, int chunkX, int chunkZ) => $"{chunkX};{chunkZ}:{elevationTier}";

        /// <summary>
        /// Sets up the mesh grid before any elevation data is created
        /// </summary>
        public override void Setup()
        {
            // Clear old mesh data
            //meshSystem.Clear();

            // Get the primitive cube
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh cube = obj.GetComponent<MeshFilter>().mesh;
            //obj.GetComponent<MeshRenderer>().material = TerrainGen.inst.terrainChunks[0].GetComponent<MeshRenderer>().material;

            // Correction
            cube.uv = GetCorrectedElevationUVFromMesh(cube.vertices);

            // Create mesh data
            InitTiers(cube, ColorManager.terrainMat);

            //World.inst.SaveTexture(Mod.helper.modPath + "/texture.png", ColorManager.terrainMat.mainTexture as Texture2D);

            // Destroy unneccessary GameObject
            GameObject.Destroy(obj);
            Mod.dLog("Mesh System initialized");
        }

        private static void InitTiers(Mesh mesh, Material mat)
        {
            for (int tier = ElevationManager.minElevation; tier < ElevationManager.maxElevation + 1; tier++)
            {
                Material newMat =
                //new Material(TerrainGen.inst.terrainChunks[chunk].GetComponent<MeshRenderer>().material);
                //TreeSystem.inst.material;
                    new Material(Shader.Find("Custom/Snow2"));

                newMat.mainTextureScale = new Vector3(1f, ColorManager.tilingConstant * (float)tier - TierColorBuffer);
                //ColorManager.SetElevationMat(tierMat);
                newMat.enableInstancing = true;
                newMat.color = Color.white;
                newMat.mainTexture = ColorManager.elevationMap;
                //newMat.SetTexture("_MainTex", ColorManager.elevationMap);

                meshSystem.AddSystem(SystemKey(tier), mesh, newMat);

                //for (int chunk = 0; chunk < TerrainGen.inst.terrainChunks.Count; chunk++)
                //{
                //    World.inst.SaveTexture(Mod.helper.modPath + $"/chunks/chunk{chunk}.png", (Texture2D)TerrainGen.inst.terrainChunks[chunk].GetComponent<MeshRenderer>().material.GetTexture("_MainTex"));
                //    Mod.dLog($"chunk uvs: {TerrainGen.inst.terrainChunks[chunk].GetComponent<MeshFilter>().mesh.uv.ToStringList()}");
                //    Mod.dLog($"block uvs: {mesh.uv.ToStringList()}");
                //}
            }
        }

        /// <summary>
        /// Configures the mesh grid after elevation data has been generated
        /// </summary>
        public void InitMeshes()
        {
            foreach (CellMeta meta in Grid.Cells)
            {
                if (meta.elevationTier == 0)
                {
                    continue;
                }

                string system = SystemKey(meta);
                System.Tuple<Vector3, Vector3> result = CalculatePositionScale(meta);

                var data = meshSystem[system].Add(result.Item1, Quaternion.identity, result.Item2);

                meta.mesh.system = system;
                meta.mesh.id = data.id;
                meta.mesh.matrix = data.matrix;

            }

            Mod.dLog("Mesh System configured");
        }


        #region Updating
        
        public override void Tick()
        {
            meshSystem.Update();
            foreach (InstanceMeshSystem system in meshSystem._systems.Values)
                system.material.SetFloat("_Snow", TerrainGen.inst.GetSnowFade());
            //baseMaterial.SetFloat("_Snow", TerrainGen.inst.GetSnowFade());
        }

        public override void UpdateAll(bool forced = false)
        {
            // Clear old mesh data
            meshSystem.Clear();

            // Get the primitive cube
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh cube = obj.GetComponent<MeshFilter>().mesh;
            //obj.GetComponent<MeshRenderer>().material = TerrainGen.inst.terrainChunks[0].GetComponent<MeshRenderer>().material;

            // Correction
            cube.uv = GetCorrectedElevationUVFromMesh(cube.vertices);

            // Create mesh data
            InitTiers(cube, ColorManager.terrainMat);

            //World.inst.SaveTexture(Mod.helper.modPath + "/texture.png", ColorManager.terrainMat.mainTexture as Texture2D);

            // Destroy unneccessary GameObject
            GameObject.Destroy(obj);
            Mod.dLog("Mesh system reinitialized");


            foreach (CellMeta meta in Grid.Cells)
                Update(meta.cell, true);

            Mod.dLog("Mesh system updated");

            //    //if (meta.mesh.system == meta.elevationTier.ToString())
            //    //    continue;
            //    //else
            //    //{
            //    //    meshSystem[meta.mesh.system].RemoveAt(meta.mesh.matrix, meta.mesh.id);

            //    //    System.Tuple<Vector3, Vector3> result = CalculateTransformScale(meta);

            //    //    meta.mesh.system = meta.elevationTier.ToString();

            //    //    // OK for struct
            //    //    var data = meshSystem[meta.mesh.system].Add(result.Item1, Quaternion.identity, result.Item2);

            //    //    meta.mesh.id = data.id;
            //    //    meta.mesh.matrix = data.matrix;
            //    //}
            //}
        }

        public override void Update(Cell cell, bool forced = false)
        {
            CellMeta meta = Grid.Cells.Get(cell);
            meta.CheckNull("meta");

            if(meta.mesh.system == null)
            {
                meta.mesh.system = SystemKey(meta);

                System.Tuple<Vector3, Vector3> result = CalculatePositionScale(meta);

                meshSystem[meta.mesh.system].CheckNull("meshSystem with id: " + meta.mesh.system);
                meshSystem[meta.mesh.system].material.SetFloat("_Snow", TerrainGen.inst.GetSnowFade());

                var data = meshSystem[meta.mesh.system].Add(result.Item1, Quaternion.identity, result.Item2);

                meta.mesh.id = data.id;
                meta.mesh.matrix = data.matrix;

                return;
            }

            if (meta.mesh.system == SystemKey(meta) && !forced)
                return;
            else
            {
                meshSystem[meta.mesh.system].CheckNull("meshSystem" + meta.mesh.system);

                meshSystem[meta.mesh.system].RemoveAt(meta.mesh.matrix, meta.mesh.id);

                System.Tuple<Vector3, Vector3> result = CalculatePositionScale(meta);

                meta.mesh.system = SystemKey(meta);
                // OK for struct
                var data = meshSystem[meta.mesh.system].Add(result.Item1, Quaternion.identity, result.Item2);

                meta.mesh.id = data.id;
                meta.mesh.matrix = data.matrix;
            }
        }

        #endregion

        #region Calculation

        /// <summary>
        /// Calculates the scale, position, and material of the item based on the CellMeta given
        /// || OLD
        /// </summary>
        /// <param name="item"></param>
        /// <param name="meta"></param>
        public void Calculate(RenderInstance item, CellMeta meta)
        {
            //CalculateTransform(item, meta);
            CalculateMaterial(item, meta);
        }

        #region Transform

        /// <summary>
        /// Calculates the Translation and Scale of a theoretical Elevation Block on a given CellMeta
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        private static System.Tuple<Vector3, Vector3> CalculatePositionScale(CellMeta meta)
        {
            Cell cell = meta.cell;

            Vector3 calculatedPosition = new Vector3(cell.Center.x, ((ElevationManager.elevationInterval / 2) * meta.elevationTier) - (LowerBuffer / 2), cell.Center.z);
            Vector3 calculatedScale = meta.elevationTier != 0 ? new Vector3(1f, (meta.elevationTier * ElevationManager.elevationInterval) + LowerBuffer, 1f) : Vector3.zero;

            return new System.Tuple<Vector3, Vector3>(calculatedPosition, calculatedScale);
        }

        #endregion

        #region Material

        /// <summary>
        /// Calculates the material for the RenerInstance Elevation Block on the given CellMeta
        /// || OLD
        /// </summary>
        /// <param name="item"></param>
        /// <param name="meta"></param>
        private static void CalculateMaterial(RenderInstance item, CellMeta meta)
        {
            Material mat = new Material(baseMaterial);

            CalculateTiling(mat, meta);
            CalculateColor(mat, meta);

            item.material = mat;
        }

        private static void CalculateTiling(Material mat, CellMeta meta)
        {
            mat.mainTextureScale = new Vector2(1f, ColorManager.tilingConstant * meta.elevationTier - TierColorBuffer);
        }

        private static void CalculateColor(Material mat, CellMeta meta)
        {
            Cell cell = meta.cell;

            Color _base = Color.white;
            Color overlay = TerrainGen.inst.GetOverlayPixelColor(cell.x, cell.z);

            bool overlayPresent = true;

            if (overlay.r == 0 && overlay.g == 0 && overlay.b == 0)
                overlayPresent = false;

            float r;
            float g;
            float b;

            // Overlay calculation from
            // https://en.wikipedia.org/wiki/Blend_modes#Overlay

            if (overlayPresent)
            {
                if (_base.r < 0.5f)
                    r = 1 - (2 * (1 - _base.r) * (1 - overlay.r));
                else
                    r = 2 * _base.r * overlay.r;
                if (_base.g < 0.5f)
                    g = 1 - (2 * (1 - _base.g) * (1 - overlay.g));
                else
                    g = 2 * _base.g * overlay.g;
                if (_base.b < 0.5f)
                    b = 1 - (2 * (1 - _base.b) * (1 - overlay.b));
                else
                    b = 2 * _base.b * overlay.b;
            }
            else
            {
                r = 1f;
                g = 1f;
                b = 1f;
            }




            mat.color = new Color(r, g, b);
            
        }

        #endregion

        #endregion

        #endregion

    }
}
