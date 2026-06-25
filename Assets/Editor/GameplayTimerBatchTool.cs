using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameplayTimerBatchTool : EditorWindow
{
    const string SceneRootPath = "Assets/Scenes/Scene Gameplay";
    string timerObjectName = "TimerText";
    string initialTimerText = "00.00";
    float fontSize = 72f;
    Vector2 anchorMin = new Vector2(0.5f, 1f);
    Vector2 anchorMax = new Vector2(0.5f, 1f);
    Vector2 pivot = new Vector2(0.5f, 1f);
    Vector2 anchoredPosition = new Vector2(0f, -80f);
    Vector2 sizeDelta = new Vector2(260f, 90f);
    Vector3 localScale = Vector3.one;
    Vector3 rotation = Vector3.zero;
    TMP_FontAsset fontAsset;
    Material fontSharedMaterial;
    FontStyles fontStyle = FontStyles.Normal;
    TextAlignmentOptions alignment = TextAlignmentOptions.Center;
    Color fontColor = Color.white;
    bool overrideExistingAssignments;
    TMP_Text templateTimerText;
    Vector2 scrollPosition;

    readonly List<TimerSceneResult> scanResults = new List<TimerSceneResult>();

    [MenuItem("Tools/Nusantara Battle/Setup Gameplay Timers")]
    static void OpenWindow()
    {
        GetWindow<GameplayTimerBatchTool>("Gameplay Timer Tool");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Gameplay Timer Batch Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        timerObjectName = EditorGUILayout.TextField("Timer Object Name", timerObjectName);
        initialTimerText = EditorGUILayout.TextField("Initial Timer Text", initialTimerText);
        fontSize = EditorGUILayout.FloatField("Font Size", fontSize);
        anchorMin = EditorGUILayout.Vector2Field("Anchor Min", anchorMin);
        anchorMax = EditorGUILayout.Vector2Field("Anchor Max", anchorMax);
        pivot = EditorGUILayout.Vector2Field("Pivot", pivot);
        anchoredPosition = EditorGUILayout.Vector2Field("Anchored Position", anchoredPosition);
        sizeDelta = EditorGUILayout.Vector2Field("Size Delta", sizeDelta);
        localScale = EditorGUILayout.Vector3Field("Local Scale", localScale);
        rotation = EditorGUILayout.Vector3Field("Rotation", rotation);
        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField("Font Asset", fontAsset, typeof(TMP_FontAsset), false);
        fontSharedMaterial = (Material)EditorGUILayout.ObjectField("Font Material", fontSharedMaterial, typeof(Material), false);
        fontStyle = (FontStyles)EditorGUILayout.EnumFlagsField("Font Style", fontStyle);
        alignment = (TextAlignmentOptions)EditorGUILayout.EnumPopup("Alignment", alignment);
        fontColor = EditorGUILayout.ColorField("Font Color", fontColor);
        overrideExistingAssignments = EditorGUILayout.Toggle("Override Existing", overrideExistingAssignments);

        TMP_Text previousTemplate = templateTimerText;
        templateTimerText = (TMP_Text)EditorGUILayout.ObjectField("Template Timer Text", templateTimerText, typeof(TMP_Text), true);
        if (templateTimerText != null && templateTimerText != previousTemplate)
            CaptureTemplateSettings(templateTimerText);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Tool ini akan scan semua scene gameplay, skip folder Tarik Tambang dan Lompat Tali, " +
            "lalu mencari manager yang punya field 'timerText'. Jika belum ada, tool akan membuat TMP text " +
            "bernama TimerText di Canvas utama, menghubungkannya ke manager, lalu menyimpan scene.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan Gameplay Scenes"))
            ScanScenes();

        if (GUILayout.Button("Create / Assign Gameplay Timers"))
            ApplyToScenes();

        if (GUILayout.Button("Move TimerText To Top Of Canvas"))
            MoveTimersToTopOfCanvas();

        EditorGUILayout.Space();
        DrawResults();
    }

    void DrawResults()
    {
        if (scanResults.Count == 0)
            return;

        EditorGUILayout.LabelField("Results", EditorStyles.boldLabel);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (TimerSceneResult result in scanResults)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Scene", result.scenePath);
            EditorGUILayout.LabelField("Managers", result.managerCount.ToString());
            EditorGUILayout.LabelField("Canvas", string.IsNullOrEmpty(result.canvasPath) ? "(none)" : result.canvasPath);
            EditorGUILayout.LabelField("Timer Object", string.IsNullOrEmpty(result.timerObjectPath) ? "(none)" : result.timerObjectPath);
            EditorGUILayout.LabelField("Assigned", result.assignedCount.ToString());

            if (!string.IsNullOrEmpty(result.note))
                EditorGUILayout.HelpBox(result.note, MessageType.None);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    void ScanScenes()
    {
        scanResults.Clear();

        foreach (string scenePath in EnumerateTargetScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            scanResults.Add(CollectSceneResult(scene));
        }
    }

    void ApplyToScenes()
    {
        scanResults.Clear();

        int changedSceneCount = 0;
        int assignedManagerCount = 0;
        int createdTimerObjectCount = 0;
        TimerTemplateData templateData = LoadTemplateData();

        foreach (string scenePath in EnumerateTargetScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneChanged = false;

            List<MonoBehaviour> managers = FindTimerManagers();
            Canvas canvas = FindBestCanvas();
            TMP_Text timerText = FindOrCreateTimerText(canvas, templateData, ref sceneChanged, ref createdTimerObjectCount);

            int assignedInScene = 0;

            foreach (MonoBehaviour manager in managers)
            {
                if (manager == null)
                    continue;

                if (AssignTimerField(manager, timerText))
                {
                    assignedInScene++;
                    assignedManagerCount++;
                    sceneChanged = true;
                }
            }

            if (sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                changedSceneCount++;
            }

            TimerSceneResult result = CollectSceneResult(scene);
            result.assignedCount = assignedInScene;
            scanResults.Add(result);
        }

        AssetDatabase.SaveAssets();

        StringBuilder summary = new StringBuilder();
        summary.Append("Updated ");
        summary.Append(changedSceneCount);
        summary.Append(" scene(s), assigned ");
        summary.Append(assignedManagerCount);
        summary.Append(" manager field(s), created ");
        summary.Append(createdTimerObjectCount);
        summary.Append(" timer object(s).");

        EditorUtility.DisplayDialog("Gameplay Timer Setup Complete", summary.ToString(), "OK");
    }

    void MoveTimersToTopOfCanvas()
    {
        scanResults.Clear();

        int changedSceneCount = 0;
        int movedTimerCount = 0;

        foreach (string scenePath in EnumerateTargetScenePaths())
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneChanged = false;

            Canvas canvas = FindBestCanvas();
            TMP_Text timerText = FindExistingTimerText();

            if (canvas != null && timerText != null)
            {
                if (MoveTimerToCanvasTop(timerText, canvas))
                {
                    sceneChanged = true;
                    movedTimerCount++;
                }
            }

            if (sceneChanged)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                changedSceneCount++;
            }

            scanResults.Add(CollectSceneResult(scene));
        }

        string summary = "Updated " + changedSceneCount + " scene(s), moved " + movedTimerCount +
                         " TimerText object(s) to the top of Canvas.";
        EditorUtility.DisplayDialog("Timer Hierarchy Update Complete", summary, "OK");
    }

    IEnumerable<string> EnumerateTargetScenePaths()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { SceneRootPath });

        foreach (string sceneGuid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);

            if (scenePath.Contains("Tarik Tambang", StringComparison.OrdinalIgnoreCase))
                continue;

            if (scenePath.Contains("Lompat Tali", StringComparison.OrdinalIgnoreCase))
                continue;

            yield return scenePath;
        }
    }

    TimerSceneResult CollectSceneResult(Scene scene)
    {
        List<MonoBehaviour> managers = FindTimerManagers();
        Canvas canvas = FindBestCanvas();
        TMP_Text timerText = FindExistingTimerText();

        return new TimerSceneResult
        {
            scenePath = scene.path,
            managerCount = managers.Count,
            canvasPath = canvas != null ? GetHierarchyPath(canvas.transform) : string.Empty,
            timerObjectPath = timerText != null ? GetHierarchyPath(timerText.transform) : string.Empty,
            assignedCount = CountAssignedManagers(managers),
            note = managers.Count == 0
                ? "No timer manager found in this scene."
                : canvas == null
                    ? "No canvas found. Tool will create one if needed when applying."
                    : string.Empty
        };
    }

    static List<MonoBehaviour> FindTimerManagers()
    {
        List<MonoBehaviour> results = new List<MonoBehaviour>();
        MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            FieldInfo timerField = GetTimerField(behaviour.GetType());
            if (timerField == null)
                continue;

            results.Add(behaviour);
        }

        return results;
    }

    static int CountAssignedManagers(List<MonoBehaviour> managers)
    {
        int count = 0;

        foreach (MonoBehaviour manager in managers)
        {
            FieldInfo timerField = GetTimerField(manager.GetType());
            if (timerField == null)
                continue;

            if (timerField.GetValue(manager) as TMP_Text != null)
                count++;
        }

        return count;
    }

    static FieldInfo GetTimerField(Type targetType)
    {
        return targetType.GetField("timerText", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    Canvas FindBestCanvas()
    {
        Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null)
                continue;

            if (canvas.isRootCanvas)
                return canvas;
        }

        return null;
    }

    TMP_Text FindExistingTimerText()
    {
        TMP_Text[] texts = UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text text in texts)
        {
            if (text != null && text.gameObject.name == timerObjectName)
                return text;
        }

        return null;
    }

    TMP_Text FindOrCreateTimerText(Canvas canvas, TimerTemplateData templateData, ref bool sceneChanged, ref int createdTimerObjectCount)
    {
        TMP_Text existing = FindExistingTimerText();
        if (existing != null)
        {
            if (ApplyTemplateToExistingTimer(existing, templateData))
                sceneChanged = true;

            return existing;
        }

        if (canvas == null)
            canvas = CreateCanvas(ref sceneChanged);

        GameObject timerObject = new GameObject(timerObjectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(timerObject, "Create Gameplay Timer Text");
        timerObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = timerObject.GetComponent<RectTransform>();
        ApplyRectTransform(rectTransform, templateData);

        TextMeshProUGUI text = timerObject.GetComponent<TextMeshProUGUI>();
        ApplyTextVisual(text, templateData);

        EditorUtility.SetDirty(timerObject);
        EditorUtility.SetDirty(text);
        sceneChanged = true;
        createdTimerObjectCount++;
        return text;
    }

    bool ApplyTemplateToExistingTimer(TMP_Text existing, TimerTemplateData templateData)
    {
        if (existing == null || templateData == null)
            return false;

        bool changed = false;
        RectTransform rectTransform = existing.rectTransform;

        if (rectTransform.anchorMin != templateData.anchorMin)
        {
            Undo.RecordObject(rectTransform, "Update Timer Rect");
            rectTransform.anchorMin = templateData.anchorMin;
            changed = true;
        }

        if (rectTransform.anchorMax != templateData.anchorMax)
        {
            if (!changed)
                Undo.RecordObject(rectTransform, "Update Timer Rect");
            rectTransform.anchorMax = templateData.anchorMax;
            changed = true;
        }

        if (rectTransform.pivot != templateData.pivot)
        {
            if (!changed)
                Undo.RecordObject(rectTransform, "Update Timer Rect");
            rectTransform.pivot = templateData.pivot;
            changed = true;
        }

        if (rectTransform.anchoredPosition != templateData.anchoredPosition)
        {
            if (!changed)
                Undo.RecordObject(rectTransform, "Update Timer Rect");
            rectTransform.anchoredPosition = templateData.anchoredPosition;
            changed = true;
        }

        if (rectTransform.sizeDelta != templateData.sizeDelta)
        {
            if (!changed)
                Undo.RecordObject(rectTransform, "Update Timer Rect");
            rectTransform.sizeDelta = templateData.sizeDelta;
            changed = true;
        }

        if (rectTransform.localScale != templateData.localScale)
        {
            if (!changed)
                Undo.RecordObject(rectTransform, "Update Timer Rect");
            rectTransform.localScale = templateData.localScale;
            changed = true;
        }

        if (rectTransform.localEulerAngles != templateData.rotation)
        {
            if (!changed)
                Undo.RecordObject(rectTransform, "Update Timer Rect");
            rectTransform.localEulerAngles = templateData.rotation;
            changed = true;
        }

        if (existing.text != templateData.text ||
            existing.font != templateData.fontAsset ||
            existing.fontSharedMaterial != templateData.fontSharedMaterial ||
            !Mathf.Approximately(existing.fontSize, templateData.fontSize) ||
            existing.fontStyle != templateData.fontStyle ||
            existing.alignment != templateData.alignment ||
            existing.color != templateData.color ||
            existing.raycastTarget != templateData.raycastTarget)
        {
            Undo.RecordObject(existing, "Update Timer Text Style");
            existing.text = templateData.text;
            existing.font = templateData.fontAsset;
            existing.fontSharedMaterial = templateData.fontSharedMaterial;
            existing.fontSize = templateData.fontSize;
            existing.fontStyle = templateData.fontStyle;
            existing.alignment = templateData.alignment;
            existing.color = templateData.color;
            existing.raycastTarget = templateData.raycastTarget;
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(rectTransform);
            EditorUtility.SetDirty(existing);
        }

        return changed;
    }

    bool MoveTimerToCanvasTop(TMP_Text timerText, Canvas canvas)
    {
        if (timerText == null || canvas == null)
            return false;

        RectTransform timerTransform = timerText.rectTransform;
        Transform canvasTransform = canvas.transform;
        bool changed = false;

        if (timerTransform.parent != canvasTransform)
        {
            Undo.SetTransformParent(timerTransform, canvasTransform, "Move TimerText To Canvas");
            timerTransform.SetParent(canvasTransform, false);
            changed = true;
        }

        if (timerTransform.GetSiblingIndex() != 0)
        {
            Undo.RecordObject(canvasTransform, "Move TimerText To Top Of Canvas");
            timerTransform.SetSiblingIndex(0);
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(timerTransform);
            EditorUtility.SetDirty(canvasTransform);
        }

        return changed;
    }

    TimerTemplateData LoadTemplateData()
    {
        return new TimerTemplateData
        {
            anchorMin = anchorMin,
            anchorMax = anchorMax,
            pivot = pivot,
            anchoredPosition = anchoredPosition,
            sizeDelta = sizeDelta,
            localScale = localScale,
            rotation = rotation,
            text = initialTimerText,
            fontAsset = fontAsset,
            fontSharedMaterial = fontSharedMaterial,
            fontSize = fontSize,
            fontStyle = fontStyle,
            alignment = alignment,
            color = fontColor,
            raycastTarget = false
        };
    }

    void CaptureTemplateSettings(TMP_Text source)
    {
        if (source == null)
            return;

        RectTransform rectTransform = source.rectTransform;
        anchorMin = rectTransform.anchorMin;
        anchorMax = rectTransform.anchorMax;
        pivot = rectTransform.pivot;
        anchoredPosition = rectTransform.anchoredPosition;
        sizeDelta = rectTransform.sizeDelta;
        localScale = rectTransform.localScale;
        rotation = rectTransform.localEulerAngles;
        initialTimerText = source.text;
        fontAsset = source.font;
        fontSharedMaterial = source.fontSharedMaterial;
        fontSize = source.fontSize;
        fontStyle = source.fontStyle;
        alignment = source.alignment;
        fontColor = source.color;
        Repaint();
    }

    void ApplyRectTransform(RectTransform rectTransform, TimerTemplateData templateData)
    {
        if (templateData != null)
        {
            rectTransform.anchorMin = templateData.anchorMin;
            rectTransform.anchorMax = templateData.anchorMax;
            rectTransform.pivot = templateData.pivot;
            rectTransform.anchoredPosition = templateData.anchoredPosition;
            rectTransform.sizeDelta = templateData.sizeDelta;
            rectTransform.localScale = templateData.localScale;
            rectTransform.localEulerAngles = templateData.rotation;
            return;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.localScale = localScale;
        rectTransform.localEulerAngles = rotation;
    }

    void ApplyTextVisual(TextMeshProUGUI text, TimerTemplateData templateData)
    {
        if (templateData != null)
        {
            text.text = templateData.text;
            text.font = templateData.fontAsset;
            text.fontSharedMaterial = templateData.fontSharedMaterial;
            text.fontSize = templateData.fontSize;
            text.fontStyle = templateData.fontStyle;
            text.alignment = templateData.alignment;
            text.color = templateData.color;
            text.raycastTarget = templateData.raycastTarget;
            return;
        }

        text.text = initialTimerText;
        text.font = fontAsset;
        text.fontSharedMaterial = fontSharedMaterial;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = fontColor;
        text.raycastTarget = false;
    }

    Canvas CreateCanvas(ref bool sceneChanged)
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Gameplay Timer Canvas");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        sceneChanged = true;
        return canvas;
    }

    bool AssignTimerField(MonoBehaviour manager, TMP_Text timerText)
    {
        if (manager == null || timerText == null)
            return false;

        FieldInfo timerField = GetTimerField(manager.GetType());
        if (timerField == null)
            return false;

        TMP_Text currentValue = timerField.GetValue(manager) as TMP_Text;
        if (currentValue == timerText)
            return false;

        if (currentValue != null && !overrideExistingAssignments)
            return false;

        Undo.RecordObject(manager, "Assign Gameplay Timer Text");
        timerField.SetValue(manager, timerText);
        EditorUtility.SetDirty(manager);
        return true;
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

    class TimerSceneResult
    {
        public string scenePath;
        public int managerCount;
        public string canvasPath;
        public string timerObjectPath;
        public int assignedCount;
        public string note;
    }

    class TimerTemplateData
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 localScale;
        public Vector3 rotation;
        public string text;
        public TMP_FontAsset fontAsset;
        public Material fontSharedMaterial;
        public float fontSize;
        public FontStyles fontStyle;
        public TextAlignmentOptions alignment;
        public Color color;
        public bool raycastTarget;
    }
}
