using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimerTextOutlineBatchTool : EditorWindow
{
    const string DefaultSceneRoot = "Assets/Scenes";
    const string GeneratedMaterialFolder = "Assets/Generated/TimerTextMaterials";

    string sceneRootPath = DefaultSceneRoot;
    string targetObjectName = "TimerText";
    float outlineWidth = 0.2f;
    Vector2 scrollPosition;

    readonly List<OutlineSceneResult> results = new List<OutlineSceneResult>();

    [MenuItem("Tools/Nusantara Battle/Update TimerText Outline Width")]
    static void OpenWindow()
    {
        GetWindow<TimerTextOutlineBatchTool>("TimerText Outline");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("TimerText Outline Batch Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        sceneRootPath = EditorGUILayout.TextField("Scene Root Path", sceneRootPath);
        targetObjectName = EditorGUILayout.TextField("Target Object Name", targetObjectName);
        outlineWidth = EditorGUILayout.Slider("Outline Width", outlineWidth, 0f, 1f);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Tool ini akan mencari semua object TMP bernama TimerText di dalam scene, " +
            "membuat material khusus untuk TimerText bila perlu, lalu mengubah ketebalan outline-nya saja.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan Scenes"))
            ScanScenes();

        if (GUILayout.Button("Apply Outline Width To All TimerText"))
            ApplyOutlineWidth();

        EditorGUILayout.Space();
        DrawResults();
    }

    void ScanScenes()
    {
        results.Clear();

        foreach (string scenePath in EnumerateScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            results.Add(CollectSceneResult(scene));
        }
    }

    void ApplyOutlineWidth()
    {
        results.Clear();
        EnsureGeneratedFolderExists();

        int changedSceneCount = 0;
        int updatedTextCount = 0;

        foreach (string scenePath in EnumerateScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneChanged = false;

            TMP_Text[] timerTexts = FindTimerTexts();
            int updatedInScene = 0;

            foreach (TMP_Text timerText in timerTexts)
            {
                if (timerText == null)
                    continue;

                if (ApplyOutlineWidthToText(scene, timerText))
                {
                    updatedInScene++;
                    updatedTextCount++;
                    sceneChanged = true;
                }
            }

            if (sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                changedSceneCount++;
            }

            OutlineSceneResult result = CollectSceneResult(scene);
            result.updatedCount = updatedInScene;
            results.Add(result);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = "Updated " + updatedTextCount + " TimerText object(s) across " +
                         changedSceneCount + " scene(s).";
        EditorUtility.DisplayDialog("TimerText Outline Update Complete", summary, "OK");
    }

    void DrawResults()
    {
        if (results.Count == 0)
            return;

        EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (OutlineSceneResult result in results)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Scene", result.scenePath);
            EditorGUILayout.LabelField("TimerText Found", result.timerCount.ToString());
            EditorGUILayout.LabelField("Updated", result.updatedCount.ToString());

            if (!string.IsNullOrEmpty(result.note))
                EditorGUILayout.HelpBox(result.note, MessageType.None);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    IEnumerable<string> EnumerateScenePaths()
    {
        if (string.IsNullOrEmpty(sceneRootPath) || !AssetDatabase.IsValidFolder(sceneRootPath))
            yield break;

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { sceneRootPath });
        foreach (string sceneGuid in sceneGuids)
            yield return AssetDatabase.GUIDToAssetPath(sceneGuid);
    }

    OutlineSceneResult CollectSceneResult(Scene scene)
    {
        TMP_Text[] timerTexts = FindTimerTexts();

        return new OutlineSceneResult
        {
            scenePath = scene.path,
            timerCount = timerTexts.Length,
            note = timerTexts.Length == 0 ? "No TimerText object found in this scene." : string.Empty
        };
    }

    TMP_Text[] FindTimerTexts()
    {
        List<TMP_Text> matches = new List<TMP_Text>();
        TMP_Text[] allTexts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TMP_Text text in allTexts)
        {
            if (text != null && text.gameObject.name == targetObjectName)
                matches.Add(text);
        }

        return matches.ToArray();
    }

    bool ApplyOutlineWidthToText(Scene scene, TMP_Text timerText)
    {
        Material sourceMaterial = GetSourceMaterial(timerText);
        if (sourceMaterial == null)
            return false;

        Material timerMaterial = GetOrCreateTimerMaterial(scene, timerText, sourceMaterial);
        if (timerMaterial == null || !timerMaterial.HasProperty(ShaderUtilities.ID_OutlineWidth))
            return false;

        bool changed = false;

        if (timerText.fontSharedMaterial != timerMaterial)
        {
            Undo.RecordObject(timerText, "Assign TimerText Material");
            timerText.fontSharedMaterial = timerMaterial;
            EditorUtility.SetDirty(timerText);
            changed = true;
        }

        float currentOutlineWidth = timerMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth);
        if (!Mathf.Approximately(currentOutlineWidth, outlineWidth))
        {
            Undo.RecordObject(timerMaterial, "Update TimerText Outline Width");
            timerMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
            EditorUtility.SetDirty(timerMaterial);
            changed = true;
        }

        return changed;
    }

    Material GetSourceMaterial(TMP_Text timerText)
    {
        if (timerText == null)
            return null;

        if (timerText.fontSharedMaterial != null)
            return timerText.fontSharedMaterial;

        if (timerText.fontMaterial != null)
            return timerText.fontMaterial;

        if (timerText.font != null)
            return timerText.font.material;

        return null;
    }

    Material GetOrCreateTimerMaterial(Scene scene, TMP_Text timerText, Material sourceMaterial)
    {
        string materialPath = BuildMaterialPath(scene, timerText);
        Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (existingMaterial != null)
            return existingMaterial;

        Material newMaterial = new Material(sourceMaterial);
        AssetDatabase.CreateAsset(newMaterial, materialPath);
        AssetDatabase.ImportAsset(materialPath);
        return AssetDatabase.LoadAssetAtPath<Material>(materialPath);
    }

    string BuildMaterialPath(Scene scene, TMP_Text timerText)
    {
        string sceneName = SanitizeName(Path.GetFileNameWithoutExtension(scene.path));
        string hierarchyName = SanitizeName(GetHierarchyPath(timerText.transform));
        string materialFileName = sceneName + "_" + hierarchyName + "_TimerText.mat";
        return GeneratedMaterialFolder + "/" + materialFileName;
    }

    void EnsureGeneratedFolderExists()
    {
        if (AssetDatabase.IsValidFolder("Assets/Generated"))
        {
            if (!AssetDatabase.IsValidFolder(GeneratedMaterialFolder))
                AssetDatabase.CreateFolder("Assets/Generated", "TimerTextMaterials");

            return;
        }

        AssetDatabase.CreateFolder("Assets", "Generated");
        AssetDatabase.CreateFolder("Assets/Generated", "TimerTextMaterials");
    }

    string GetHierarchyPath(Transform target)
    {
        if (target == null)
            return string.Empty;

        List<string> parts = new List<string>();
        Transform current = target;

        while (current != null)
        {
            parts.Add(current.name);
            current = current.parent;
        }

        parts.Reverse();
        return string.Join("_", parts);
    }

    string SanitizeName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "Unnamed";

        StringBuilder builder = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
                builder.Append(character);
            else
                builder.Append('_');
        }

        return builder.ToString();
    }

    class OutlineSceneResult
    {
        public string scenePath;
        public int timerCount;
        public int updatedCount;
        public string note;
    }
}
