using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ConvertURPToStandard : EditorWindow
{
    private List<Material> urpMaterials = new List<Material>();
    private Vector2 scrollPosition;
    private bool scanned = false;

    [MenuItem("Tools/Convert URP to Standard Shaders")]
    public static void ShowWindow()
    {
        GetWindow<ConvertURPToStandard>("URP to Standard Converter");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Convert URP Materials to Standard", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Scan Project for URP Materials", GUILayout.Height(30)))
        {
            ScanForURPMaterials();
        }

        EditorGUILayout.Space();

        if (scanned)
        {
            EditorGUILayout.LabelField($"Found {urpMaterials.Count} materials using URP shaders", EditorStyles.helpBox);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            foreach (Material mat in urpMaterials)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(mat, typeof(Material), false);
                EditorGUILayout.LabelField(mat.shader.name, GUILayout.Width(300));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            if (urpMaterials.Count > 0)
            {
                EditorGUILayout.HelpBox("This will convert all URP materials to Standard shaders. This cannot be undone automatically. Make sure to backup your project first!", MessageType.Warning);
                EditorGUILayout.Space();

                if (GUILayout.Button("Convert All to Standard", GUILayout.Height(40)))
                {
                    if (EditorUtility.DisplayDialog("Convert Materials", 
                        $"Are you sure you want to convert {urpMaterials.Count} materials to Standard shaders?", 
                        "Yes, Convert", "Cancel"))
                    {
                        ConvertMaterials();
                    }
                }
            }
        }
    }

    private void ScanForURPMaterials()
    {
        urpMaterials.Clear();
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        
        int progress = 0;
        foreach (string guid in materialGuids)
        {
            progress++;
            EditorUtility.DisplayProgressBar("Scanning Materials", 
                $"Scanning {progress}/{materialGuids.Length}", 
                (float)progress / materialGuids.Length);

            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && mat.shader != null)
            {
                string shaderName = mat.shader.name;
                if (shaderName.Contains("Universal Render Pipeline") || 
                    shaderName.Contains("URP") ||
                    shaderName.StartsWith("Shader Graphs/"))
                {
                    urpMaterials.Add(mat);
                }
            }
        }

        EditorUtility.ClearProgressBar();
        scanned = true;
        Debug.Log($"Scan complete. Found {urpMaterials.Count} URP materials.");
    }

    private void ConvertMaterials()
    {
        int converted = 0;
        int failed = 0;

        for (int i = 0; i < urpMaterials.Count; i++)
        {
            Material mat = urpMaterials[i];
            EditorUtility.DisplayProgressBar("Converting Materials", 
                $"Converting {i + 1}/{urpMaterials.Count}: {mat.name}", 
                (float)i / urpMaterials.Count);

            if (ConvertMaterial(mat))
            {
                converted++;
            }
            else
            {
                failed++;
            }
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Conversion Complete", 
            $"Converted {converted} materials successfully.\n{failed} materials failed to convert.", 
            "OK");

        ScanForURPMaterials();
    }

    private bool ConvertMaterial(Material mat)
    {
        if (mat == null || mat.shader == null)
            return false;

        string shaderName = mat.shader.name;
        Shader newShader = null;

        Color mainColor = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
        Texture mainTex = mat.HasProperty("_BaseMap") ? mat.GetTexture("_BaseMap") : null;
        Texture normalMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
        float normalScale = mat.HasProperty("_BumpScale") ? mat.GetFloat("_BumpScale") : 1f;
        float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
        float smoothness = mat.HasProperty("_Smoothness") ? mat.GetFloat("_Smoothness") : 0.5f;
        Texture metallicMap = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;
        Texture occlusionMap = mat.HasProperty("_OcclusionMap") ? mat.GetTexture("_OcclusionMap") : null;
        float occlusionStrength = mat.HasProperty("_OcclusionStrength") ? mat.GetFloat("_OcclusionStrength") : 1f;
        Color emissionColor = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;
        Texture emissionMap = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;

        if (shaderName.Contains("Lit") && !shaderName.Contains("Unlit"))
        {
            newShader = Shader.Find("Standard");
        }
        else if (shaderName.Contains("Unlit"))
        {
            newShader = Shader.Find("Unlit/Texture");
        }
        else if (shaderName.Contains("Particles"))
        {
            if (shaderName.Contains("Lit"))
                newShader = Shader.Find("Standard");
            else
                newShader = Shader.Find("Particles/Standard Unlit");
        }
        else if (shaderName.Contains("Sprite"))
        {
            newShader = Shader.Find("Sprites/Default");
        }
        else
        {
            newShader = Shader.Find("Standard");
        }

        if (newShader == null)
        {
            Debug.LogWarning($"Could not find suitable shader for {mat.name} (was using {shaderName})");
            return false;
        }

        Undo.RecordObject(mat, "Convert to Standard");
        mat.shader = newShader;

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", mainColor);
        
        if (mat.HasProperty("_MainTex") && mainTex != null)
            mat.SetTexture("_MainTex", mainTex);
        
        if (mat.HasProperty("_BumpMap") && normalMap != null)
        {
            mat.SetTexture("_BumpMap", normalMap);
            if (mat.HasProperty("_BumpScale"))
                mat.SetFloat("_BumpScale", normalScale);
        }
        
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);
        
        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);
        
        if (mat.HasProperty("_MetallicGlossMap") && metallicMap != null)
            mat.SetTexture("_MetallicGlossMap", metallicMap);
        
        if (mat.HasProperty("_OcclusionMap") && occlusionMap != null)
        {
            mat.SetTexture("_OcclusionMap", occlusionMap);
            if (mat.HasProperty("_OcclusionStrength"))
                mat.SetFloat("_OcclusionStrength", occlusionStrength);
        }
        
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.SetColor("_EmissionColor", emissionColor);
            if (emissionColor != Color.black)
                mat.EnableKeyword("_EMISSION");
        }
        
        if (mat.HasProperty("_EmissionMap") && emissionMap != null)
            mat.SetTexture("_EmissionMap", emissionMap);

        EditorUtility.SetDirty(mat);
        Debug.Log($"Converted {mat.name} from {shaderName} to {newShader.name}");
        return true;
    }
}
