using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    public const string GameplayVolumePlayerPrefsKey = "GameplayVolume";
    public const string LastNonZeroGameplayVolumePlayerPrefsKey = "LastNonZeroGameplayVolume";

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject cheatPanel;

    [Header("Countdown")]
    public GameplayStartCountdown gameplayStartCountdown;

    [Header("Controls")]
    public Slider volumeSlider;
    public Image muteToggleImage;
    public Sprite speakerOnSprite;
    public Sprite speakerOffSprite;
    public TMP_InputField cheatInputField;
    public TMP_Text cheatFeedbackText;
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode cheatModifierKey = KeyCode.LeftShift;
    public KeyCode cheatPauseKey = KeyCode.RightBracket;

    [Header("Scenes")]
    public string mainMenuScene = "MainMenu";
    public string singlePlayerSelectGameScene = "SelectGameSinglePlayer";
    public string multiPlayerSelectGameScene = "SelectGameMultiPlayer";
    public string singlePlayerSelectDifficultyScene = "SelectDifficultySinglePlayer";
    public string multiPlayerSelectDifficultyScene = "SelectDifficultyMultiPlayer";

    private bool isPaused;
    private bool pausedDuringCountdown;

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindFirstObjectByType<GameplayStartCountdown>();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (cheatPanel != null)
            cheatPanel.SetActive(false);

        float savedVolume = GetSavedGameplayVolume();
        ApplyGameplayVolume(savedVolume);

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetGameplayVolume);
        }

        UpdateMuteToggleVisual(savedVolume);
    }

    void Update()
    {
        if (IsCheatPauseShortcutPressed())
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame(true);
        }
        else if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame(false);
        }
    }

    bool IsCheatPauseShortcutPressed()
    {
        bool modifierHeld =
            Input.GetKey(cheatModifierKey) ||
            (cheatModifierKey == KeyCode.LeftShift && Input.GetKey(KeyCode.RightShift)) ||
            (cheatModifierKey == KeyCode.RightShift && Input.GetKey(KeyCode.LeftShift));

        return modifierHeld && Input.GetKeyDown(cheatPauseKey);
    }

    public void PauseGame()
    {
        PauseGame(false);
    }

    public void PauseGame(bool openCheatPanel)
    {
        isPaused = true;

        pausedDuringCountdown =
            gameplayStartCountdown != null &&
            gameplayStartCountdown.IsCountdownRunning();

        if (pausedDuringCountdown)
            gameplayStartCountdown.PauseCountdown();

        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (openCheatPanel)
            OpenCheatPanel();
        else if (cheatPanel != null)
            cheatPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (cheatPanel != null)
            cheatPanel.SetActive(false);

        if (pausedDuringCountdown && gameplayStartCountdown != null)
            gameplayStartCountdown.ResumeCountdownFromStart();

        pausedDuringCountdown = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void GoToSelectGame()
    {
        Time.timeScale = 1f;

        string targetScene = IsMultiPlayer()
            ? multiPlayerSelectGameScene
            : singlePlayerSelectGameScene;

        SceneManager.LoadScene(targetScene);
    }

    public void GoToSelectDifficulty()
    {
        Time.timeScale = 1f;

        string targetScene = IsMultiPlayer()
            ? multiPlayerSelectDifficultyScene
            : singlePlayerSelectDifficultyScene;

        SceneManager.LoadScene(targetScene);
    }

    public void CheatPlayer1Win()
    {
        TriggerCheat(target => target.CheatPlayer1Win());
    }

    public void CheatPlayer2Win()
    {
        TriggerCheat(target => target.CheatPlayer2Win());
    }

    public void CheatDraw()
    {
        TriggerCheat(target => target.CheatDraw());
    }

    public void OpenCheatPanel()
    {
        if (cheatPanel != null)
            cheatPanel.SetActive(true);

        SetCheatFeedback(string.Empty);

        if (cheatInputField == null || !cheatInputField.gameObject.activeInHierarchy)
            return;

        cheatInputField.text = string.Empty;
        cheatInputField.ActivateInputField();
        cheatInputField.Select();
    }

    public void CloseCheatPanel()
    {
        if (cheatPanel != null)
            cheatPanel.SetActive(false);

        SetCheatFeedback(string.Empty);
    }

    public void SubmitCheat()
    {
        if (cheatInputField == null)
        {
            SetCheatFeedback("Cheat input belum diisi.");
            return;
        }

        string cheatCode = NormalizeCheatCode(cheatInputField.text);

        switch (cheatCode)
        {
            case "p1":
            case "p1win":
            case "player1":
            case "player1win":
                CheatPlayer1Win();
                break;

            case "p2":
            case "p2win":
            case "player2":
            case "player2win":
                CheatPlayer2Win();
                break;

            case "both":
            case "draw":
            case "bothfinish":
            case "simultaneous":
                CheatDraw();
                break;

            default:
                SetCheatFeedback("Cheat tidak dikenali.");
                return;
        }

        if (cheatInputField != null)
            cheatInputField.text = string.Empty;
    }

    public void SetGameplayVolume(float volume)
    {
        if (volume > 0f)
            SaveLastNonZeroGameplayVolume(volume);

        SaveGameplayVolume(volume);
        ApplyGameplayVolume(volume);
        UpdateMuteToggleVisual(volume);
    }

    public void ToggleMuteGameplayVolume()
    {
        float currentVolume = GetSavedGameplayVolume();

        if (currentVolume > 0.0001f)
        {
            SaveLastNonZeroGameplayVolume(currentVolume);
            SetGameplayVolume(0f);
            return;
        }

        float restoredVolume = GetLastNonZeroGameplayVolume();
        SetGameplayVolume(restoredVolume);
    }

    public static float GetSavedGameplayVolume()
    {
        return PlayerPrefs.GetFloat(GameplayVolumePlayerPrefsKey, AudioListener.volume);
    }

    public static void SaveGameplayVolume(float volume)
    {
        PlayerPrefs.SetFloat(GameplayVolumePlayerPrefsKey, volume);
        PlayerPrefs.Save();
    }

    public static float GetLastNonZeroGameplayVolume()
    {
        return PlayerPrefs.GetFloat(LastNonZeroGameplayVolumePlayerPrefsKey, 1f);
    }

    public static void SaveLastNonZeroGameplayVolume(float volume)
    {
        if (volume <= 0f)
            return;

        PlayerPrefs.SetFloat(LastNonZeroGameplayVolumePlayerPrefsKey, volume);
        PlayerPrefs.Save();
    }

    public static void ApplyGameplayVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    void TriggerCheat(System.Action<IGameplayCheatTarget> action)
    {
        MonoBehaviour[] targets = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour target in targets)
        {
            if (target is not IGameplayCheatTarget cheatTarget)
                continue;

            isPaused = false;
            Time.timeScale = 1f;

            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (cheatPanel != null)
                cheatPanel.SetActive(false);

            pausedDuringCountdown = false;
            action(cheatTarget);
            return;
        }

        SetCheatFeedback("Manager gameplay belum support cheat.");
        Debug.LogWarning("Tidak ada manager gameplay yang support cheat di scene ini.");
    }

    static string NormalizeCheatCode(string rawCheatCode)
    {
        if (string.IsNullOrWhiteSpace(rawCheatCode))
            return string.Empty;

        return rawCheatCode
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty);
    }

    void SetCheatFeedback(string message)
    {
        if (cheatFeedbackText != null)
            cheatFeedbackText.text = message;
    }

    void UpdateMuteToggleVisual(float volume)
    {
        if (muteToggleImage == null)
            return;

        bool isMuted = volume <= 0.0001f;

        if (isMuted && speakerOffSprite != null)
            muteToggleImage.sprite = speakerOffSprite;
        else if (!isMuted && speakerOnSprite != null)
            muteToggleImage.sprite = speakerOnSprite;

        if (volumeSlider != null && !Mathf.Approximately(volumeSlider.value, volume))
            volumeSlider.SetValueWithoutNotify(volume);
    }

    bool IsMultiPlayer()
    {
        return GameSelectionManager.Instance != null &&
               GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;
    }
}
