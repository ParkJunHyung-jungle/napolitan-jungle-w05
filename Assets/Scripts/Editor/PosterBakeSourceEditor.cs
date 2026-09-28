using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PosterBakeSource))]
public class PosterBakeSourceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var src = (PosterBakeSource)target;
        EditorGUILayout.Space();

        string error = PosterBaker.GetError(src);
        if (error != null)
            EditorGUILayout.HelpBox(error, MessageType.Error);

        foreach (string warning in PosterBaker.GetWarnings(src))
            EditorGUILayout.HelpBox(warning, MessageType.Warning);

        using (new EditorGUI.DisabledScope(error != null))
        {
            if (GUILayout.Button("Bake Poster", GUILayout.Height(30)))
                PosterBaker.Bake(src);
        }
    }
}

public static class PosterBaker
{
    [MenuItem("Tools/Poster/Bake All In Open Scenes")]
    static void BakeAllInOpenScenes()
    {
        var sources = Object.FindObjectsByType<PosterBakeSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int baked = 0;
        foreach (var src in sources)
        {
            if (Bake(src))
                baked++;
        }
        Debug.Log($"[PosterBaker] {baked}/{sources.Length} posters baked.");
    }

    public static string GetError(PosterBakeSource src)
    {
        if (src.bakeCamera == null)
            return "Bake Camera가 비어 있습니다.";
        if (src.bakeCamera.targetTexture == null)
            return "Bake Camera에 Target Texture(RenderTexture)가 연결되어 있지 않습니다.";
        if (string.IsNullOrEmpty(src.outputPath)
            || !src.outputPath.StartsWith("Assets/")
            || !src.outputPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            return "Output Path는 Assets/ 로 시작하고 .png 로 끝나야 합니다.";
        return null;
    }

    public static IEnumerable<string> GetWarnings(PosterBakeSource src)
    {
        var cam = src.bakeCamera;
        if (cam == null)
            yield break;

        var canvas = src.GetComponent<Canvas>();
        if (canvas.renderMode != RenderMode.ScreenSpaceCamera || canvas.worldCamera != cam)
            yield return "Canvas Render Mode를 Screen Space - Camera로, Render Camera를 Bake Camera로 설정하세요.";

        if ((cam.cullingMask & (1 << src.gameObject.layer)) == 0)
            yield return "Bake Camera의 Culling Mask에 Canvas 레이어가 없습니다. 결과가 비어 보입니다.";

        if (!cam.orthographic)
            yield return "Bake Camera가 Orthographic이 아닙니다.";

        var rt = cam.targetTexture;
        if (rt == null)
            yield break;

        if (QualitySettings.activeColorSpace == ColorSpace.Linear && !rt.sRGB)
            yield return "RenderTexture가 sRGB 포맷이 아닙니다. 색이 뿌옇게 저장될 수 있습니다.";

        if (rt.width % 4 != 0 || rt.height % 4 != 0)
            yield return "RenderTexture 해상도가 4의 배수가 아니라 BC7 압축이 적용되지 않습니다.";
    }

    public static bool Bake(PosterBakeSource src)
    {
        string error = GetError(src);
        if (error != null)
        {
            Debug.LogError($"[PosterBaker] {src.name}: {error}", src);
            return false;
        }

        var cam = src.bakeCamera;
        var rt = cam.targetTexture;

        // 1. 레이아웃과 TMP 메시를 최신 상태로 (TMP 크기가 레이아웃에 영향을 주므로 앞뒤로 갱신)
        Canvas.ForceUpdateCanvases();
        foreach (var text in src.GetComponentsInChildren<TMP_Text>())
            text.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();

        // 2. 카메라가 비활성이어도 수동으로 한 번 렌더링
        cam.Render();

        // 3. RenderTexture → Texture2D (sRGB)
        var prevActive = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);
        RenderTexture.active = prevActive;

        if (src.forceOpaque)
        {
            var pixels = tex.GetPixels32();
            for (int i = 0; i < pixels.Length; i++)
                pixels[i].a = 255;
            tex.SetPixels32(pixels);
        }
        tex.Apply(false);

        // 4. PNG 저장 및 임포트
        string fullPath = Path.GetFullPath(src.outputPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(src.outputPath, ImportAssetOptions.ForceUpdate);
        if (src.applyImportSettings)
            ApplyImportSettings(src, rt.width, rt.height);

        // 5. 머티리얼 연결 (mainTexture는 URP Lit의 _BaseMap을 가리킴)
        if (src.targetMaterial != null)
        {
            var baked = AssetDatabase.LoadAssetAtPath<Texture2D>(src.outputPath);
            Undo.RecordObject(src.targetMaterial, "Assign Baked Poster");
            src.targetMaterial.mainTexture = baked;
            EditorUtility.SetDirty(src.targetMaterial);
        }

        Debug.Log($"[PosterBaker] Baked {src.name} → {src.outputPath} ({rt.width}x{rt.height})", src);
        return true;
    }

    static void ApplyImportSettings(PosterBakeSource src, int width, int height)
    {
        var importer = (TextureImporter)AssetImporter.GetAtPath(src.outputPath);
        int maxSize = Mathf.NextPowerOfTwo(Mathf.Max(width, height));

        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = src.forceOpaque ? TextureImporterAlphaSource.None : TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = !src.forceOpaque;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Trilinear;
        importer.anisoLevel = 16;
        importer.mipmapEnabled = true;
        importer.mipmapFilter = TextureImporterMipFilter.KaiserFilter;
        importer.mipMapBias = src.mipMapBias;
        importer.maxTextureSize = maxSize;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;

        var standalone = importer.GetPlatformTextureSettings("Standalone");
        standalone.overridden = true;
        standalone.maxTextureSize = maxSize;
        standalone.format = TextureImporterFormat.BC7;
        standalone.compressionQuality = 100;
        importer.SetPlatformTextureSettings(standalone);

        importer.SaveAndReimport();
    }
}
