using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class AssetBundleBuilder : EditorWindow
{
    private string m_OutputPath = string.Empty;
    private string m_SelectedAssetBundle = string.Empty;

    [MenuItem("Tools/Build AssetBundle")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleBuilder>("AssetBundle Builder");
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        var assetBundles = AssetDatabase.GetAllAssetBundleNames();
        foreach (var assetBundleName in assetBundles)
        {
            var isSelected = assetBundleName == m_SelectedAssetBundle;
            isSelected = GUILayout.Toggle(isSelected, assetBundleName);

            if (isSelected)
            {
                m_SelectedAssetBundle = assetBundleName;
            }
        }
        GUILayout.EndHorizontal();

        // Display a text field for the user to input the output path
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Path:", GUILayout.Width(80));
        m_OutputPath = EditorGUILayout.TextField(m_OutputPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            m_OutputPath = EditorUtility.OpenFolderPanel("Select Output Folder", "", "");
            Repaint();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Build AssetBundle"))
        {
            BuildAssetBundle();
        }
    }

    private void BuildAssetBundle()
    {
        if (string.IsNullOrEmpty(m_OutputPath) || string.IsNullOrEmpty(m_SelectedAssetBundle))
        {
            Debug.LogError("Output path is not specified!");
            return;
        }

        var options = BuildAssetBundleOptions.DisableLoadAssetByFileName | BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension;

        var buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = m_SelectedAssetBundle;
        buildMap[0].assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(m_SelectedAssetBundle);

        // Build the asset bundle
        BuildPipeline.BuildAssetBundles(m_OutputPath, buildMap, options, BuildTarget.StandaloneWindows64);
        CleanupAfterBuildingAssetBundle(m_OutputPath);

        Debug.Log("AssetBundle has been built successfully at: " + m_OutputPath);
    }

    /// <summary>
    /// Unity (sometimes?) creates an empty bundle with the same name as the folder, so we delete it.
    /// </summary>
    private void CleanupAfterBuildingAssetBundle(string outputPath)
    {
        var directoryName = Path.GetFileName(outputPath);
        var emptyBundlePath = Path.Combine(outputPath, directoryName);
        if (File.Exists(emptyBundlePath))
        {
            File.Delete(emptyBundlePath);
        }

        var emptyManifestPath = emptyBundlePath + ".manifest";
        if (File.Exists(emptyManifestPath))
        {
            File.Delete(emptyManifestPath);
        }
    }
}

