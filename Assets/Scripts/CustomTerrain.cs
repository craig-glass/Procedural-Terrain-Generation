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

    // Mid Point Displacement
    public float roughness = 2.0f;
    public float heightDampenerPower = 2.0f;

    public int smoothAmount = 1;

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

    // Splatmaps
    [System.Serializable]
    public class SplatHeights
    {

        public Texture2D texture = null;
        public float minSlope = 0;
        public float maxSlope = 1.5f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50.0f, 50.0f);
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public bool remove = false;
    }
    public float splatPerlinXScale = 0.01f;
    public float splatPerlinYScale = 0.01f;
    public float splatPerlinBlendAmount = 0.2f;
    public float splatOffset = 0.01f;

    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()
    };

    // Vegetation
    [System.Serializable]
    public class Vegetation
    {
        public GameObject mesh = null;
        public float minHeight = 0;
        public float maxHeight = 1;
        public float minSlope = 0;
        public float maxSlope = 90f;
        public float minScale = 0.5f;
        public float maxScale = 1f;
        public Color colour1 = Color.white;
        public Color colour2 = Color.white;
        public Color lightmapColour = Color.white;
        public float minRotation = 0;
        public float maxRotation = 360;
        public float density = 0.5f;
        public bool remove = false;
    }
    public int maxTrees = 1000;
    public int treeSpacing = 5;

    public List<Vegetation> vegetation = new List<Vegetation>()
    {
        new Vegetation()
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

    public void MidPointDisplacement()
    {
        float[,] heightMap = GetHeightMap();
        int width = terrainData.heightmapResolution - 1;
        int squareSize = width;
        float height = (float)squareSize / 2.0f * 0.01f;

        float heightDampener = (float)Mathf.Pow(heightDampenerPower, -1 * roughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;    

        while (squareSize > 0)
        {
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    heightMap[midX, midY] = (float)(
                        (heightMap[x, y] +
                        heightMap[cornerX, y] +
                        heightMap[x, cornerY] +
                        heightMap[cornerX, cornerY]) / 4.0f +
                        UnityEngine.Random.Range(-height, height));
                }
            }

            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int)(x + squareSize / 2.0f);
                    midY = (int)(y + squareSize / 2.0f);

                    pmidXR = (int)(midX + squareSize);
                    pmidYU = (int)(midY + squareSize);
                    pmidXL = (int)(midX - squareSize);
                    pmidYD = (int)(midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width - 1 || pmidYU >= width - 1) continue;

                    heightMap[midX, y] = (float)(
                        (heightMap[midX, midY] +
                        heightMap[x, y] +
                        heightMap[midX, pmidYD] +
                        heightMap[cornerX, y]) / 4.0f +
                        UnityEngine.Random.Range(-height, height));

                    heightMap[x, midY] = (float)(
                        (heightMap[x, cornerY] +
                        heightMap[midX, midY] +
                        heightMap[x, y] +
                        heightMap[pmidXL, midY]) / 4.0f +
                        UnityEngine.Random.Range(-height, height));

                    heightMap[midX, cornerY] = (float)(
                        (heightMap[midX, pmidYU] +
                        heightMap[cornerX, cornerY] +
                        heightMap[midX, midY] +
                        heightMap[x, cornerY]) / 4.0f +
                        UnityEngine.Random.Range(-height, height));

                    heightMap[cornerX, midY] = (float)(
                        (heightMap[cornerX, cornerY] +
                        heightMap[pmidXR, midY] +
                        heightMap[cornerX, y] +
                        heightMap[midX, midY]) / 4.0f +
                        UnityEngine.Random.Range(-height, height));
                }
            }

            squareSize = (int)(squareSize / 2.0f);
            height *= heightDampener;
        }


        terrainData.SetHeights(0, 0, heightMap);
    }

    public void SmoothTerrain()
    {
        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        float smoothProgress = 0;
        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);

        for (int i = 0; i < smoothAmount; i++)
        {
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    float averageHeight = 0;

                    if (y == 0 && x > 0 && x < terrainData.heightmapResolution - 2)
                    {
                        averageHeight = (
                            heightMap[x, y] +
                            heightMap[x + 1, y] +
                            heightMap[x - 1, y] +
                            heightMap[x + 1, y + 1] +
                            heightMap[x - 1, y + 1] +
                            heightMap[x, y + 1]) / 6.0f;
                    }

                    else if (x == 0 && y > 0 && y < terrainData.heightmapResolution - 2)
                    {
                        averageHeight = (
                            heightMap[x, y] +
                            heightMap[x + 1, y] +
                            heightMap[x + 1, y + 1] +
                            heightMap[x + 1, y - 1] +
                            heightMap[x, y + 1] +
                            heightMap[x, y - 1]) / 6.0f;
                    }
                    else if (x == terrainData.heightmapResolution - 2 && y > terrainData.heightmapResolution - 2 && y < 0)
                    {
                        averageHeight = (
                            heightMap[x, y] +
                            heightMap[x - 1, y] +
                            heightMap[x - 1, y + 1] +
                            heightMap[x - 1, y - 1] +
                            heightMap[x, y + 1] +
                            heightMap[x, y - 1]) / 6.0f;
                    }
                    else if (y > 0 && x > 0 && y < terrainData.heightmapResolution - 2 && x < terrainData.heightmapResolution - 2)
                    {
                        averageHeight = (
                            heightMap[x, y] +
                            heightMap[x + 1, y] +
                            heightMap[x - 1, y] +
                            heightMap[x + 1, y + 1] +
                            heightMap[x - 1, y - 1] +
                            heightMap[x + 1, y - 1] +
                            heightMap[x - 1, y + 1] +
                            heightMap[x, y - 1] +
                            heightMap[x, y + 1]) / 9.0f;
                    }

                    heightMap[x, y] = averageHeight;
                }
            }
            smoothProgress++;
            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / smoothAmount);
        }

        terrainData.SetHeights(0, 0, heightMap);
        EditorUtility.ClearProgressBar();
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

    float GetSteepness(float[,] heightmap, int x, int y, int width, int height)
    {
        float h = heightmap[x, y];
        int nx = x + 1;
        int ny = y + 1;

        // if on the upper edge of the map find gradient by going backward
        if (nx > width - 1) nx = x - 1;
        if (ny > height - 1) ny = y - 1;

        float dx = heightmap[nx, y] - h;
        float dy = heightmap[x, ny] - h;
        Vector2 gradient = new Vector2(dx, dy);

        float steep = gradient.magnitude;

        return steep;
    }

    public void SplatMaps()
    {
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[splatHeights.Count];
        int spindex = 0;
        foreach (SplatHeights sh in splatHeights)
        {
            newSplatPrototypes[spindex] = new TerrainLayer();
            newSplatPrototypes[spindex].diffuseTexture = sh.texture;
            newSplatPrototypes[spindex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spindex].tileSize = sh.tileSize;
            newSplatPrototypes[spindex].diffuseTexture.Apply(true);

            string path = "Assets/New Terrain Layer " + spindex + ".terrainlayer";
            AssetDatabase.CreateAsset(newSplatPrototypes[spindex], path);
            spindex++;
            Selection.activeObject = this.gameObject;
        }
        terrainData.terrainLayers = newSplatPrototypes;

        float[,] heightMap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        float[,,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[terrainData.alphamapLayers];
                for (int i = 0; i < splatHeights.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x * splatPerlinXScale, y * splatPerlinYScale) * splatPerlinBlendAmount;
                    float offset = splatOffset + noise;
                    float thisHeightStart = splatHeights[i].minHeight - offset;
                    float thisHeightStop = splatHeights[i].maxHeight + offset;
                    float steepness = GetSteepness(heightMap, x, y, terrainData.heightmapResolution, terrainData.heightmapResolution);

                    if ((heightMap[x, y] >= thisHeightStart && heightMap[x, y] <= thisHeightStop) &&
                        (steepness >= splatHeights[i].minSlope && steepness <= splatHeights[i].maxSlope))
                    {
                        splat[i] = 1;
                    }
                }
                NormalizeVector(splat);
                for (int j = 0; j < splatHeights.Count; j++)
                {
                    splatmapData[x, y, j] = splat[j];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    void NormalizeVector(float[] v)
    {
        float total = 0;
        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        for (int i = 0; i < v.Length; i++)
        {
            v[i] /= total;
        }
    }

    public void AddNewSplatHeight()
    {
        splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeight()
    {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();
        for (int i = 0; i < splatHeights.Count; i++)
        {
            if (!splatHeights[i].remove)
            {
                keptSplatHeights.Add(splatHeights[i]);
            }
        }
        if (keptSplatHeights.Count == 0)
        {
            keptSplatHeights.Add(splatHeights[0]);
        }
        splatHeights = keptSplatHeights;
    }

    public void PlantVegetation()
    {
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[vegetation.Count];
        int tindex = 0;
        foreach (Vegetation v in vegetation)
        {
            newTreePrototypes[tindex] = new TreePrototype();
            newTreePrototypes[tindex].prefab = v.mesh;
            tindex++;
        }
        terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();

        for (int z = 0; z < terrainData.heightmapResolution; z += treeSpacing)
        {
            for (int x = 0; x < terrainData.heightmapResolution; x += treeSpacing)
            {
                for (int tp = 0; tp < terrainData.treePrototypes.Length; tp++)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > vegetation[tp].density) break;

                    float thisHeight = terrainData.GetHeight(x, z) / terrainData.size.y;
                    float thisHeightStart = vegetation[tp].minHeight;
                    float thisHeightEnd = vegetation[tp].maxHeight;

                    float steepness = terrainData.GetSteepness(x / (float)terrainData.heightmapResolution, z / (float)terrainData.heightmapResolution);

                    if ((thisHeight >= thisHeightStart && thisHeight <= thisHeightEnd) &&
                        steepness >= vegetation[tp].minSlope && steepness <= vegetation[tp].maxSlope)
                    {
                        TreeInstance instance = new TreeInstance();
                        instance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.heightmapResolution,
                            terrainData.GetHeight(x, z) / terrainData.size.y,
                            (z + UnityEngine.Random.Range(-5.0f, 5.0f)) / terrainData.heightmapResolution);

                        Vector3 treeWorldPos = new Vector3(instance.position.x * terrainData.heightmapResolution,
                            instance.position.y * terrainData.heightmapResolution, instance.position.z * terrainData.heightmapResolution) + this.transform.position;

                        //RaycastHit hit;
                        //int layerMask = 1 << 6;
                        //if (Physics.Raycast(treeWorldPos + new Vector3(0, 10, 0), -Vector3.up, out hit, 100, layerMask) ||
                        //    Physics.Raycast(treeWorldPos - new Vector3(0, 10, 0), Vector3.up, out hit, 100, layerMask))
                        //{
                        //    float treeHeight = (hit.point.y - this.transform.position.y) / terrainData.size.y;
                        //    instance.position = new Vector3(instance.position.x,
                        //                                     treeHeight,
                        //                                     instance.position.z);
                        //}


                        instance.position = new Vector3(instance.position.x * terrainData.heightmapResolution / terrainData.alphamapWidth,
                            instance.position.y,
                            instance.position.z * terrainData.heightmapResolution / terrainData.alphamapHeight);

                        instance.rotation = UnityEngine.Random.Range(vegetation[tp].minRotation, vegetation[tp].maxRotation);
                        instance.prototypeIndex = tp;
                        instance.color = Color.Lerp(vegetation[tp].colour1,
                            vegetation[tp].colour2,
                            UnityEngine.Random.Range(0.0f, 1.0f));
                        instance.lightmapColor = vegetation[tp].lightmapColour;
                        float treeScale = UnityEngine.Random.Range(vegetation[tp].minScale, vegetation[tp].maxScale);
                        instance.heightScale = treeScale;
                        instance.widthScale = treeScale;

                        allVegetation.Add(instance);
                        if (allVegetation.Count >= maxTrees) goto TREESDONE;



                    }

                }
            }
        }
    TREESDONE:
        terrainData.treeInstances = allVegetation.ToArray();
    }

    public void AddNewVegetation()
    {
        vegetation.Add(new Vegetation());
    }

    public void RemoveVegetation()
    {
        List<Vegetation> keptVegetation = new List<Vegetation>();
        for (int i = 0; i < vegetation.Count; i++)
        {
            if (!vegetation[i].remove)
            {
                keptVegetation.Add(vegetation[i]);
            }

        }
        if (keptVegetation.Count == 0)
        {
            keptVegetation.Add(vegetation[0]);
        }
        vegetation = keptVegetation;
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
