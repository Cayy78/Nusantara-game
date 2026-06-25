using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayMusicVolumeBatchUpdater : EditorWindow
{
    float targetVolume = 1f;
    string objectNameFilter = "GameplayMusic";
    bool useObjectNameFilter = true;

    [MenuItem("Tools/Nusantara Battle/Update Gameplay Music Volume")]
    static void OpenWindow()
    {
        GetWindow<GameplayMusicVolumeBatchUpdater>("Gameplay Music Volume");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Set Gameplay Music Volume In All Scenes", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        targetVolume = EditorGUILayout.Slider("Target Volume", targetVolume, 0f, 1f);
        useObjectNameFilter = EditorGUILayout.Toggle("Filter By Object Name", useObjectNameFilter);

        using (new EditorGUI.DisabledScope(!useObjectNameFilter))
        {
            objectNameFilter = EditorGUILayout.TextField("Object Name", objectNameFilter);
        }

        EditorGUILayout.HelpBox(
            "This will scan every scene in Assets/Scenes and set matching GameplayMusic AudioSource volume values to the selected target.",
            MessageType.Info);

        if (GUILayout.Button("Update All Gameplay Music Volumes"))
            UpdateAllGameplayMusicVolumes();
    }

    void UpdateAllGameplayMusicVolumes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        int updatedSceneCount = 0;
        int updatedSourceCount = 0;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneChanged = false;

            AudioSource[] audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int j = 0; j < audioSources.Length; j++)
            {
                AudioSource audioSource = audioSources[j];
                if (audioSource == null)
                    continue;

                if (useObjectNameFilter && audioSource.gameObject.name != objectNameFilter)
                    continue;

                if (Mathf.Approximately(audioSource.volume, targetVolume))
                    continue;

                Undo.RecordObject(audioSource, "Update Gameplay Music Volume");
                audioSource.volume = targetVolume;
                EditorUtility.SetDirty(audioSource);
                sceneChanged = true;
                updatedSourceCount++;
            }

            if (!sceneChanged)
                continue;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            updatedSceneCount++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog(
            "Gameplay Music Volume Update Complete",
            "Updated " + updatedSourceCount + " AudioSource component(s) across " + updatedSceneCount + " scene(s).",
            "OK");
    }
}
