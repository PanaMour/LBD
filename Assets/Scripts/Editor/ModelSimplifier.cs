using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;

// The monster models are ~1.5-2M triangles each (~190 MB of memory apiece)
// and every one is loaded at startup. This builds lightweight copies next to
// them and can point the Resources prefabs at either version, so the swap is
// reversible and the original FBX files are never modified.
[InitializeOnLoad]
public static class ModelSimplifier
{
    const string QueueKey = "ModelSimplifier.Queue";
    const string TotalKey = "ModelSimplifier.Total";

    // A script reload wipes static state, so the queue lives in SessionState
    // and processing resumes after any reload.
    static ModelSimplifier()
    {
        string saved = SessionState.GetString(QueueKey, "");
        if (saved.Length == 0) return;
        foreach (string n in saved.Split(',')) if (n.Length > 0) pending.Enqueue(n);
        total = SessionState.GetInt(TotalKey, pending.Count);
        EditorApplication.update += ProcessNext;
    }

    static void SaveQueue()
    {
        SessionState.SetString(QueueKey, string.Join(",", pending.ToArray()));
        SessionState.SetInt(TotalKey, total);
    }

    const string ResourcesFolder = "Assets/Resources";
    const string OutputFolder = "Assets/Models/Simplified";
    const string PreviewFolder = "Temp/ModelPreviews";
    const int TargetTriangles = 30000;
    static readonly string[] SampleModels = { "legendary_ninja", "sharpchine", "quadropticus" };

    static readonly Queue<string> pending = new Queue<string>();
    static int total;

    [MenuItem("Tools/Simplify Models/1. Generate samples (3 models)")]
    static void GenerateSamples() => Enqueue(SampleModels);

