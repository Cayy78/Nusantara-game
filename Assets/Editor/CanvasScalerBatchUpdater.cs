using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CanvasScalerBatchUpdater
{
    const float ReferenceWidth = 1920f;
    const float ReferenceHeight = 1080f;
    const float MatchValue = 0.5f;

    [MenuItem("Tools/Nusantara Battle/Update All Canvas Scalers")]
    public static void UpdateAllCanvasScalers()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        int updatedCanvasCount = 0;
        int updatedSceneCount = 0;

        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool sceneChanged = false;

            CanvasScaler[] scalers = Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            for (int j = 0; j < scalers.Length; j++)
            {
                CanvasScaler scaler = scalers[j];
                if (scaler == null)
                    continue;

                bool changed = false;

                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    Undo.RecordObject(scaler, "Update Canvas Scaler");
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    changed = true;
                }

                if (scaler.referenceResolution != new Vector2(ReferenceWidth, ReferenceHeight))
                {
                    if (!changed)
                        Undo.RecordObject(scaler, "Update Canvas Scaler");

                    scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
                    changed = true;
                }

                if (scaler.screenMatchMode != CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                {
                    if (!changed)
                        Undo.RecordObject(scaler, "Update Canvas Scaler");

                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    changed = true;
                }

                if (!Mathf.Approximately(scaler.matchWidthOrHeight, MatchValue))
                {
                    if (!changed)
                        Undo.RecordObject(scaler, "Update Canvas Scaler");

                    scaler.matchWidthOrHeight = MatchValue;
                    changed = true;
                }

                if (!changed)
                    continue;

                EditorUtility.SetDirty(scaler);
                sceneChanged = true;
                updatedCanvasCount++;
            }

            if (!sceneChanged)
                continue;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            updatedSceneCount++;
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog(
            "Canvas Scaler Update",
            "Updated " + updatedCanvasCount + " CanvasScaler component(s) across " + updatedSceneCount + " scene(s).",
            "OK");
    }
}
