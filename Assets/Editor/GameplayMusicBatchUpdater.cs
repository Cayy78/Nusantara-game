using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayMusicBatchUpdater : EditorWindow
{
    AudioClip replacementClip;
    string objectNameFilter = "GameplayMusic";
    bool useObjectNameFilter = true;

    [MenuItem("Tools/Nusantara Battle/Update Gameplay Music")]
    static void OpenWindow()
    {
        GetWindow<GameplayMusicBatchUpdater>("Gameplay Music Updater");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Update Gameplay Music In All Scenes", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        replacementClip = (AudioClip)EditorGUILayout.ObjectField("Replacement Clip", replacementClip, typeof(AudioClip), false);
        useObjectNameFilter = EditorGUILayout.Toggle("Filter By Object Name", useObjectNameFilter);

        using (new EditorGUI.DisabledScope(!useObjectNameFilter))
        {
            objectNameFilter = EditorGUILayout.TextField("Object Name", objectNameFilter);
        }

        EditorGUILayout.HelpBox(
            "Select the new AudioClip, then click the button below. " +
            "The tool will scan every scene in Assets/Scenes and replace AudioSource clips on matching GameplayMusic objects.",
            MessageType.Info);

        using (new EditorGUI.DisabledScope(replacementClip == null))
        {
            if (GUILayout.Button("Update All Gameplay Music"))
                UpdateAllGameplayMusic();
        }
    }

    void UpdateAllGameplayMusic()
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

                if (audioSource.clip == replacementClip)
                    continue;

                Undo.RecordObject(audioSource, "Update Gameplay Music Clip");
                audioSource.clip = replacementClip;
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
            "Gameplay Music Update Complete",
            "Updated " + updatedSourceCount + " AudioSource component(s) across " + updatedSceneCount + " scene(s).",
            "OK");
    }
}
