using System;
using System.Collections.Generic;
using UnityEngine;
using Fox.Rendering.VoxelRendering;
using Elevation;
using Elevation.Utils;
using System.Reflection;

namespace Elevation
{
    /// <summary>
    /// Rendering mode that utilizes voxel style face-based rendering; makes better face culling and multiple tiles on one XZ grid space possible, also uses subdivided faces allowing diveting
    /// </summary>
    public class VoxelRenderingMode : RenderingMode
    {
        private VoxelDataRenderer[] chunks;
        private Mesh[] meshes;
        private Material[] materials;

        public bool setup { get; private set; } = false;

        public override void Init()
        {
            ColorManager.terrainMat = new Material(Shader.Find("Custom/Snow2"));

            ColorManager.terrainMat.enableInstancing = true;
            ColorManager.terrainMat.color = Color.white;
            ColorManager.terrainMat.mainTexture = ColorManager.elevationMap;
        }

        public override void Setup()
        {
            try
            {
                if (chunks == null || chunks.Length != TerrainGen.inst.terrainChunks.Count)
                {
                    chunks = new VoxelDataRenderer[TerrainGen.inst.terrainChunks.Count];
                    meshes = new Mesh[TerrainGen.inst.terrainChunks.Count];
                    materials = new Material[TerrainGen.inst.terrainChunks.Count];
                }

                for (int i = 0; i < TerrainGen.inst.terrainChunks.Count; i++)
                {
                    if (chunks[i] == null)
                        chunks[i] = new VoxelDataRenderer();
                    else
                    {
                        chunks[i].data = null;
                        chunks[i].mesh = null;
                        meshes[i] = null;
                        materials[i] = null;
                    }
                }

                for (int i = 0; i < TerrainGen.inst.terrainChunks.Count; i++)
                {
                    TerrainChunk chunk = TerrainGen.inst.terrainChunks[i];
                    Voxel[,,] chunkData = new Voxel[chunk.SizeX, ElevationManager.maxElevation + 1, chunk.SizeZ];
                    
                    //Texture2D tex = (Texture2D) typeof(TerrainChunk).GetField("terrainTexture", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(chunk);
                    //World.inst.SaveTexture(Mod.helper.modPath + $"/chunk{i}.png", tex);

                    for (int z = 0; z < chunk.SizeZ; z++)
                    {
                        for (int x = 0; x < chunk.SizeX; x++)
                        {
                            // TerrainChunk x and z are double the world scaling in the base game for some reason
                            CellMeta meta = Grid.Cells.Get(chunk.x/2 + x, chunk.z/2 + z);
                            for (int y = 0; y <= ElevationManager.maxElevation; y++)
                            {
                                try
                                {

                                    chunkData[x, y, z] = new Voxel();

                                    if (meta != null && y < meta.elevationTier)
                                    {
                                        chunkData[x, y, z].opacity = 1f;
                                        chunkData[x, y, z].uvOffset = new Vector2(y, 0f);
                                    }
                                    else
                                    {
                                        chunkData[x, y, z].opacity = 0f;
                                    }

                                    chunkData[x, y, z].index = new Vector3Int(x, y, z);
                                }
                                catch(Exception ex)
                                {
                                    Mod.Log($"Exception updating chunk {i}: ${ex}");
                                }
                            }
                        }
                    }

                    chunks[i].dimensions = new Vector3Int(chunkData.GetLength(0), chunkData.GetLength(1), chunkData.GetLength(2));
                    chunks[i].position = new Vector3Int(chunk.x/2, 0, chunk.z/2);
                    chunks[i].data = chunkData;
                    meshes[i] = chunks[i].Rebuild(chunkData);
                    materials[i] = chunk.GetComponent<MeshRenderer>().material;

                    Mod.dLog($"chunk {i} (x={chunk.x},z={chunk.z},w={chunk.SizeX * 2},h={chunk.SizeZ * 2}): built mesh given {chunkData.Length} voxels and with {chunks[i].mesh.vertexCount} vertices");
                }
            }
            catch(Exception ex)
            {
                Mod.Log(ex);
            }

            setup = true;

            Mod.Log("Voxel Rendering Mode Initialized");
        }

