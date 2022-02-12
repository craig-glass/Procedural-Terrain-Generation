using UnityEditor;
using UnityEngine;
using EditorGUITable;
using System.IO;

[CustomEditor(typeof(CustomTerrain)), CanEditMultipleObjects]

public class CustomTerrainEditor : Editor
{
    // properties ----------
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapImage;
    SerializedProperty heightMapScale;

    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinXOffset;
    SerializedProperty perlinYOffset;

    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;
    SerializedProperty resetTerrain;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    SerializedProperty fallOff;
    SerializedProperty dropOff;
    SerializedProperty peakCount;
    SerializedProperty minHeight;
    SerializedProperty maxHeight;
    SerializedProperty voronoiType;

    
    SerializedProperty roughness;
    SerializedProperty heightDampenerPower;
    SerializedProperty smoothAmount;

    GUITableState splatMapTable;
    SerializedProperty splatHeights;
    SerializedProperty splatPerlinXScale;
    SerializedProperty splatPerlinYScale;
    SerializedProperty splatPerlinBlendAmount;
    SerializedProperty splatOffset;

    SerializedProperty maxTrees;
    SerializedProperty treeSpacing;
    GUITableState vegetationTable;
    SerializedProperty vegetation;

    // fold outs ----------
    bool showRandom = false;
    bool showLoadheights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;
    bool showVoronoiTessellation = false;
    bool showMidPointDisplacement = false;
    bool showSmoothTerrain = false;
    bool showSplatmaps = false;
    bool showDisplayHeightMap = false;
    bool showVegetation = false;

    // Current Height Map
    Texture2D currentHeightMapTexture;
    string filename = "CurrentHeightMap";

    private void OnEnable()
    {
        randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        heightMapImage = serializedObject.FindProperty("heightMapImage");
        heightMapScale = serializedObject.FindProperty("heightMapScale");
        resetTerrain = serializedObject.FindProperty("resetTerrain");

        perlinXScale = serializedObject.FindProperty("perlinXScale");
        perlinYScale = serializedObject.FindProperty("perlinYScale");
        perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        perlinYOffset = serializedObject.FindProperty("perlinYOffset");

        perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");

        perlinParameterTable = new GUITableState("perlinParameterTable");
        perlinParameters = serializedObject.FindProperty("perlinParameters");

        fallOff = serializedObject.FindProperty("fallOff");
        dropOff = serializedObject.FindProperty("dropOff");
        peakCount = serializedObject.FindProperty("peakCount");
        minHeight = serializedObject.FindProperty("minHeight");
        maxHeight = serializedObject.FindProperty("maxHeight");
        voronoiType = serializedObject.FindProperty("voronoiType");

        roughness = serializedObject.FindProperty("roughness");
        heightDampenerPower = serializedObject.FindProperty("heightDampenerPower");
        smoothAmount = serializedObject.FindProperty("smoothAmount");

        splatMapTable = new GUITableState("splatMapTable");
        splatHeights = serializedObject.FindProperty("splatHeights");
        splatPerlinXScale = serializedObject.FindProperty("splatPerlinXScale");
        splatPerlinYScale = serializedObject.FindProperty("splatPerlinYScale");
        splatPerlinBlendAmount = serializedObject.FindProperty("splatPerlinBlendAmount");
        splatOffset = serializedObject.FindProperty("splatOffset");

        currentHeightMapTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);

