using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuMuteBatchTool : EditorWindow
{
    Sprite speakerOnSprite;
    Sprite speakerOffSprite;
    bool autoAssignMuteImage = true;
    bool autoAssignMuteButtonOnClick = true;
    string muteImageObjectName = "MuteButton";
    Vector2 scrollPosition;
    readonly List<PauseMenuScanResult> scanResults = new List<PauseMenuScanResult>();

    [MenuItem("Tools/Nusantara Battle/Update Pause Menu Mute Buttons")]
    static void OpenWindow()
    {
        GetWindow<PauseMenuMuteBatchTool>("Pause Menu Mute Tool");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Pause Menu Mute Batch Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        speakerOnSprite = (Sprite)EditorGUILayout.ObjectField("Speaker On Sprite", speakerOnSprite, typeof(Sprite), false);
        speakerOffSprite = (Sprite)EditorGUILayout.ObjectField("Speaker Off Sprite", speakerOffSprite, typeof(Sprite), false);

        EditorGUILayout.Space();
        autoAssignMuteImage = EditorGUILayout.Toggle("Auto Assign Mute Image", autoAssignMuteImage);
        autoAssignMuteButtonOnClick = EditorGUILayout.Toggle("Auto Assign Mute Button On Click", autoAssignMuteButtonOnClick);

        using (new EditorGUI.DisabledScope(!autoAssignMuteImage))
        {
            muteImageObjectName = EditorGUILayout.TextField("Mute Image Object Name", muteImageObjectName);
        }

        EditorGUILayout.HelpBox(
            "Tool ini akan scan semua scene di Assets/Scenes, cari PauseMenuUI, lalu bisa:\n" +
            "1. menampilkan status wiring mute button,\n" +
            "2. mengisi speaker on/off sprite,\n" +
            "3. mencoba auto-link field Mute Toggle Image berdasarkan nama object yang konsisten.\n\n" +
            "Catatan: tool ini tidak membuat tombol speaker baru. Layout tombol tetap perlu kamu siapkan sekali di scene/prefab.",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan All Pause Menus"))
            ScanAllPauseMenus();

        using (new EditorGUI.DisabledScope(speakerOnSprite == null && speakerOffSprite == null && !autoAssignMuteImage))
        {
            if (GUILayout.Button("Apply To All Pause Menus"))
                ApplyToAllPauseMenus();
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
            PauseMenuScanResult result = scanResults[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Scene", result.scenePath);
            EditorGUILayout.LabelField("PauseMenuUI", result.objectPath);
            EditorGUILayout.LabelField("Pause Panel", result.hasPausePanel ? "OK" : "Missing");
            EditorGUILayout.LabelField("Mute Button", result.hasMuteButton ? "OK" : "Missing");
            EditorGUILayout.LabelField("Mute Button On Click", result.hasMuteButtonOnClick ? "OK" : "Missing");
            EditorGUILayout.LabelField("Mute Toggle Image", result.hasMuteToggleImage ? "OK" : "Missing");
            EditorGUILayout.LabelField("Speaker On Sprite", result.hasSpeakerOnSprite ? "OK" : "Missing");
            EditorGUILayout.LabelField("Speaker Off Sprite", result.hasSpeakerOffSprite ? "OK" : "Missing");

            if (!string.IsNullOrEmpty(result.note))
                EditorGUILayout.HelpBox(result.note, MessageType.None);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    void ScanAllPauseMenus()
    {
        scanResults.Clear();

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            PauseMenuUI[] pauseMenus = Object.FindObjectsByType<PauseMenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int j = 0; j < pauseMenus.Length; j++)
            {
                PauseMenuUI pauseMenu = pauseMenus[j];
                if (pauseMenu == null)
                    continue;

                scanResults.Add(CreateScanResult(scene, pauseMenu));
            }
        }
    }

    void ApplyToAllPauseMenus()
    {
        scanResults.Clear();

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        int updatedSceneCount = 0;
        int updatedPauseMenuCount = 0;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            PauseMenuUI[] pauseMenus = Object.FindObjectsByType<PauseMenuUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool sceneChanged = false;

            for (int j = 0; j < pauseMenus.Length; j++)
            {
                PauseMenuUI pauseMenu = pauseMenus[j];
                if (pauseMenu == null)
                    continue;

                bool changed = false;

                if (speakerOnSprite != null && pauseMenu.speakerOnSprite != speakerOnSprite)
                {
                    Undo.RecordObject(pauseMenu, "Assign Pause Menu Speaker On Sprite");
                    pauseMenu.speakerOnSprite = speakerOnSprite;
                    changed = true;
                }

                if (speakerOffSprite != null && pauseMenu.speakerOffSprite != speakerOffSprite)
                {
                    if (!changed)
                        Undo.RecordObject(pauseMenu, "Assign Pause Menu Speaker Off Sprite");

                    pauseMenu.speakerOffSprite = speakerOffSprite;
                    changed = true;
                }

                if (autoAssignMuteImage && pauseMenu.muteToggleImage == null)
                {
                    Image foundImage = FindMuteImage(pauseMenu);
                    if (foundImage != null)
                    {
                        if (!changed)
                            Undo.RecordObject(pauseMenu, "Assign Pause Menu Mute Toggle Image");

                        pauseMenu.muteToggleImage = foundImage;
                        changed = true;
                    }
                }

                if (autoAssignMuteButtonOnClick)
                {
                    Button muteButton = FindMuteButton(pauseMenu);
                    if (muteButton != null && !HasMuteToggleListener(muteButton, pauseMenu))
                    {
                        if (!changed)
                            Undo.RecordObject(muteButton, "Assign Pause Menu Mute Button On Click");

                        UnityEventTools.AddPersistentListener(muteButton.onClick, pauseMenu.ToggleMuteGameplayVolume);
                        EditorUtility.SetDirty(muteButton);
                        changed = true;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(pauseMenu);
                    sceneChanged = true;
                    updatedPauseMenuCount++;
                }

                scanResults.Add(CreateScanResult(scene, pauseMenu));
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
        message.Append(updatedPauseMenuCount);
        message.Append(" PauseMenuUI component(s) across ");
        message.Append(updatedSceneCount);
        message.Append(" scene(s).");

        EditorUtility.DisplayDialog("Pause Menu Mute Update Complete", message.ToString(), "OK");
    }

    PauseMenuScanResult CreateScanResult(Scene scene, PauseMenuUI pauseMenu)
    {
        PauseMenuScanResult result = new PauseMenuScanResult
        {
            scenePath = scene.path,
            objectPath = GetHierarchyPath(pauseMenu.transform),
            hasPausePanel = pauseMenu.pausePanel != null,
            hasMuteButton = FindMuteButton(pauseMenu) != null,
            hasMuteButtonOnClick = false,
            hasMuteToggleImage = pauseMenu.muteToggleImage != null,
            hasSpeakerOnSprite = pauseMenu.speakerOnSprite != null,
            hasSpeakerOffSprite = pauseMenu.speakerOffSprite != null
        };

        Button muteButton = FindMuteButton(pauseMenu);
        if (muteButton != null)
            result.hasMuteButtonOnClick = HasMuteToggleListener(muteButton, pauseMenu);

        if (!result.hasMuteToggleImage)
        {
            Image foundImage = FindMuteImage(pauseMenu);
            if (foundImage != null)
                result.note = "Mute image bisa ditemukan otomatis: " + GetHierarchyPath(foundImage.transform);
            else if (autoAssignMuteImage)
                result.note = "Mute image belum ketemu otomatis. Pastikan object icon/tombol speaker bernama \"" + muteImageObjectName + "\".";
        }

        return result;
    }

    Image FindMuteImage(PauseMenuUI pauseMenu)
    {
        if (pauseMenu == null)
            return null;

        Transform root = pauseMenu.pausePanel != null ? pauseMenu.pausePanel.transform : pauseMenu.transform;
        if (root == null)
            return null;

        Transform target = FindChildRecursive(root, muteImageObjectName);
        if (target == null)
            return null;

        Image image = target.GetComponent<Image>();
        if (image != null)
            return image;

        return target.GetComponentInChildren<Image>(true);
    }

    Button FindMuteButton(PauseMenuUI pauseMenu)
    {
        if (pauseMenu == null)
            return null;

        Transform root = pauseMenu.pausePanel != null ? pauseMenu.pausePanel.transform : pauseMenu.transform;
        if (root == null)
            return null;

        Transform target = FindChildRecursive(root, muteImageObjectName);
        if (target == null)
            return null;

        Button button = target.GetComponent<Button>();
        if (button != null)
            return button;

        return target.GetComponentInChildren<Button>(true);
    }

    static bool HasMuteToggleListener(Button button, PauseMenuUI pauseMenu)
    {
        if (button == null || pauseMenu == null)
            return false;

        int eventCount = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < eventCount; i++)
        {
            Object target = button.onClick.GetPersistentTarget(i);
            string methodName = button.onClick.GetPersistentMethodName(i);

            if (target == pauseMenu && methodName == nameof(PauseMenuUI.ToggleMuteGameplayVolume))
                return true;
        }

        return false;
    }

    static Transform FindChildRecursive(Transform root, string objectName)
    {
        if (root == null)
            return null;

        if (root.name == objectName)
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildRecursive(root.GetChild(i), objectName);
            if (found != null)
                return found;
        }

        return null;
    }

    static string GetHierarchyPath(Transform target)
    {
        if (target == null)
            return string.Empty;

        string path = target.name;
        Transform current = target.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    class PauseMenuScanResult
    {
        public string scenePath;
        public string objectPath;
        public bool hasPausePanel;
        public bool hasMuteButton;
        public bool hasMuteButtonOnClick;
        public bool hasMuteToggleImage;
        public bool hasSpeakerOnSprite;
        public bool hasSpeakerOffSprite;
        public string note;
    }
}
