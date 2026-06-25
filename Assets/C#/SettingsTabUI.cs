using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsTabUI : MonoBehaviour
{
    public GameObject soundsPanel;
    public GameObject customKeyPanel;

    public GameObject customKeyMultiplayerPanel;
    public GameObject customKeySinglePlayerPanel;

    public void ShowSounds()
    {
        soundsPanel.SetActive(true);
        customKeyPanel.SetActive(false);
    }

    public void ShowCustomKey()
    {
        soundsPanel.SetActive(false);
        customKeyPanel.SetActive(true);
        ShowCustomKeyMultiplayer();
    }

    public void ShowCustomKeyMultiplayer()
    {
        customKeyMultiplayerPanel.SetActive(true);
        customKeySinglePlayerPanel.SetActive(false);
    }

    public void ShowCustomKeySinglePlayer()
    {
        customKeyMultiplayerPanel.SetActive(false);
        customKeySinglePlayerPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
