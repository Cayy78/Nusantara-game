using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMusicManager : MonoBehaviour
{
    public static MenuMusicManager Instance;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip menuMusicClip;

    [Header("Volume")]
    [Range(0f, 1f)] public float defaultVolume = 1f;
    public string volumePlayerPrefsKey = "MenuMusicVolume";

    [Header("Allowed Scenes")]
    public string[] menuSceneNames =
    {
        "MainMenu",
        "Settings",
        "SelectMode",
        "SelectGameSinglePlayer",
        "SelectGameMultiPlayer",
        "SelectGameMultiplayer",
        "SelectDifficultySinglePlayer",
        "SelectDifficultyMultiPlayer",
        "SelectCharacterSinglePlayer",
        "SelectCharacterMultiPlayer",
        "GameplayInfo"
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        SetupAudioSource();
        ApplySavedVolume();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        RefreshForScene(SceneManager.GetActiveScene().name);
    }

    public void RefreshForCurrentScene()
    {
        RefreshForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshForScene(scene.name);
    }

    void SetupAudioSource()
    {
        if (audioSource == null)
            return;

        audioSource.playOnAwake = false;
        audioSource.loop = true;

        if (menuMusicClip != null)
            audioSource.clip = menuMusicClip;
    }

    void ApplySavedVolume()
    {
        if (audioSource == null)
            return;

        float savedVolume = PlayerPrefs.GetFloat(volumePlayerPrefsKey, defaultVolume);
        audioSource.volume = savedVolume;
    }

    void RefreshForScene(string sceneName)
    {
        if (audioSource == null)
            return;

        if (ShouldPlayInScene(sceneName))
        {
            // Menu music uses its own dedicated volume control.
            // Reset the global gameplay listener multiplier while we are in menu scenes
            // so the menu slider can behave independently from gameplay volume.
            AudioListener.volume = 1f;

            if (menuMusicClip != null && audioSource.clip != menuMusicClip)
                audioSource.clip = menuMusicClip;

            if (!audioSource.isPlaying && audioSource.clip != null)
                audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }

    bool ShouldPlayInScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return false;

        string normalizedSceneName = sceneName.Replace(" ", "").ToLowerInvariant();

        if (normalizedSceneName == "gameplayinfo")
            return true;

        if (normalizedSceneName == "selectgamemultiplayer")
            return true;

        if (menuSceneNames == null)
            return false;

        for (int i = 0; i < menuSceneNames.Length; i++)
        {
            if (string.IsNullOrEmpty(menuSceneNames[i]))
                continue;

            string normalizedAllowedName = menuSceneNames[i].Replace(" ", "").ToLowerInvariant();

            if (normalizedAllowedName == normalizedSceneName)
                return true;
        }

        return false;
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = volume;

        PlayerPrefs.SetFloat(volumePlayerPrefsKey, volume);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        if (audioSource != null)
            return audioSource.volume;

        return PlayerPrefs.GetFloat(volumePlayerPrefsKey, defaultVolume);
    }
}