    [MenuItem("Tools/Simplify Models/2. Generate all")]
    static void GenerateAll()
    {
        var names = new List<string>();
        foreach (string path in ModelPrefabs())
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (!File.Exists($"{OutputFolder}/{name}.asset")) names.Add(name);
        }
        Enqueue(names);
    }

    [MenuItem("Tools/Simplify Models/3. Use simplified meshes in prefabs")]
    static void ApplyAll() => SwapPrefabMeshes(true);

    [MenuItem("Tools/Simplify Models/4. Revert prefabs to original meshes")]
    static void RevertAll() => SwapPrefabMeshes(false);

    static void Enqueue(IEnumerable<string> names)
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder)) AssetDatabase.CreateFolder("Assets/Models", "Simplified");
        foreach (string n in names) if (!pending.Contains(n)) pending.Enqueue(n);
        total = pending.Count;
        SaveQueue();
        EditorApplication.update -= ProcessNext;
        EditorApplication.update += ProcessNext;
        Debug.Log($"[ModelSimplifier] Queued {total} model(s).");
    }

    // One model per editor tick keeps the editor (and the MCP bridge) responsive.
    static void ProcessNext()
    {
        if (pending.Count == 0)
        {
            EditorApplication.update -= ProcessNext;
            SessionState.EraseString(QueueKey);
            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            Debug.Log("[ModelSimplifier] Done.");
            return;
        }

        string name = pending.Peek();
        int index = total - pending.Count + 1;
        EditorUtility.DisplayProgressBar("Simplifying monster models", $"{name} ({index}/{total}) - Unity may look frozen per model", (float)(index - 1) / total);
        try
        {
            SimplifyOne(name, index, total);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ModelSimplifier] {name} failed: {e.Message}");
        }
        pending.Dequeue();
        SaveQueue();
        if (pending.Count == 0) EditorUtility.ClearProgressBar();
        EditorUtility.UnloadUnusedAssetsImmediate();
    }

    static void SimplifyOne(string name, int index, int count)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ResourcesFolder}/{name}.prefab");
        MeshFilter mf = prefab != null ? prefab.GetComponentInChildren<MeshFilter>(true) : null;
        Mesh source = OriginalMesh(mf);
        if (source == null)
        {
            Debug.LogWarning($"[ModelSimplifier] {name}: no original mesh found, skipped.");
            return;
        }

        int sourceTris = (int)(source.GetIndexCount(0) / 3);
        float quality = Mathf.Clamp01((float)TargetTriangles / Mathf.Max(1, sourceTris));

        var simplifier = new MeshSimplifier();
        var options = SimplificationOptions.Default;
        // These generated meshes have heavily fragmented UVs; preserving every
        // seam stops the reduction at ~25%, so seams are allowed to collapse.
        options.PreserveUVSeamEdges = false;
        options.PreserveUVFoldoverEdges = false;
        options.EnableSmartLink = true;
        options.MaxIterationCount = 1000;
        simplifier.SimplificationOptions = options;
        simplifier.Initialize(source);
        simplifier.SimplifyMesh(quality);

        Mesh result = simplifier.ToMesh();
        result.name = name + "_simplified";
        result.RecalculateBounds();

        string outPath = $"{OutputFolder}/{name}.asset";
        AssetDatabase.DeleteAsset(outPath);
        AssetDatabase.CreateAsset(result, outPath);

        int resultTris = (int)(result.GetIndexCount(0) / 3);
        Debug.Log($"[ModelSimplifier] ({index}/{count}) {name}: {sourceTris:N0} -> {resultTris:N0} triangles");
    }

    static Mesh OriginalMesh(MeshFilter mf)
    {
        if (mf == null || mf.sharedMesh == null) return null;
        string path = AssetDatabase.GetAssetPath(mf.sharedMesh);
        if (path.StartsWith("Assets/Models/") && !path.StartsWith(OutputFolder)) return mf.sharedMesh;

        // Prefab already points at the simplified copy: find the FBX mesh again.
        string fbx = $"Assets/Models/{Path.GetFileNameWithoutExtension(path)}.fbx";
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(fbx))
            if (o is Mesh m) return m;
        return null;
    }

    static List<string> ModelPrefabs()
    {
        var result = new List<string>();
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { ResourcesFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>(true);
            if (mf != null && mf.sharedMesh != null && AssetDatabase.GetAssetPath(mf.sharedMesh).StartsWith("Assets/Models/"))
                result.Add(path);
        }
        return result;
    }

    static void SwapPrefabMeshes(bool useSimplified)
    {
        int swapped = 0, missing = 0;
        foreach (string path in ModelPrefabs())
        {
            string name = Path.GetFileNameWithoutExtension(path);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            MeshFilter mf = root.GetComponentInChildren<MeshFilter>(true);

            Mesh target = useSimplified
                ? AssetDatabase.LoadAssetAtPath<Mesh>($"{OutputFolder}/{name}.asset")
                : OriginalMesh(mf);

            if (target == null) missing++;
            else if (mf.sharedMesh != target)
            {
                mf.sharedMesh = target;
                PrefabUtility.SaveAsPrefabAsset(root, path);
                swapped++;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"[ModelSimplifier] {(useSimplified ? "Simplified" : "Original")} meshes: {swapped} prefab(s) changed, {missing} without a {(useSimplified ? "simplified" : "original")} mesh.");
    }

    // Side-by-side renders (original left, simplified right) for comparison.
    [MenuItem("Tools/Simplify Models/Render sample previews")]
    static void RenderPreviews()
    {
        Directory.CreateDirectory(PreviewFolder);
        foreach (string name in SampleModels)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ResourcesFolder}/{name}.prefab");
            MeshFilter mf = prefab != null ? prefab.GetComponentInChildren<MeshFilter>(true) : null;
            Mesh original = OriginalMesh(mf);
            Mesh simple = AssetDatabase.LoadAssetAtPath<Mesh>($"{OutputFolder}/{name}.asset");
            if (original == null || simple == null) continue;

            Material mat = mf.GetComponent<Renderer>().sharedMaterial;
            byte[] png = RenderPair(original, simple, mat);
            File.WriteAllBytes($"{PreviewFolder}/{name}.png", png);
        }
        Debug.Log($"[ModelSimplifier] Previews written to {Path.GetFullPath(PreviewFolder)}");
    }

    static byte[] RenderPair(Mesh left, Mesh right, Material mat)
    {
        const int size = 512;
        Texture2D a = RenderSingle(left, mat, size);
        Texture2D b = RenderSingle(right, mat, size);
        int w = Mathf.Min(a.width, b.width), h = Mathf.Min(a.height, b.height);
        var combined = new Texture2D(w * 2, h, TextureFormat.RGBA32, false);
        combined.SetPixels(0, 0, w, h, a.GetPixels(0, 0, w, h));
        combined.SetPixels(w, 0, w, h, b.GetPixels(0, 0, w, h));
        combined.Apply();
        return combined.EncodeToPNG();
    }

    static Texture2D RenderSingle(Mesh mesh, Material mat, int size)
    {
        var utility = new PreviewRenderUtility();
        try
        {
            utility.camera.backgroundColor = new Color(0.18f, 0.18f, 0.2f);
            utility.camera.clearFlags = CameraClearFlags.SolidColor;
            utility.lights[0].intensity = 1.2f;
            utility.lights[0].transform.rotation = Quaternion.Euler(30, 30, 0);
            utility.ambientColor = new Color(0.5f, 0.5f, 0.5f);

            Quaternion rot = Quaternion.Euler(-90, 180, 0);
            Vector3 center = rot * mesh.bounds.center;
            float radius = mesh.bounds.extents.magnitude;
            float fov = utility.camera.fieldOfView;
            float distance = radius / Mathf.Sin(fov * 0.5f * Mathf.Deg2Rad) * 1.05f;

            utility.camera.transform.position = new Vector3(0, radius * 0.3f, -distance);
            utility.camera.transform.LookAt(Vector3.zero);
            utility.camera.nearClipPlane = distance * 0.01f;
            utility.camera.farClipPlane = distance * 4f;

            // Copy the result out before Cleanup() releases the render texture.
            utility.BeginStaticPreview(new Rect(0, 0, size, size));
            utility.DrawMesh(mesh, -center, rot, mat, 0);
            utility.camera.Render();
            Texture2D rendered = utility.EndStaticPreview();
            var copy = new Texture2D(rendered.width, rendered.height, TextureFormat.RGBA32, false);
            copy.SetPixels(rendered.GetPixels());
            copy.Apply();
            return copy;
        }
        finally
        {
            utility.Cleanup();
        }
    }
}
