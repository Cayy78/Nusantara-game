using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectGameUI : MonoBehaviour
{
    public string singlePlayerDifficultyScene = "SelectDifficultySinglePlayer";
    public string multiPlayerDifficultyScene = "SelectDifficultyMultiPlayer";
    public string selectModeScene = "SelectMode";

    public void SelectPanjatPinang()
    {
        GameSelectionManager.Instance.SetGame(GameType.PanjatPinang);
        LoadSelectDifficultyScene();
    }

    public void SelectBalapKarung()
    {
        GameSelectionManager.Instance.SetGame(GameType.BalapKarung);
        LoadSelectDifficultyScene();
    }

    public void SelectTarikTambang()
    {
        GameSelectionManager.Instance.SetGame(GameType.TarikTambang);
        LoadSelectDifficultyScene();
    }

    public void SelectMakanKerupuk()
    {
        GameSelectionManager.Instance.SetGame(GameType.MakanKerupuk);
        LoadSelectDifficultyScene();
    }

    public void SelectPakuBotol()
    {
        GameSelectionManager.Instance.SetGame(GameType.PakuBotol);
        LoadSelectDifficultyScene();
    }

    public void SelectLemparSandal()
    {
        GameSelectionManager.Instance.SetGame(GameType.LemparSandal);
        LoadSelectDifficultyScene();
    }

    public void SelectKelereng()
    {
        GameSelectionManager.Instance.SetGame(GameType.Kelereng);
        LoadSelectDifficultyScene();
    }

    public void SelectLompatTali()
    {
        GameSelectionManager.Instance.SetGame(GameType.LompatTali);
        LoadSelectDifficultyScene();
    }

    public void SelectEgrang()
    {
        GameSelectionManager.Instance.SetGame(GameType.Egrang);
        LoadSelectDifficultyScene();
    }

    public void SelectBakiak()
    {
        GameSelectionManager.Instance.SetGame(GameType.Bakiak);
        LoadSelectDifficultyScene();
    }

    public void BackToSelectMode()
    {
        SceneManager.LoadScene(selectModeScene);
    }

    void LoadSelectDifficultyScene()
    {
        if (GameSelectionManager.Instance == null)
            return;

        string targetScene =
            GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer
                ? multiPlayerDifficultyScene
                : singlePlayerDifficultyScene;

        SceneManager.LoadScene(targetScene);
    }
}
