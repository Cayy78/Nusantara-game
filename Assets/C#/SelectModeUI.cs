using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectModeUI : MonoBehaviour
{
    public string singlePlayerSelectGameScene = "SelectGameSinglePlayer";
    public string multiPlayerSelectGameScene = "SelectGameMultiPlayer";
    public string mainMenuScene = "MainMenu";

    public void SelectSinglePlayerMode()
    {
        if (GameSelectionManager.Instance == null)
            return;

        GameSelectionManager.Instance.ResetRunSelections();
        GameSelectionManager.Instance.SetMode(PlayMode.SinglePlayer);
        SceneManager.LoadScene(singlePlayerSelectGameScene);
    }

    public void SelectMultiPlayerMode()
    {
        if (GameSelectionManager.Instance == null)
            return;

        GameSelectionManager.Instance.ResetRunSelections();
        GameSelectionManager.Instance.SetMode(PlayMode.MultiPlayer);
        SceneManager.LoadScene(multiPlayerSelectGameScene);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
