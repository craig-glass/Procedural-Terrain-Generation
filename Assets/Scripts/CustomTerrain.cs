using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]

public class CustomTerrain : MonoBehaviour
{
    public Vector2 randomHeightRange = new Vector2(0, 0.1f);
    public Terrain terrain;
    public TerrainData terrainData;

    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);
    public bool resetTerrain = true;

    // Perlin noise
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinXOffset = 0;
    public int perlinYOffset = 0;
    public int perlinOctaves = 2;
    public float perlinPersistance = 8;
    public float perlinHeightScale = 0.09f;

    // Voronoi
    public float fallOff = 0.5f;
    public float dropOff = 0.5f;
    public int peakCount = 2;
    public float minHeight = 0.1f;
    public float maxHeight = 0.7f;
    public enum VoronoiType { Linear = 0, Power = 1, Combined = 2, Sinewave = 3 };
    public VoronoiType voronoiType = VoronoiType.Combined;

    // Multiple Perlin
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8;
        public float mPerlinHeightScale = 0.09f;
        public int mPerlinOffsetX = 0;
        public int mPerlinOffsetY = 0;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()
    };


    float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            return terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        }

        else
        {
            return new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        }
    }

    public void VoronoiTessellation()
    {
        float[,] heightMap = GetHeightMap();
        
        for (int i = 0; i < peakCount; i++)
        {
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, terrainData.heightmapResolution),
                UnityEngine.Random.Range(minHeight, maxHeight), UnityEngine.Random.Range(0, terrainData.heightmapResolution));

            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            else
                continue;

            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0, 0), new Vector2(terrainData.heightmapResolution, terrainData.heightmapResolution));

            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x, y)) / maxDistance;
                        float h;

                        if (voronoiType == VoronoiType.Linear)
                        {
                            h = peak.y - distanceToPeak * fallOff;
                        }
                        else if (voronoiType == VoronoiType.Power)
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak, dropOff) * fallOff;
                        }
                        else if (voronoiType == VoronoiType.Combined)
                        {
                            h = peak.y - distanceToPeak * fallOff - Mathf.Pow(distanceToPeak, dropOff);
                        }                           
                        else 
                        {
                            h = peak.y - Mathf.Pow(distanceToPeak * 3, fallOff) - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / dropOff;
                        }
                         
                        

                        if (heightMap[x, y] < h)
                            heightMap[x, y] = h;
                    }
                }
            }
        }


       

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();

        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                heightMap[x, y] += Utils.fBM(x * perlinXScale, y * perlinYScale, perlinOctaves, perlinPersistance, perlinXOffset, perlinYOffset) * perlinHeightScale;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int y = 0; y < terrainData.heightmapResolution; y++)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x++)
            {
                foreach (PerlinParameters p in perlinParameters)
                {
                    heightMap[x, y] += Utils.fBM(x * p.mPerlinXScale,
                        y * p.mPerlinYScale,
                        p.mPerlinOctaves, p.mPerlinPersistance,
                        p.mPerlinOffsetX, p.mPerlinOffsetY) * p.mPerlinHeightScale;
                }
            }
        }
        terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewPerlin()
    {
        perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < perlinParameters.Count; i++)
        {
            if (!perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(perlinParameters[i]);
            }
        }
        if (keptPerlinParameters.Count == 0)
        {
            keptPerlinParameters.Add(perlinParameters[0]);
        }
        perlinParameters = keptPerlinParameters;
    }

    public void LoadTexture()
    {
        float[,] heightMap;
        heightMap = GetHeightMap();

        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                heightMap[x, z] += heightMapImage.GetPixel((int)(x * heightMapScale.x), (int)(z * heightMapScale.z)).grayscale * heightMapScale.y;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                heightMap[x, z] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetHeights()
    {
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {                
                heightMap[x, z] = 0;
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
    }

    private void OnEnable()
    {
        Debug.Log("Initialising Terrain Data");
        terrain = GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }   

    
}
