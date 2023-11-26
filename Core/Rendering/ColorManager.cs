using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Elevation
{
    public static class ColorManager
    {
        private static Dictionary<int, Color> tierColoring = new Dictionary<int, Color>()
        {
            //{1, new Color(0.486f, 0.552f, 0.298f) },
            //{2, new Color(0.709f, 0.729f, 0.380f) },
            //{3, new Color(0.447f, 0.329f, 0.156f) },
            //{4, new Color(0.501f, 0.501f, 0.501f) },
            //{5, new Color(0.360f, 0.360f, 0.360f) },
            //{6, new Color(0.250f, 0.250f, 0.250f) },
            //{7, new Color(0.509f, 0.509f, 0.509f) },
            //{8, new Color(0.803f, 0.796f, 0.796f) }
        };

        public static Texture2D elevationMap;
        public static Material terrainMat { get; set; }
        public static float tilingConstant;

        public static float coloringBias { get; set; } = 0.3f;
        public static Color unreachableColor { get; set; } = new Color(0.3f, 0.3f, 0.3f);

        public static Color GetColor(int elevationTier)
        {
            //TEMP
            //if (elevationTier < 0 || elevationTier > ElevationManager.maxElevation)
            //    return Color.black;
            //if(Settings.inst == null || Settings.inst.c_Coloring == null)
            //    return tierColoring[elevationTier];


            //switch (elevationTier)
            //{
            //    case 1: return Settings.inst.c_Coloring.c_Tiers.t_1.Color.ToUnityColor(); 
            //    case 2: return Settings.inst.c_Coloring.c_Tiers.t_2.Color.ToUnityColor();
            //    case 3: return Settings.inst.c_Coloring.c_Tiers.t_3.Color.ToUnityColor();
            //    case 4: return Settings.inst.c_Coloring.c_Tiers.t_4.Color.ToUnityColor();
            //    case 5: return Settings.inst.c_Coloring.c_Tiers.t_5.Color.ToUnityColor();
            //    case 6: return Settings.inst.c_Coloring.c_Tiers.t_6.Color.ToUnityColor();
            //    case 7: return Settings.inst.c_Coloring.c_Tiers.t_7.Color.ToUnityColor();
            //    case 8: return Settings.inst.c_Coloring.c_Tiers.t_8.Color.ToUnityColor();
            //}

            //return Color.black;

            //SetTierColoring();
            
            return tierColoring.ContainsKey(elevationTier) ? tierColoring[elevationTier] : Color.black;
        }

        public static void SetColor(int elevationTier, Color color)
        {
            if (tierColoring.ContainsKey(elevationTier))
            {
                if (tierColoring[elevationTier] == color)
                    return;

                tierColoring[elevationTier] = color;
            }
            else
                tierColoring.Add(elevationTier, color);
        }

        public static void GetTileColor(int fertility, bool irrigated, out Color normalColor, out Color winterColor)
        {
            normalColor = Color.white;
            winterColor = Color.white;

            if (fertility > 3 || fertility < 0)
                return;

            if (irrigated)
            {
                if (fertility == 0)
                {
                    normalColor = TerrainGen.inst.irrigatedBarrenColor;
                    winterColor = TerrainGen.inst.winterIrrigatedBarrenColor;
                }
                if (fertility == 1)
                {
                    normalColor = TerrainGen.inst.irrigatedTileColor;
                    winterColor = TerrainGen.inst.winterIrrigatedTileColor;
                }
                if (fertility == 2)
                {
                    normalColor = TerrainGen.inst.irrigatedFertileColor;
                    winterColor = TerrainGen.inst.winterIrrigatedFertileColor;
                }
            }
            else
            {
                if (fertility == 0)
                {
                    normalColor = TerrainGen.inst.barrenColor;
                    winterColor = TerrainGen.inst.winterBarrenColor;
                }
                if (fertility == 1)
                {
                    normalColor = TerrainGen.inst.tileColor;
                    winterColor = TerrainGen.inst.winterTileColor;
                }
                if (fertility == 2)
                {
                    normalColor = TerrainGen.inst.fertileColor;
                    winterColor = TerrainGen.inst.winterFertileColor;
                }
            }
        }

        //public static void SetTierColoring()
        //{
        //    if (Settings.inst == null || Settings.inst.c_Coloring == null)
        //        return;
        //    for(int i = 1; i < ElevationManager.maxElevation + 1; i++) 
        //        if (!tierColoring.ContainsKey(i))
        //            tierColoring.Add(i, Color.white);
        //    tierColoring[1] = Settings.inst.c_Coloring.c_Tiers.t_1.Color.ToUnityColor();
        //    tierColoring[2] = Settings.inst.c_Coloring.c_Tiers.t_2.Color.ToUnityColor();
        //    tierColoring[3] = Settings.inst.c_Coloring.c_Tiers.t_3.Color.ToUnityColor();
        //    tierColoring[4] = Settings.inst.c_Coloring.c_Tiers.t_4.Color.ToUnityColor();
        //    tierColoring[5] = Settings.inst.c_Coloring.c_Tiers.t_5.Color.ToUnityColor();
        //    tierColoring[6] = Settings.inst.c_Coloring.c_Tiers.t_6.Color.ToUnityColor();
        //    tierColoring[7] = Settings.inst.c_Coloring.c_Tiers.t_7.Color.ToUnityColor();
        //    tierColoring[8] = Settings.inst.c_Coloring.c_Tiers.t_8.Color.ToUnityColor();
        //}

        public static void Update()
        {
            BakeElevationMap();
            SetElevationMat(terrainMat);
            ElevationManager.UpdateCellMetas();
        }

        public static void Setup()
        {
            tierColoring = Settings.elevationColorPresets["Default"];
            BakeElevationMap();
            SetElevationMat(terrainMat);   
        }

        public static void BakeElevationMap()
        {

            Texture2D tex = new Texture2D(1, ElevationManager.maxElevation - ElevationManager.minElevation, TextureFormat.ARGB32, false);

            for (int i = ElevationManager.maxElevation; i > ElevationManager.minElevation; i--)
            {
                tex.SetPixel(1, i - ElevationManager.maxElevation - 1, tierColoring[i]);
            }
            tex.Apply();

            Mod.dLog("Terrain Map Baked");

            elevationMap = tex;

            World.inst.SaveTexture(Mod.helper.modPath + "/terrainmap.png", elevationMap);
        }



        public static void SetElevationMat(Material material)
        {
            tilingConstant = 1f / (ElevationManager.maxElevation - ElevationManager.minElevation);
            if(material == null)
                material = new Material(Shader.Find("Standard"));

            material.enableInstancing = true;

            material.SetFloat("_Glossiness", 0f);
            material.SetFloat("_Metallic", 0f);

            elevationMap.filterMode = FilterMode.Point;

            material.mainTexture = elevationMap;

            material.color = Color.white;

            Mod.dLog("Terrain Material Setup");

            //if (terrainMat == null)
            //    Mod.Log("could not find terrain material");
        }

        private static Texture2D GetWorldColorTexture()
        {
            Texture2D texture = new Texture2D(World.inst.GridWidth, World.inst.GridWidth);
            return texture;
        }

        private static Texture2D GetOverlayTexture()
        {
            return typeof(TerrainGen).GetField("overlayTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(TerrainGen.inst) as Texture2D;
        }

        public static void Tick()
        {
            if (TerrainGen.inst.terrainChunks == null || TerrainGen.inst.terrainChunks.Count == 0)
                return;

            //TerrainChunk chunk = TerrainGen.inst.terrainChunks[0];
            //terrainMat = chunk.GetComponent<MeshRenderer>().material;
            //terrainMat.SetFloat("_TerritoryYCutoff", ElevationManager.maxElevation * ElevationManager.elevationInterval);
        }
    }

    //[HarmonyPatch(typeof(TerrainChunk), "SetTerritoryTextures")]
    //static class TerritoryTexturePatch
    //{
    //    static void Postfix(Texture2D oldTex, Texture newTex, float blendTime)
    //    {
    //        ColorManager.terrainMat?.SetTexture("_TerritoryTexOld", oldTex);
    //        ColorManager.terrainMat?.SetTexture("_TerritoryTexNew", newTex);
    //        ColorManager.terrainMat?.SetFloat("_TerritoryBlend", 1f);
    //    }
    //}

    //[HarmonyPatch(typeof(TerrainChunk), "UpdateCursorPosition")]
    //static class CursorPosPatch
    //{
    //    static void Postfix(Vector4 packedCursorInfo)
    //    {
    //        ColorManager.terrainMat?.SetVector("_Cursor", packedCursorInfo);
    //    }
    //}

    //[HarmonyPatch(typeof(TerrainChunk), "UpdateCursorColor")]
    //static class CursorColorPatch
    //{
    //    static void Postfix(Color color)
    //    {
    //        ColorManager.terrainMat?.SetColor("_CursorColor", color);
    //    }
    //}

    //[HarmonyPatch(typeof(TerrainChunk), "UpdateDimensions")]
    //static class TerrainDimensionsPatch
    //{
    //    static void Postfix(Vector4 dimensions)
    //    {
    //        ColorManager.terrainMat?.SetVector("_TerrainDimensions", dimensions);
    //    }
    //}

    //[HarmonyPatch(typeof(TerrainChunk), "UpdateHighlight")]
    //static class TerrainHighlightPatch
    //{
    //    static void Postfix(float intensity)
    //    {
    //        ColorManager.terrainMat?.SetFloat("_TerritoryPulse", intensity);
    //    }
    //}

    //[HarmonyPatch(typeof(TerrainChunk), "UpdateFade")]
    //static class TerrainFadePatch
    //{
    //    static void Postfix(float fade)
    //    {
    //        ColorManager.terrainMat?.SetFloat("_TerritoryFade", fade);
    //    }
    //}

    //[HarmonyPatch(typeof(TerrainChunk), "SetSnowFade")]
    //static class TerrainSnowFadePatch
    //{
    //    static void Postfix(float fade)
    //    {
    //        ColorManager.terrainMat?.SetFloat("_SnowAlpha", fade);
    //    }
    //}

    //[HarmonyPatch(typeof(TerrainChunk), "SetOverlayFade")]
    //static class TerrainOverlayFadePatch
    //{
    //    static void Postfix(float endFade)
    //    {
    //        ColorManager.terrainMat?.SetFloat("_OverlayAlpha", endFade);
    //    }
    //}


}