        maxTrees = serializedObject.FindProperty("maxTrees");
        treeSpacing = serializedObject.FindProperty("treeSpacing");
        vegetationTable = new GUITableState("vegetationTable");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain)target;
        EditorGUILayout.PropertyField(resetTerrain);

        showRandom = EditorGUILayout.Foldout(showRandom, "Random");

        if (showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights Between Random Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(randomHeightRange);
            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }          
                       
        }

        showLoadheights = EditorGUILayout.Foldout(showLoadheights, "Load Heights");
        if (showLoadheights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights From Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(heightMapImage);
            EditorGUILayout.PropertyField(heightMapScale);

            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        // Perlin noise
        showPerlin = EditorGUILayout.Foldout(showPerlin, "Perlin");
        if (showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Add perlin noise to heights", EditorStyles.boldLabel);

            EditorGUILayout.Slider(perlinXScale, 0f, 0.01f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0f, 0.01f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinXOffset, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinYOffset, 0, 10000, new GUIContent("Y Offset"));

            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, -5, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(perlinHeightScale, 0, 1, new GUIContent("Height Scale"));

            if (GUILayout.Button("Add Perlin"))
            {
                terrain.Perlin();
            }
        }

        // Multiple Perlin
        showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin");
        if (showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            perlinParameterTable = GUITableLayout.DrawTable(perlinParameterTable, serializedObject.FindProperty("perlinParameters"));

            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();            
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
            }
        }

        // Voronoi Tessellation
        showVoronoiTessellation = EditorGUILayout.Foldout(showVoronoiTessellation, "Voronoi Tessellation");
        if (showVoronoiTessellation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Voronoi Parameters", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(peakCount, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(fallOff, 0, 10, new GUIContent("Falloff"));
            EditorGUILayout.Slider(dropOff, 0, 10, new GUIContent("Dropoff"));
            EditorGUILayout.Slider(minHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(maxHeight, 0, 1, new GUIContent("Max Height"));
            EditorGUILayout.PropertyField(voronoiType);

            if (GUILayout.Button("Add Voronoi"))
            {
                terrain.VoronoiTessellation();
            }
        }

        // Midpoint Displacement
        showMidPointDisplacement = EditorGUILayout.Foldout(showMidPointDisplacement, "Midpoint Displacement");
        if (showMidPointDisplacement)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Midpoint Displacement", EditorStyles.boldLabel);
            EditorGUILayout.Slider(roughness, 0f, 10f, new GUIContent("Roughness"));
            EditorGUILayout.Slider(heightDampenerPower, 0f, 10f, new GUIContent("Height Dampener Power"));

            if (GUILayout.Button("Add Midpoint Displacement"))
            {
                terrain.MidPointDisplacement();
            }
        }

        // Smooth Terrain
        showSmoothTerrain = EditorGUILayout.Foldout(showSmoothTerrain, "Smooth Terrain");
        if (showSmoothTerrain)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Smooth Terrain", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(smoothAmount, 1, 200, new GUIContent("Smooth Terrain"));

            if (GUILayout.Button("Smooth Terrian"))
            {
                terrain.SmoothTerrain();
            }
        }

        // Splatmaps
        showSplatmaps = EditorGUILayout.Foldout(showSplatmaps, "Splatmaps");
        if (showSplatmaps)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Splatmaps", EditorStyles.boldLabel);

            splatMapTable = GUITableLayout.DrawTable(splatMapTable, serializedObject.FindProperty("splatHeights"));

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewSplatHeight();
            }
            if (GUILayout.Button("-"))
            {
                terrain.RemoveSplatHeight();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Perlin Parameters", EditorStyles.boldLabel);

            EditorGUILayout.Slider(splatPerlinXScale, 0.01f, 0.1f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(splatPerlinYScale, 0.01f, 0.1f, new GUIContent("Y Scale"));
            EditorGUILayout.Slider(splatOffset, 0.001f, 0.1f, new GUIContent("Offset"));
            EditorGUILayout.Slider(splatPerlinBlendAmount, 0.01f, 0.5f, new GUIContent("Blend Amount"));
            if (GUILayout.Button("Apply SplatMaps"))
            {
                terrain.SplatMaps();
            }
        }

        // Vegetation
        showVegetation = EditorGUILayout.Foldout(showVegetation, "Vegetation");
        if (showVegetation)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Vegetation", EditorStyles.boldLabel);

            EditorGUILayout.IntSlider(maxTrees, 1, 20000, new GUIContent("Maximum Trees"));
            EditorGUILayout.IntSlider(treeSpacing, 1, 20, new GUIContent("Tree Spacing"));

            vegetationTable = GUITableLayout.DrawTable(vegetationTable, serializedObject.FindProperty("vegetation"));

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("+"))
            {
                terrain.AddNewVegetation();
            }

            if (GUILayout.Button("-"))
            {
                terrain.RemoveVegetation();
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Vegetation"))
            {
                terrain.PlantVegetation();
            }

            GUILayout.EndHorizontal();
        }

        // Reset Heights
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Reset Heights", EditorStyles.boldLabel);
        if (GUILayout.Button("Reset Heights"))
        {
            terrain.ResetHeights();
        }

       

        // Current Height Map
        showDisplayHeightMap = EditorGUILayout.Foldout(showDisplayHeightMap, "Height Map");
        if (showDisplayHeightMap)
        {
            int wSize = (int)(EditorGUIUtility.currentViewWidth - 100);

            EditorGUILayout.TextField("HeightMap Name", filename);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(currentHeightMapTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Refresh", GUILayout.Width(wSize)))
            {
                float[,] heightMap = terrain.terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
                for (int y = 0; y < terrain.terrainData.heightmapResolution; y++)
                {
                    for (int x = 0; x < terrain.terrainData.heightmapResolution; x++)
                    {
                        currentHeightMapTexture.SetPixel(x, y, new Color(heightMap[x, y],
                            heightMap[x, y],
                            heightMap[x, y], 1));
                    }
                }
                currentHeightMapTexture.Apply();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save", GUILayout.Width(wSize)))
            {
                byte[] bytes = currentHeightMapTexture.EncodeToPNG();
                System.IO.Directory.CreateDirectory(Application.dataPath + "/SavedTextures");
                File.WriteAllBytes(Application.dataPath + "/SavedTextures/" + filename + ".png", bytes);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

   
}