        public override void Update(Cell cell, bool forced = false)
        {
            if (!setup)
                return;

            if (cell == null)
                return;

            int globalX = cell.x;
            int globalZ = cell.z;

            int chunkIndexX = globalX / TerrainGen.inst.chunkSize;
            int chunkIndexZ = globalZ / TerrainGen.inst.chunkSize;

            int index = chunkIndexX + chunkIndexZ * Mathf.CeilToInt((float)World.inst.GridWidth / (float)TerrainGen.inst.chunkSize);

            VoxelDataRenderer chunk = ((VoxelRenderingMode)RenderingMode.current).chunks[index];


            CellMeta meta = Grid.Cells.Get(cell);
            try
            {
                int vertsBefore = chunk.mesh.vertexCount;

                int chunkX = globalX - chunk.position.x;
                int chunkZ = globalZ - chunk.position.z;

                if (meta && chunk != null)
                    for (int y = ElevationManager.minElevation; y < ElevationManager.maxElevation; y++)
                        if (chunk.GetAt(chunkX, y, chunkZ))
                            chunk.data[chunkX, y, chunkZ].opacity = y < meta.elevationTier ? 1f : 0f;

                meshes[index] = chunk.Rebuild();

                Mod.dLog($"chunk with index {index} modified; {vertsBefore} vertices before, {chunk.mesh.vertexCount} vertices after");
            }
            catch(Exception ex)
            {
                Mod.Log(ex);
            }
        }

        public override void UpdateAll(bool forced = false)
        {
            Setup();
        }

        public override void Tick()
        {
            if (!setup)
                return;

            ColorManager.terrainMat.SetFloat("_Snow", TerrainGen.inst.GetSnowFade());
            
            for(int i = 0; i < chunks.Length; i++)
            {
                Graphics.DrawMesh(meshes[i], Matrix4x4.TRS(chunks[i].position, Quaternion.identity, new Vector3(1f, ElevationManager.elevationInterval, 1f)), Settings.useTerrainTexture ? materials[i] : ColorManager.terrainMat, 0);
            }
        }

        public VoxelDataRenderer GetAt(Vector3 position) => GetAt((int)position.x, (int)position.z);

        public VoxelDataRenderer GetAt(int x, int z)
        {
            int chunkX = x / TerrainGen.inst.chunkSize;
            int chunkZ = z / TerrainGen.inst.chunkSize;


            int index = chunkX + chunkZ * Mathf.CeilToInt(World.inst.GridWidth / (float)TerrainGen.inst.chunkSize);

            return chunks[index];
        }

        public Voxel GetVoxelAt(int globalX, int globalY, int globalZ)
        {
            VoxelDataRenderer chunk = GetAt(globalX, globalZ);
            if (chunk == null)
                return null;
            return chunk.GetAt(globalX % TerrainGen.inst.chunkSize, globalY, globalZ % TerrainGen.inst.chunkSize);
        }

        public Voxel GetVoxelAt(Vector3Int globalPosition) => GetVoxelAt(globalPosition.x, globalPosition.y, globalPosition.z);

        public Voxel GetVoxelAt(VoxelDataRenderer chunk, int localX, int localY, int localZ) => chunk.GetAt(localX, localY, localZ);
    }
}

namespace Fox.Rendering.VoxelRendering
{

    /// <summary>
    /// Builds a face-based mesh construction of a group of voxels
    /// </summary>
    public class VoxelDataRenderer
    {
        public static float buffer { get; } = 0.00001f;

        #region Settings

        public Vector3Int position;

        /// <summary>
        /// The number of elements in each dimension of this chunk
        /// </summary>
        public Vector3Int dimensions = new Vector3Int(10, 10, 10);
        /// <summary>
        /// The world space size 1 voxel index represents
        /// </summary>
        public float voxelSize = 1f;

