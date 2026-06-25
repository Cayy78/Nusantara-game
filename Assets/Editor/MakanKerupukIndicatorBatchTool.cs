using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MakanKerupukIndicatorBatchTool : EditorWindow
{
    const string SceneRootPath = "Assets/Scenes/Scene Gameplay";

    Color player1Color = new Color32(255, 0, 0, 255);
    float player1HorizontalMargin = 0f;
    float player1VerticalMargin = -0.4f;
    int player1SortingOrderOffset = 100;
    Vector3 player1IndicatorScale = new Vector3(0.75f, 0.75f, 1f);

    Color player2Color = new Color32(0, 26, 255, 255);
    float player2HorizontalMargin = 0f;
    float player2VerticalMargin = -0.4f;
    int player2SortingOrderOffset = 100;
    Vector3 player2IndicatorScale = new Vector3(0.75f, 0.75f, 1f);

    Vector2 scrollPosition;
    readonly List<SceneUpdateResult> results = new List<SceneUpdateResult>();

    [MenuItem("Tools/Nusantara Battle/Update Makan Kerupuk Indicator Settings")]
    static void OpenWindow()
    {
        GetWindow<MakanKerupukIndicatorBatchTool>("Makan Kerupuk Indicators");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Makan Kerupuk Indicator Batch Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Player 1 Settings", EditorStyles.boldLabel);
        player1Color = EditorGUILayout.ColorField("Player 1 Color", player1Color);
        player1HorizontalMargin = EditorGUILayout.FloatField("Player 1 Horizontal Margin", player1HorizontalMargin);
        player1VerticalMargin = EditorGUILayout.FloatField("Player 1 Vertical Margin", player1VerticalMargin);
        player1SortingOrderOffset = EditorGUILayout.IntField("Player 1 Sorting Offset", player1SortingOrderOffset);
        player1IndicatorScale = EditorGUILayout.Vector3Field("Player 1 Indicator Scale", player1IndicatorScale);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Player 2 Settings", EditorStyles.boldLabel);
        player2Color = EditorGUILayout.ColorField("Player 2 Color", player2Color);
        player2HorizontalMargin = EditorGUILayout.FloatField("Player 2 Horizontal Margin", player2HorizontalMargin);
        player2VerticalMargin = EditorGUILayout.FloatField("Player 2 Vertical Margin", player2VerticalMargin);
        player2SortingOrderOffset = EditorGUILayout.IntField("Player 2 Sorting Offset", player2SortingOrderOffset);
        player2IndicatorScale = EditorGUILayout.Vector3Field("Player 2 Indicator Scale", player2IndicatorScale);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Tool ini hanya mengubah setting panah untuk scene Makan Kerupuk, baik single player maupun multiplayer.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan Makan Kerupuk Scenes"))
            ScanScenes();

        if (GUILayout.Button("Apply To Makan Kerupuk Scenes"))
            ApplyToScenes();

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

    void ApplyToScenes()
    {
        results.Clear();
        int changedSceneCount = 0;
        int updatedComponentCount = 0;

        foreach (string scenePath in EnumerateScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneChanged = false;
            int updatedInScene = 0;

            MonoBehaviour[] loaders = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (MonoBehaviour loader in loaders)
            {
                if (loader == null || !HasIndicatorSettings(loader))
                    continue;

                if (ApplySettings(loader))
                {
                    sceneChanged = true;
                    updatedInScene++;
                    updatedComponentCount++;
                }
            }

            if (sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                changedSceneCount++;
            }

            SceneUpdateResult result = CollectSceneResult(scene);
            result.updatedCount = updatedInScene;
            results.Add(result);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog(
            "Makan Kerupuk Indicator Update Complete",
            "Updated " + updatedComponentCount + " loader component(s) across " + changedSceneCount + " Makan Kerupuk scene(s).",
            "OK");
    }

    void DrawResults()
    {
        if (results.Count == 0)
            return;

        EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (SceneUpdateResult result in results)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Scene", result.scenePath);
            EditorGUILayout.LabelField("Loader Count", result.loaderCount.ToString());
            EditorGUILayout.LabelField("Updated", result.updatedCount.ToString());

            if (!string.IsNullOrEmpty(result.note))
                EditorGUILayout.HelpBox(result.note, MessageType.None);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    IEnumerable<string> EnumerateScenePaths()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SceneRootPath });
        foreach (string sceneGuid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            if (!scenePath.Contains("Makan Kerupuk"))
                continue;

            yield return scenePath;
        }
    }

    SceneUpdateResult CollectSceneResult(Scene scene)
    {
        int loaderCount = 0;
        MonoBehaviour[] loaders = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (MonoBehaviour loader in loaders)
        {
            if (loader != null && HasIndicatorSettings(loader))
                loaderCount++;
        }

        return new SceneUpdateResult
        {
            scenePath = scene.path,
            loaderCount = loaderCount,
            note = loaderCount == 0 ? "No supported character loader found in this scene." : string.Empty
        };
    }

    bool HasIndicatorSettings(MonoBehaviour behaviour)
    {
        return GetSettingsField(behaviour.GetType(), "player1IndicatorSettings") != null &&
               GetSettingsField(behaviour.GetType(), "player2IndicatorSettings") != null;
    }

    bool ApplySettings(MonoBehaviour loader)
    {
        FieldInfo player1Field = GetSettingsField(loader.GetType(), "player1IndicatorSettings");
        FieldInfo player2Field = GetSettingsField(loader.GetType(), "player2IndicatorSettings");
        if (player1Field == null || player2Field == null)
            return false;

        PlayerArrowIndicatorSettings player1Settings = player1Field.GetValue(loader) as PlayerArrowIndicatorSettings;
        PlayerArrowIndicatorSettings player2Settings = player2Field.GetValue(loader) as PlayerArrowIndicatorSettings;
        if (player1Settings == null || player2Settings == null)
            return false;

        bool changed = false;
        Undo.RecordObject(loader, "Update Makan Kerupuk Indicator Settings");

        changed |= ApplySettings(
            player1Settings,
            player1Color,
            player1HorizontalMargin,
            player1VerticalMargin,
            player1SortingOrderOffset,
            player1IndicatorScale);

        changed |= ApplySettings(
            player2Settings,
            player2Color,
            player2HorizontalMargin,
            player2VerticalMargin,
            player2SortingOrderOffset,
            player2IndicatorScale);

        if (changed)
            EditorUtility.SetDirty(loader);

        return changed;
    }

    bool ApplySettings(
        PlayerArrowIndicatorSettings settings,
        Color color,
        float horizontalMargin,
        float verticalMargin,
        int sortingOrderOffset,
        Vector3 indicatorScale)
    {
        bool changed = false;

        if (settings.indicatorColor != color)
        {
            settings.indicatorColor = color;
            changed = true;
        }

        if (!Mathf.Approximately(settings.horizontalMargin, horizontalMargin))
        {
            settings.horizontalMargin = horizontalMargin;
            changed = true;
        }

        if (!Mathf.Approximately(settings.verticalMargin, verticalMargin))
        {
            settings.verticalMargin = verticalMargin;
            changed = true;
        }

        if (settings.sortingOrderOffset != sortingOrderOffset)
        {
            settings.sortingOrderOffset = sortingOrderOffset;
            changed = true;
        }

        if (settings.indicatorScale != indicatorScale)
        {
            settings.indicatorScale = indicatorScale;
            changed = true;
        }

        return changed;
    }

    FieldInfo GetSettingsField(System.Type targetType, string fieldName)
    {
        return targetType.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    class SceneUpdateResult
    {
        public string scenePath;
        public int loaderCount;
        public int updatedCount;
        public string note;
    }
}
