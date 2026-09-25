using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Browser builds leave the 3D monster models out (they are ~2 GB, far too big
// for WebGL). Everything in Resources is always packed into a build, so the
// model prefabs are moved aside for the duration of a WebGL build and moved
// back afterwards. With no model, LabyrinthObject shows the card art instead.
// Desktop builds are unaffected.
[InitializeOnLoad]
public class WebGLBuild : IPreprocessBuildWithReport
{
    const string ResourcesFolder = "Assets/Resources";
    const string ParkingFolder = "Assets/WebGLExcludedModels";
    const string OutputPath = "Builds/WebGL";

    static bool buildingViaMenu;

    public int callbackOrder => 0;

    static double nextRestoreCheck;

    static WebGLBuild()
    {
        // Recover from a crash or an interrupted WebGL build: whenever no
        // build is running, parked models are always put back.
        EditorApplication.update += () =>
        {
            if (EditorApplication.timeSinceStartup < nextRestoreCheck) return;
            nextRestoreCheck = EditorApplication.timeSinceStartup + 2.0;
            if (!buildingViaMenu && !BuildPipeline.isBuildingPlayer && AssetDatabase.IsValidFolder(ParkingFolder))
                RestoreModels();
        };
    }

    [MenuItem("Tools/Build WebGL (card art)")]
    static void BuildWebGL()
    {
        if (buildingViaMenu || BuildPipeline.isBuildingPlayer) return;

        var scenes = new List<string>();
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            if (s.enabled) scenes.Add(s.path);

        buildingViaMenu = true;
        try
        {
            ParkModels();
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            });
            Debug.Log($"[WebGLBuild] {report.summary.result}, {report.summary.totalSize / (1024 * 1024)} MB, {report.summary.totalErrors} errors");
        }
        finally
        {
            buildingViaMenu = false;
            RestoreModels();
        }
    }

    [MenuItem("Tools/Restore 3D models to Resources")]
    static void RestoreModels()
    {
        if (!AssetDatabase.IsValidFolder(ParkingFolder)) return;

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { ParkingFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string error = AssetDatabase.MoveAsset(path, ResourcesFolder + "/" + Path.GetFileName(path));
            if (!string.IsNullOrEmpty(error)) Debug.LogError("[WebGLBuild] Could not restore " + path + ": " + error);
        }
        if (AssetDatabase.FindAssets("", new[] { ParkingFolder }).Length == 0)
            AssetDatabase.DeleteAsset(ParkingFolder);
    }

    static List<string> ModelPrefabsInResources()
    {
        var result = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { ResourcesFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (string dep in AssetDatabase.GetDependencies(path, true))
            {
                if (dep.StartsWith("Assets/Models/") && (dep.EndsWith(".fbx") || dep.EndsWith(".FBX")))
                {
                    result.Add(path);
                    break;
                }
            }
        }
        return result;
    }

    static void ParkModels()
    {
        if (!AssetDatabase.IsValidFolder(ParkingFolder))
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(ParkingFolder));

        foreach (string path in ModelPrefabsInResources())
        {
            string error = AssetDatabase.MoveAsset(path, ParkingFolder + "/" + Path.GetFileName(path));
            if (!string.IsNullOrEmpty(error)) throw new BuildFailedException("[WebGLBuild] Could not move " + path + ": " + error);
        }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform != BuildTarget.WebGL || buildingViaMenu) return;
        if (ModelPrefabsInResources().Count > 0)
            throw new BuildFailedException("WebGL builds must use Tools > Build WebGL (card art); a normal build would include the ~2 GB of 3D models.");
    }
}
