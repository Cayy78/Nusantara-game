using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class GameplaySceneEntry
{
    public PlayMode mode;
    public GameType game;
    public DifficultyLevel difficulty;
    public string gameplaySceneName;
}

public class SelectDifficultyUI : MonoBehaviour
{
    public string singlePlayerCharacterScene = "SelectCharacterSinglePlayer";
    public string multiPlayerCharacterScene = "SelectCharacterMultiPlayer";
    public string gameplayInfoScene = "GameplayInfo";
    public string singlePlayerSelectGameScene = "SelectGameSinglePlayer";
    public string multiPlayerSelectGameScene = "SelectGameMultiPlayer";

    public GameplaySceneEntry[] gameplayScenes;

    public void SelectEasy()
    {
        ApplyDifficultyAndGoNext(DifficultyLevel.Easy);
    }

    public void SelectMedium()
    {
        ApplyDifficultyAndGoNext(DifficultyLevel.Medium);
    }

    public void SelectHard()
    {
        ApplyDifficultyAndGoNext(DifficultyLevel.Hard);
    }

    public void BackToSelectGame()
    {
        if (GameSelectionManager.Instance == null)
        {
            SceneManager.LoadScene(singlePlayerSelectGameScene);
            return;
        }

        string targetScene =
            GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer
                ? multiPlayerSelectGameScene
                : singlePlayerSelectGameScene;

        SceneManager.LoadScene(targetScene);
    }

    void ApplyDifficultyAndGoNext(DifficultyLevel difficulty)
    {
        if (GameSelectionManager.Instance == null)
            return;

        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.SetDifficulty(difficulty);

        GameSelectionManager.Instance.SetDifficulty(difficulty);
        GameSelectionManager.Instance.ClearSelectedCharacters();
        GameSelectionManager.Instance.SetPendingGameplayInfoScene(gameplayInfoScene);
        GameSelectionManager.Instance.SetPendingGameplayScene(GetSelectedGameplayScene(difficulty));

        string targetScene =
            GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer
                ? multiPlayerCharacterScene
                : singlePlayerCharacterScene;

        SceneManager.LoadScene(targetScene);
    }

    string GetSelectedGameplayScene(DifficultyLevel difficulty)
    {
        if (gameplayScenes == null || GameSelectionManager.Instance == null)
            return string.Empty;

        for (int i = 0; i < gameplayScenes.Length; i++)
        {
            GameplaySceneEntry entry = gameplayScenes[i];
            if (entry == null)
                continue;

            if (entry.mode == GameSelectionManager.Instance.selectedMode &&
                entry.game == GameSelectionManager.Instance.selectedGame &&
                entry.difficulty == difficulty)
            {
                return entry.gameplaySceneName;
            }
        }

        Debug.LogWarning("Scene gameplay belum diisi untuk kombinasi mode/game/difficulty yang dipilih.");
        return string.Empty;
    }
}