        public float yTilingScale = 1f;

        /// <summary>
        /// divets create minor variations in the center point of each quad face
        /// <para>Set to 0 for no divets (uses simple quad rendering rather than 9-point quads)</para>
        /// </summary>
        public float divetVariation = 0.2f;

        /// <summary>
        /// When set to true, mesh will use 32 bit integers rather than 16 bit; 
        /// <para>doubles memory footprint but allows up to 4 billion vertices versus the traditional 65535 vertex limit.  </para>
        /// </summary>
        public bool _32BitBuffer = true;

        public bool closeOuterEdges = true;
        public bool divetAll = false;

        public Vector2 textureSize;

        #endregion

        public Voxel[,,] data;
        private Dictionary<string, List<int>> vertexPositionIndexLookup = new Dictionary<string, List<int>>();

        public Mesh mesh;

        public Voxel this[int x, int y, int z]
        {
            private set
            {
                if (InDimensions(new Vector3Int(x, y, z)))
                    data[x, y, z] = value;
            }
            get
            {
                if (InDimensions(new Vector3Int(x, y, z)))
                    return data[x, y, z];
                return null;
            }
        }

        public void Reset()
        {
            data = new Voxel[dimensions.x, dimensions.y, dimensions.z];
            vertexPositionIndexLookup.Clear();

            Loop((x, y, z) =>
            {
                data[x, y, z] = new Voxel();

                data[x, y, z].index = new Vector3Int(x, y, z);
                data[x, y, z].opacity = 1f;
            });
        }

