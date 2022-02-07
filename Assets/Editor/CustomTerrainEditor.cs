using UnityEditor;
using UnityEngine;
using EditorGUITable;

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


    // fold outs ----------
    bool showRandom = false;
    bool showLoadheights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;
    bool showVoronoiTessellation = false;

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

            EditorGUILayout.Slider(perlinXScale, 0, 0.01f, new GUIContent("X Scale"));
            EditorGUILayout.Slider(perlinYScale, 0, 0.01f, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(perlinXOffset, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(perlinYOffset, 0, 10000, new GUIContent("Y Offset"));

            EditorGUILayout.IntSlider(perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(perlinPersistance, 1, 10, new GUIContent("Persistance"));
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

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Reset Heights", EditorStyles.boldLabel);
        if (GUILayout.Button("Reset Heights"))
        {
            terrain.ResetHeights();
        }

        serializedObject.ApplyModifiedProperties();
    }

   
}
