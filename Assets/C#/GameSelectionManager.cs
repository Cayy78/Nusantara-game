using UnityEngine;

public enum GameType
{
    PanjatPinang,
    BalapKarung,
    TarikTambang,
    MakanKerupuk,
    PakuBotol,
    LemparSandal,
    Kelereng,
    LompatTali,
    Egrang,
    Bakiak
}

public enum PlayMode
{
    SinglePlayer,
    MultiPlayer
}

public class GameSelectionManager : MonoBehaviour
{
    public static GameSelectionManager Instance;

    public GameType selectedGame;
    public PlayMode selectedMode = PlayMode.SinglePlayer;
    public DifficultyLevel selectedDifficulty = DifficultyLevel.Easy;
    public string selectedCharacterPlayer1;
    public string selectedCharacterPlayer2;
    public string pendingGameplayScene;
    public string pendingGameplayInfoScene = "GameplayInfo";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGame(GameType game)
    {
        selectedGame = game;
    }

    public void SetMode(PlayMode mode)
    {
        selectedMode = mode;
    }

    public void SetDifficulty(DifficultyLevel difficulty)
    {
        selectedDifficulty = difficulty;
    }

    public void SetPlayer1Character(string characterName)
    {
        selectedCharacterPlayer1 = characterName;
    }

    public void SetPlayer2Character(string characterName)
    {
        selectedCharacterPlayer2 = characterName;
    }

    public void SetPendingGameplayScene(string sceneName)
    {
        pendingGameplayScene = sceneName;
    }

    public void SetPendingGameplayInfoScene(string sceneName)
    {
        pendingGameplayInfoScene = sceneName;
    }

    public void ClearPendingGameplayScene()
    {
        pendingGameplayScene = string.Empty;
    }

    public void ClearSelectedCharacters()
    {
        selectedCharacterPlayer1 = string.Empty;
        selectedCharacterPlayer2 = string.Empty;
    }

    public void ResetRunSelections()
    {
        selectedGame = GameType.PanjatPinang;
        selectedMode = PlayMode.SinglePlayer;
        selectedDifficulty = DifficultyLevel.Easy;
        ClearSelectedCharacters();
        pendingGameplayScene = string.Empty;
        pendingGameplayInfoScene = "GameplayInfo";
    }
}