        public Mesh Rebuild(Voxel[,,] data = null)
        {
            if (data != null)
            {
                this.data = data;
                dimensions = new Vector3Int(this.data.GetLength(0), this.data.GetLength(1), this.data.GetLength(2));
            }

            vertexPositionIndexLookup.Clear();
            mesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            int master = 0;

            if (_32BitBuffer)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            bool hasTransparent = false;

            List<string> toDivet = new List<string>();

            Loop((voxel) =>
            {
                int x = voxel.index.x;
                int y = voxel.index.y;
                int z = voxel.index.z;

                Vector3[] faceNormals = GetFaces(voxel);

                if (faceNormals.Length > 0)
                    hasTransparent = true;

                foreach (Vector3 normal in faceNormals)
                {
                    bool divet = divetVariation > 0f && normal.y > 0f;
                    divet |= divetAll;

                    Quad face = divet ?
                            RendererUtil.NinePointQuad(
                            new Vector2(voxelSize, voxelSize),
                            new Vector3(x, y, z) * voxelSize,
                            master) :
                            RendererUtil.Quad(
                            new Vector2(voxelSize, voxelSize),
                            new Vector3(x, y, z) * voxelSize,
                            master);
                    face = face.SetRotation(
                            normal,
                            new Vector3(voxelSize * 0.5f, voxelSize * 0.5f, voxelSize * 0.5f) +
                            new Vector3(x, y, z) * voxelSize,
                            Vector3.up
                            );



                    for (int i = 0; i < face.vertices.Length; i++)
                    {
                        Vector3 vertex = face.vertices[i];
                        string index = $"{Elevation.Utils.Util.RoundToFactor(vertex.x, 0.1f)}_{Elevation.Utils.Util.RoundToFactor(vertex.y, 0.1f)}_{Elevation.Utils.Util.RoundToFactor(vertex.z, 0.1f)}";
                        if (!vertexPositionIndexLookup.ContainsKey(index))
                            vertexPositionIndexLookup.Add(index, new List<int>());
                        vertexPositionIndexLookup[index].Add(master + i);
                    }

                    if (divet) 
                    {
                        Vector3Int adjacent = position + new Vector3Int(0, 1, 0);

                        toDivet.Add(GetIndex(face.vertices[3]));

                        // specifically for up-facing diveted faces only
                        //Voxel adjacentN = GetAt(adjacent + new Vector3Int(0, 0, 1));
                        //Voxel adjacentE = GetAt(adjacent + new Vector3Int(1, 0, 0));
                        //Voxel adjacentS = GetAt(adjacent + new Vector3Int(0, 0, -1));
                        //Voxel adjacentW = GetAt(adjacent + new Vector3Int(-1, 0, 0));

                        //if (adjacentN != null && adjacentN.opacity <= 0f)
                        //{
                        //    toDivet.Add(GetIndex(face.vertices[6]));
                        //    toDivet.Add(GetIndex(face.vertices[7]));
                        //    toDivet.Add(GetIndex(face.vertices[8]));
                        //}
                        //if (adjacentE != null && adjacentE.opacity <= 0f)
                        //{
                        //    toDivet.Add(GetIndex(face.vertices[4]));
                        //    toDivet.Add(GetIndex(face.vertices[5]));
                        //    toDivet.Add(GetIndex(face.vertices[6]));
                        //}
                        //if (adjacentS != null && adjacentS.opacity <= 0f)
                        //{
                        //    toDivet.Add(GetIndex(face.vertices[0]));
                        //    toDivet.Add(GetIndex(face.vertices[1]));
                        //    toDivet.Add(GetIndex(face.vertices[4]));
                        //}
                        //if (adjacentW != null && adjacentW.opacity <= 0f)
                        //{
                        //    toDivet.Add(GetIndex(face.vertices[0]));
                        //    toDivet.Add(GetIndex(face.vertices[2]));
                        //    toDivet.Add(GetIndex(face.vertices[8]));
                        //}
                    }


                    if (Settings.useTerrainTexture)
                        for (int i = 0; i < face.vertices.Length; i++)
                            face.uvs[i] = new Vector2((float)(x)/((float)dimensions.x) + buffer, ((float)z)/((float)dimensions.z) + buffer);
                    else
                        for (int i = 0; i < face.vertices.Length; i++)
                            face.uvs[i] = new Vector2(1f - buffer, ((float)y) / ((float)ElevationManager.maxElevation) - buffer);

                    vertices.AddRange(face.vertices);
                    triangles.AddRange(face.triangles);
                    normals.AddRange(face.normals);
                    uv.AddRange(face.uvs);

                    master += face.vertices.Length;
                }
            });

            //IMS426120816
            //IMN500812677
            //LMS890122356
            //IMN2663310
            //IMS108469851

            if (divetVariation > 0f)
            {
                foreach (string index in toDivet)
                {
                    string[] indexParts = index.Split('_');
                    Vector3 pos = new Vector3(float.Parse(indexParts[0]), float.Parse(indexParts[1]), float.Parse(indexParts[2]));
                    float divet = Mathf.PerlinNoise(pos.x / (dimensions.x * 10f), pos.z / (dimensions.z * 10f)) * divetVariation;
                    foreach (int i in vertexPositionIndexLookup[index])
                        vertices[i] = new Vector3(vertices[i].x, vertices[i].y + divet, vertices[i].z);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.uv = uv.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            Mod.dLog($"Mesh built; has transparent: {hasTransparent}");

            return mesh;
        }


        private string GetIndex(Vector3 vertex) => $"{Elevation.Utils.Util.RoundToFactor(vertex.x, 0.1f)}_{Elevation.Utils.Util.RoundToFactor(vertex.y, 0.1f)}_{Elevation.Utils.Util.RoundToFactor(vertex.z, 0.1f)}";

        public Vector3[] GetFaces(Voxel voxel)
        {
            if (voxel.opacity <= 0f)
                return new Vector3[0];

            Vector3[] toCheck = new Vector3[]
            {
                new Vector3(1f, 0f, 0f),
                new Vector3(-1f, 0f, 0f),

                new Vector3(0f, 1f, 0f),
                new Vector3(0f, -1f, 0f),

                new Vector3(0f, 0f, 1f),
                new Vector3(0f, 0f, -1f),
            };

            List<Vector3> found = new List<Vector3>();


            foreach (Vector3 vector in toCheck)
            {
                Voxel neighbor = GetAt(new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z) + voxel.index);
                if (neighbor != null)
                    if (neighbor.opacity <= 0f || !InDimensions(neighbor.index)) // if not in dimensions but not null (meaning voxel exists), it is an external voxel, not an open face. 
                        found.Add(vector);
                if (neighbor == null && closeOuterEdges)
                    found.Add(vector);
            }

            return found.ToArray();
        }

        /// <summary>
        /// Gets access to tile outside of this render data (for border tiles)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual Voxel External(Vector3Int index)
        {
            try
            {
                bool exists = ((VoxelRenderingMode)RenderingMode.current) != null;
                if (index != null && exists && !InDimensions(index))
                {
                    VoxelDataRenderer adjacent = ((VoxelRenderingMode)RenderingMode.current).GetAt(position + index);
                    if (adjacent != null)
                    {
                        Vector3Int adjacentIndex = new Vector3Int(Mathf.Abs(index.x % TerrainGen.inst.chunkSize), index.y, index.z % TerrainGen.inst.chunkSize);
                        if (adjacent.data != null && adjacent.InDimensions(adjacentIndex))
                            return adjacent.data[adjacentIndex.x, adjacentIndex.y, adjacentIndex.z];
                    }

                    //Mod.dLog($"{Mathf.Abs(index.globalX % TerrainGen.inst.chunkSize)}, {index.y}, {index.globalZ % TerrainGen.inst.chunkSize}");
                }
                //if(!InDimensions(index) && ((VoxelRenderingMode)RenderingMode.current) != null)
                //    return ((VoxelRenderingMode)RenderingMode.current).GetVoxelAt(position + index);
            }catch(Exception ex)
            {
                Mod.Log(ex);
            }
            return null;
        }

        public Voxel GetAt(int x, int y, int z) => GetAt(new Vector3Int(x, y, z));

        public Voxel GetAt(Vector3Int index)
        {
            try
            {
                if (InDimensions(index))
                {
                    return data[index.x, index.y, index.z];
                }
                Voxel external = External(index);
                if (external != null && external.opacity <= 0f)
                {
                    return external;
                }
            }catch(Exception ex)
            {
                Mod.dLog(ex);
            }
            return null;
        }

        private bool InDimensions(Vector3Int vector)
        {
            return (vector.x >= 0 && vector.x < dimensions.x)
                && (vector.y >= 0 && vector.y < dimensions.y)
                && (vector.z >= 0 && vector.z < dimensions.z);
        }



        #region Utils

        /// <summary>
        /// Loops through the chunk in the order globalX, y, globalZ and calls a function (passing in the globalX, y, and globalZ integer coordinates of that voxel)
        /// </summary>
        /// <param name="action"></param>
        public void Loop(Action<int, int, int> action)
        {
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    for (int z = 0; z < data.GetLength(2); z++)
                    {
                        action(x, y, z);
                    }
                }
            }
        }

        /// <summary>
        /// Loops through the chunk in the order globalX, y, globalZ and calls a function (passing in the voxel)
        /// </summary>
        /// <param name="action"></param>
        public void Loop(Action<Voxel> action)
        {
            Loop((x, y, z) =>
            {
                if (data[x, y, z] != null)
                    action(data[x, y, z]);
            });
        }



        #endregion

    }

    [Serializable]
    public class Voxel
    {
        /// <summary>
        /// Index of this voxel within its domain
        /// </summary>
        public Vector3Int index;
        /// <summary>
        /// TEMP: any opacity lower than 1f will mean the voxel will not be rendered
        /// </summary>
        public float opacity;

        /// <summary>
        /// UV coordinate of this voxel through its domain's texture
        /// </summary>
        public Vector2 uvOffset;

        public static implicit operator bool(Voxel obj) => obj != null;
    }

}