using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayPauseTitleBatchTool : EditorWindow
{
    string targetObjectName = "VolumeGame";
    string replacementText = "Pause";
    float replacementFontSize = 200f;
    Vector2 scrollPosition;
    readonly List<PauseTitleScanResult> scanResults = new List<PauseTitleScanResult>();

    [MenuItem("Tools/Nusantara Battle/Update Gameplay Pause Titles")]
    static void OpenWindow()
    {
        GetWindow<GameplayPauseTitleBatchTool>("Pause Title Tool");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Gameplay Pause Title Batch Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetObjectName = EditorGUILayout.TextField("Target Object Name", targetObjectName);
        replacementText = EditorGUILayout.TextField("Replacement Text", replacementText);
        replacementFontSize = EditorGUILayout.FloatField("Replacement Font Size", replacementFontSize);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Tool ini akan scan semua scene di Assets/Scenes/Scene Gameplay, cari object UI bernama target, " +
            "lalu mengubah text TMP dan font size-nya.\n\n" +
            "Catatan: tool ini tidak mengubah nama object GameObject, hanya isi text dan ukuran font.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan All Gameplay Pause Titles"))
            ScanAllGameplayPauseTitles();

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(targetObjectName)))
        {
            if (GUILayout.Button("Apply To All Gameplay Scenes"))
                ApplyToAllGameplayScenes();
        }

        EditorGUILayout.Space();
        DrawResults();
    }

    void DrawResults()
    {
        if (scanResults.Count == 0)
            return;

        EditorGUILayout.LabelField("Scan Results", EditorStyles.boldLabel);
        EditorGUILayout.Space(4f);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < scanResults.Count; i++)
        {
            PauseTitleScanResult result = scanResults[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Scene", result.scenePath);
            EditorGUILayout.LabelField("Object", result.objectPath);
            EditorGUILayout.LabelField("Current Text", result.currentText);
            EditorGUILayout.LabelField("Current Font Size", result.currentFontSize.ToString());
            EditorGUILayout.LabelField("TMP Component", result.hasTmpText ? "OK" : "Missing");

            if (!string.IsNullOrEmpty(result.note))
                EditorGUILayout.HelpBox(result.note, MessageType.None);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    void ScanAllGameplayPauseTitles()
    {
        scanResults.Clear();

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Scene Gameplay" });

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int j = 0; j < texts.Length; j++)
            {
                TMP_Text textComponent = texts[j];
                if (textComponent == null || textComponent.gameObject.name != targetObjectName)
                    continue;

                scanResults.Add(CreateScanResult(scene, textComponent));
            }
        }
    }

    void ApplyToAllGameplayScenes()
    {
        scanResults.Clear();

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Scene Gameplay" });
        int updatedSceneCount = 0;
        int updatedObjectCount = 0;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool sceneChanged = false;

            for (int j = 0; j < texts.Length; j++)
            {
                TMP_Text textComponent = texts[j];
                if (textComponent == null || textComponent.gameObject.name != targetObjectName)
                    continue;

                bool changed = false;

                if (textComponent.text != replacementText)
                {
                    Undo.RecordObject(textComponent, "Update Gameplay Pause Title Text");
                    textComponent.text = replacementText;
                    changed = true;
                }

                if (!Mathf.Approximately(textComponent.fontSize, replacementFontSize))
                {
                    if (!changed)
                        Undo.RecordObject(textComponent, "Update Gameplay Pause Title Font Size");

                    textComponent.fontSize = replacementFontSize;
                    changed = true;
                }

                if (changed)
                {
                    EditorUtility.SetDirty(textComponent);
                    sceneChanged = true;
                    updatedObjectCount++;
                }

                scanResults.Add(CreateScanResult(scene, textComponent));
            }

            if (!sceneChanged)
                continue;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            updatedSceneCount++;
        }

        AssetDatabase.SaveAssets();

        StringBuilder message = new StringBuilder();
        message.Append("Updated ");
        message.Append(updatedObjectCount);
        message.Append(" pause title object(s) across ");
        message.Append(updatedSceneCount);
        message.Append(" gameplay scene(s).");

        EditorUtility.DisplayDialog("Gameplay Pause Title Update Complete", message.ToString(), "OK");
    }

    static PauseTitleScanResult CreateScanResult(Scene scene, TMP_Text textComponent)
    {
        return new PauseTitleScanResult
        {
            scenePath = scene.path,
            objectPath = GetHierarchyPath(textComponent.transform),
            hasTmpText = textComponent != null,
            currentText = textComponent != null ? textComponent.text : "(missing)",
            currentFontSize = textComponent != null ? textComponent.fontSize : 0f
        };
    }

    static string GetHierarchyPath(Transform target)
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
        return string.Join("/", parts);
    }

    class PauseTitleScanResult
    {
        public string scenePath;
        public string objectPath;
        public bool hasTmpText;
        public string currentText;
        public float currentFontSize;
        public string note;
    }
}
