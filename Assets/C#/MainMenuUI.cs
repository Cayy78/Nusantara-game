using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public string playSceneName = "SelectMode";
    public GameObject creditPanel;
    public GameObject aboutPanel;

    private void Start()
    {
        if (creditPanel != null)
            creditPanel.SetActive(false);

        if (aboutPanel != null)
            aboutPanel.SetActive(false);
    }

    public void GoToPlayMenu()
    {
        Debug.Log("Tombol Play ditekan");
        SceneManager.LoadScene(playSceneName);
    }

    public void GoToSettings()
    {
        Debug.Log("Tombol Settings ditekan");
        SceneManager.LoadScene("Settings");
    }

    public void OpenCreditPanel()
    {
        if (aboutPanel != null)
            aboutPanel.SetActive(false);

        if (creditPanel != null)
            creditPanel.SetActive(true);
    }

    public void CloseCreditPanel()
    {
        if (creditPanel != null)
            creditPanel.SetActive(false);
    }

    public void OpenAboutPanel()
    {
        if (creditPanel != null)
            creditPanel.SetActive(false);

        if (aboutPanel != null)
            aboutPanel.SetActive(true);
    }

    public void CloseAboutPanel()
    {
        if (aboutPanel != null)
            aboutPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Tombol Quit ditekan");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
