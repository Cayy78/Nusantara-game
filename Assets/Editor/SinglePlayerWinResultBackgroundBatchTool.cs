using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SinglePlayerWinResultBackgroundBatchTool : EditorWindow
{
    const string SceneRootPath = "Assets/Scenes/Scene Gameplay/SinglePlayer";
    const string ResultPanelObjectName = "WinResultPanel";
    const string ResultBackgroundObjectName = "result background";

    Image templateBackgroundImage;
    Sprite templateSprite;
    Color templateColor = Color.white;
    Image.Type templateImageType = Image.Type.Simple;
    bool templatePreserveAspect;
    Material templateMaterial;
    bool overrideExisting = true;
    bool copyColor = true;
    bool copyImageType = true;
    bool copyPreserveAspect = true;
    bool copyMaterial = true;
    Vector2 scrollPosition;

    readonly List<WinResultBackgroundScanResult> scanResults = new List<WinResultBackgroundScanResult>();

    [MenuItem("Tools/Nusantara Battle/Update Single Player Win Result Backgrounds")]
    static void OpenWindow()
    {
        GetWindow<SinglePlayerWinResultBackgroundBatchTool>("Win Result BG Tool");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Single Player Win Result Background Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        Image previousTemplate = templateBackgroundImage;
        templateBackgroundImage = (Image)EditorGUILayout.ObjectField(
            "Template Background Image",
            templateBackgroundImage,
            typeof(Image),
            true);

        if (templateBackgroundImage != null && templateBackgroundImage != previousTemplate)
            CaptureTemplateSettings(templateBackgroundImage);

        overrideExisting = EditorGUILayout.Toggle("Override Existing", overrideExisting);
        copyColor = EditorGUILayout.Toggle("Copy Color", copyColor);
        copyImageType = EditorGUILayout.Toggle("Copy Image Type", copyImageType);
        copyPreserveAspect = EditorGUILayout.Toggle("Copy Preserve Aspect", copyPreserveAspect);
        copyMaterial = EditorGUILayout.Toggle("Copy Material", copyMaterial);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Drag object 'result background' dari WinResultPanel scene template kamu, lalu tool ini akan " +
            "copy sprite dan setting Image ke semua scene single player.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan Single Player Scenes"))
            ScanScenes();

        using (new EditorGUI.DisabledScope(templateSprite == null))
        {
            if (GUILayout.Button("Apply To All Single Player Scenes"))
                ApplyToScenes();
        }

        EditorGUILayout.Space();
        DrawResults();
    }

    void CaptureTemplateSettings(Image templateImage)
    {
        if (templateImage == null)
            return;

        templateSprite = templateImage.sprite;
        templateColor = templateImage.color;
        templateImageType = templateImage.type;
        templatePreserveAspect = templateImage.preserveAspect;
        templateMaterial = templateImage.material;
    }

    void DrawResults()
    {
        if (scanResults.Count == 0)
            return;

        EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (WinResultBackgroundScanResult result in scanResults)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Scene", result.scenePath);
            EditorGUILayout.LabelField("Panel Found", result.panelFound ? "Yes" : "No");
            EditorGUILayout.LabelField("Background Found", result.backgroundFound ? "Yes" : "No");
            EditorGUILayout.LabelField("Background Path", string.IsNullOrEmpty(result.backgroundPath) ? "(none)" : result.backgroundPath);
            EditorGUILayout.LabelField("Sprite", string.IsNullOrEmpty(result.spriteName) ? "(none)" : result.spriteName);

            if (!string.IsNullOrEmpty(result.note))
                EditorGUILayout.HelpBox(result.note, MessageType.None);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    void ScanScenes()
    {
        scanResults.Clear();

        foreach (string scenePath in EnumerateSinglePlayerScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            scanResults.Add(CollectSceneResult(scene));
        }
    }

    void ApplyToScenes()
    {
        scanResults.Clear();

        int updatedSceneCount = 0;
        int updatedBackgroundCount = 0;

        foreach (string scenePath in EnumerateSinglePlayerScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneChanged = false;

            Image targetBackground = FindWinResultBackground();
            if (targetBackground != null)
            {
                if (ApplyTemplateToTarget(targetBackground))
                {
                    sceneChanged = true;
                    updatedBackgroundCount++;
                }
            }

            if (sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                updatedSceneCount++;
            }

            scanResults.Add(CollectSceneResult(scene));
        }

        AssetDatabase.SaveAssets();

        StringBuilder summary = new StringBuilder();
        summary.Append("Updated ");
        summary.Append(updatedBackgroundCount);
        summary.Append(" win result background object(s) across ");
        summary.Append(updatedSceneCount);
        summary.Append(" single player scene(s).");

        EditorUtility.DisplayDialog("Win Result Background Update Complete", summary.ToString(), "OK");
    }

    IEnumerable<string> EnumerateSinglePlayerScenePaths()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SceneRootPath });

        foreach (string sceneGuid in sceneGuids)
            yield return AssetDatabase.GUIDToAssetPath(sceneGuid);
    }

    WinResultBackgroundScanResult CollectSceneResult(Scene scene)
    {
        GameObject panel = FindObjectByName(ResultPanelObjectName);
        Image background = FindWinResultBackground();

        return new WinResultBackgroundScanResult
        {
            scenePath = scene.path,
            panelFound = panel != null,
            backgroundFound = background != null,
            backgroundPath = background != null ? GetHierarchyPath(background.transform) : string.Empty,
            spriteName = background != null && background.sprite != null ? background.sprite.name : string.Empty,
            note = panel == null
                ? "WinResultPanel not found."
                : background == null
                    ? "result background child with Image component not found."
                    : string.Empty
        };
    }

    Image FindWinResultBackground()
    {
        GameObject panel = FindObjectByName(ResultPanelObjectName);
        if (panel == null)
            return null;

        Transform backgroundTransform = FindDescendantByName(panel.transform, ResultBackgroundObjectName);
        if (backgroundTransform == null)
            return null;

        return backgroundTransform.GetComponent<Image>();
    }

    static GameObject FindObjectByName(string objectName)
    {
        GameObject[] gameObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject != null && gameObject.name == objectName)
                return gameObject;
        }

        return null;
    }

    static Transform FindDescendantByName(Transform root, string objectName)
    {
        if (root == null)
            return null;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == objectName)
                return child;

            Transform nested = FindDescendantByName(child, objectName);
            if (nested != null)
                return nested;
        }

        return null;
    }

    bool ApplyTemplateToTarget(Image targetImage)
    {
        if (templateSprite == null || targetImage == null)
            return false;

        bool changed = false;

        if (overrideExisting || targetImage.sprite == null)
        {
            if (targetImage.sprite != templateSprite)
            {
                Undo.RecordObject(targetImage, "Update Win Result Background Sprite");
                targetImage.sprite = templateSprite;
                changed = true;
            }
        }

        if (copyColor && targetImage.color != templateColor)
        {
            if (!changed)
                Undo.RecordObject(targetImage, "Update Win Result Background Color");

            targetImage.color = templateColor;
            changed = true;
        }

        if (copyImageType && targetImage.type != templateImageType)
        {
            if (!changed)
                Undo.RecordObject(targetImage, "Update Win Result Background Type");

            targetImage.type = templateImageType;
            changed = true;
        }

        if (copyPreserveAspect && targetImage.preserveAspect != templatePreserveAspect)
        {
            if (!changed)
                Undo.RecordObject(targetImage, "Update Win Result Background Preserve Aspect");

            targetImage.preserveAspect = templatePreserveAspect;
            changed = true;
        }

        if (copyMaterial && targetImage.material != templateMaterial)
        {
            if (!changed)
                Undo.RecordObject(targetImage, "Update Win Result Background Material");

            targetImage.material = templateMaterial;
            changed = true;
        }

        if (changed)
            EditorUtility.SetDirty(targetImage);

        return changed;
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

    class WinResultBackgroundScanResult
    {
        public string scenePath;
        public bool panelFound;
        public bool backgroundFound;
        public string backgroundPath;
        public string spriteName;
        public string note;
    }
}
