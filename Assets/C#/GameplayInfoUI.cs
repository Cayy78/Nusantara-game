using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public class GameplayInfoEntry
{
    public GameType game;
    public string englishText;
    [TextArea]
    public string description;
    [TextArea]
    public string howToPlay;
    public string howToPlaySingleVideoUrl;
    public string howToPlayMultiVideoUrl;
}

public class GameplayInfoUI : MonoBehaviour
{
    [Header("Main Info")]
    public TMP_Text titleText;
    public TMP_Text englishText;
    public TMP_Text modeText;
    public TMP_Text difficultyText;
    public TMP_Text descriptionText;

    [Header("Keybind Display")]
    public TMP_Text player1CustomKeyText;
    public TMP_Text player2CustomKeyText;
    public TMP_Text howToPlayKeybindText;

    [Header("How To Play")]
    public GameObject howToPlayPanel;
    public TMP_Text howToPlayTitleText;
    public TMP_Text howToPlayEnglishText;
    public TMP_Text howToPlayModeText;
    public TMP_Text howToPlayText;
    public RawImage howToPlayVideoImage;
    public VideoPlayer howToPlayVideoPlayer;
    public Vector2Int howToPlayVideoResolution = new Vector2Int(1660, 648);

    [Header("Content")]
    public GameplayInfoEntry[] descriptions;

    [Header("Scenes")]
    public string singlePlayerCharacterScene = "SelectCharacterSinglePlayer";
    public string multiPlayerCharacterScene = "SelectCharacterMultiPlayer";

    RenderTexture howToPlayRenderTexture;
    bool isPreparingHowToPlayVideo;

    void Start()
    {
        if (MenuMusicManager.Instance != null)
            MenuMusicManager.Instance.RefreshForCurrentScene();

        CloseHowToPlay();
        RefreshInfo();
    }

    void Update()
    {
        RefreshKeybindDisplay();
    }

    public void StartGameplay()
    {
        if (GameSelectionManager.Instance == null ||
            string.IsNullOrEmpty(GameSelectionManager.Instance.pendingGameplayScene))
            return;

        SceneManager.LoadScene(GameSelectionManager.Instance.pendingGameplayScene);
    }

    public void BackToSelectCharacter()
    {
        if (GameSelectionManager.Instance == null)
            return;

        string targetScene =
            GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer
                ? multiPlayerCharacterScene
                : singlePlayerCharacterScene;

        SceneManager.LoadScene(targetScene);
    }

    public void OpenHowToPlay()
    {
        if (GameSelectionManager.Instance == null)
            return;

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);

        if (howToPlayTitleText != null)
            howToPlayTitleText.text = "How To Play " + GameSelectionManager.Instance.selectedGame;

        SetHowToPlayEnglishText(GameSelectionManager.Instance.selectedGame);

        if (howToPlayModeText != null)
            howToPlayModeText.text = GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer
                ? "Multiplayer"
                : "Single Player";

        RefreshHowToPlayContent(GameSelectionManager.Instance.selectedGame);
        RefreshKeybindDisplay();
    }

    public void CloseHowToPlay()
    {
        StopHowToPlayVideo();

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    void OnDisable()
    {
        UnregisterHowToPlayVideoEvents();
        StopHowToPlayVideo();
    }

    void OnDestroy()
    {
        UnregisterHowToPlayVideoEvents();
        ReleaseHowToPlayRenderTexture();
    }

    void RefreshInfo()
    {
        if (GameSelectionManager.Instance == null)
            return;

        if (titleText != null)
            titleText.text = GameSelectionManager.Instance.selectedGame.ToString();

        if (englishText != null)
        {
            string formattedEnglishText = GetFormattedEnglishText(GameSelectionManager.Instance.selectedGame);
            englishText.text = formattedEnglishText;
            englishText.enabled = !string.IsNullOrEmpty(formattedEnglishText);
        }

        if (modeText != null)
            modeText.text = GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer
                ? "Multiplayer"
                : "Single Player";

        if (difficultyText != null)
            difficultyText.text = GameSelectionManager.Instance.selectedDifficulty.ToString();

        if (descriptionText != null)
            descriptionText.text = GetDescription(GameSelectionManager.Instance.selectedGame);

        if (howToPlayTitleText != null)
            howToPlayTitleText.text = "How To Play " + GameSelectionManager.Instance.selectedGame;

        SetHowToPlayEnglishText(GameSelectionManager.Instance.selectedGame);

        if (howToPlayModeText != null)
            howToPlayModeText.text = GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer
                ? "Multiplayer"
                : "Single Player";

        if (howToPlayPanel == null || howToPlayPanel.activeInHierarchy)
            RefreshHowToPlayContent(GameSelectionManager.Instance.selectedGame);
        else
            StopHowToPlayVideo();

        RefreshKeybindDisplay();
    }

    string GetDescription(GameType game)
    {
        if (descriptions == null)
            return string.Empty;

        for (int i = 0; i < descriptions.Length; i++)
        {
            GameplayInfoEntry entry = descriptions[i];
            if (entry != null && entry.game == game)
                return entry.description;
        }

        return string.Empty;
    }

    void SetHowToPlayEnglishText(GameType game)
    {
        if (howToPlayEnglishText == null)
            return;

        string formattedEnglishText = GetFormattedEnglishText(game);
        howToPlayEnglishText.text = formattedEnglishText;
        howToPlayEnglishText.enabled = !string.IsNullOrEmpty(formattedEnglishText);
    }

    string GetFormattedEnglishText(GameType game)
    {
        if (descriptions == null)
            return string.Empty;

        for (int i = 0; i < descriptions.Length; i++)
        {
            GameplayInfoEntry entry = descriptions[i];
            if (entry == null || entry.game != game || string.IsNullOrWhiteSpace(entry.englishText))
                continue;

            string trimmedEnglishText = entry.englishText.Trim();
            if (trimmedEnglishText.StartsWith("(") && trimmedEnglishText.EndsWith(")"))
                return trimmedEnglishText;

            return "(" + trimmedEnglishText + ")";
        }

        return string.Empty;
    }

    string GetHowToPlay(GameType game)
    {
        if (descriptions == null)
            return string.Empty;

        for (int i = 0; i < descriptions.Length; i++)
        {
            GameplayInfoEntry entry = descriptions[i];
            if (entry != null && entry.game == game)
                return FormatHowToPlay(entry.howToPlay, game);
        }

        return string.Empty;
    }

    string GetHowToPlayVideoUrl(GameType game)
    {
        if (descriptions == null || GameSelectionManager.Instance == null)
            return string.Empty;

        bool isMultiplayer = GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        for (int i = 0; i < descriptions.Length; i++)
        {
            GameplayInfoEntry entry = descriptions[i];
            if (entry == null || entry.game != game)
                continue;

            string videoLocation = isMultiplayer
                ? entry.howToPlayMultiVideoUrl
                : entry.howToPlaySingleVideoUrl;

            return ResolveVideoLocation(videoLocation);
        }

        return string.Empty;
    }

    string ResolveVideoLocation(string videoLocation)
    {
        if (string.IsNullOrWhiteSpace(videoLocation))
            return string.Empty;

        string trimmedLocation = videoLocation.Trim();

        if (Uri.IsWellFormedUriString(trimmedLocation, UriKind.Absolute))
            return trimmedLocation;

        string normalizedRelativePath = trimmedLocation.Replace("\\", "/").TrimStart('/');
        string basePath = Application.streamingAssetsPath.Replace("\\", "/").TrimEnd('/');
        return basePath + "/" + normalizedRelativePath;
    }

    void RefreshHowToPlayContent(GameType game)
    {
        string howToPlayVideoUrl = GetHowToPlayVideoUrl(game);
        bool useVideo = CanUseHowToPlayVideo(howToPlayVideoUrl);

        if (useVideo)
            PlayHowToPlayVideo(howToPlayVideoUrl);
        else
            StopHowToPlayVideo();

        if (howToPlayVideoImage != null)
            howToPlayVideoImage.enabled = useVideo;

        if (howToPlayText != null)
        {
            bool useText = !useVideo;
            howToPlayText.text = useText ? GetHowToPlay(game) : string.Empty;
            howToPlayText.enabled = useText;
        }

        RefreshKeybindDisplay();
    }

    bool CanUseHowToPlayVideo(string videoUrl)
    {
        if (howToPlayVideoPlayer == null || howToPlayVideoImage == null)
            return false;

        return !string.IsNullOrWhiteSpace(videoUrl);
    }

    void RefreshKeybindDisplay()
    {
        if (GameSelectionManager.Instance == null)
            return;

        GameType game = GameSelectionManager.Instance.selectedGame;
        bool isMultiplayer = GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        SetKeybindText(player1CustomKeyText, BuildPlayerKeybindLabel(game, 1));
        SetKeybindText(player2CustomKeyText, isMultiplayer ? BuildPlayerKeybindLabel(game, 2) : string.Empty);
        SetKeybindText(howToPlayKeybindText, BuildHowToPlayKeybindSummary(game));
    }

    void SetKeybindText(TMP_Text targetText, string value)
    {
        if (targetText == null)
            return;

        bool hasValue = !string.IsNullOrEmpty(value);

        if (targetText.text != value)
            targetText.text = value;

        if (targetText.enabled != hasValue)
            targetText.enabled = hasValue;
    }

    string BuildPlayerKeybindLabel(GameType game, int playerIndex)
    {
        bool isMultiplayer = GameSelectionManager.Instance != null &&
                             GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        if (!isMultiplayer && playerIndex == 2)
            return string.Empty;

        switch (game)
        {
            case GameType.Egrang:
            case GameType.Bakiak:
                string leftKey = GetDirectionalKeyLabel(game, playerIndex, true);
                string rightKey = GetDirectionalKeyLabel(game, playerIndex, false);
                if (string.IsNullOrEmpty(leftKey) || string.IsNullOrEmpty(rightKey))
                    return string.Empty;

                return leftKey + " / " + rightKey;
            default:
                string primaryKey = GetPrimaryKeyLabel(game, playerIndex);
                if (string.IsNullOrEmpty(primaryKey))
                    return string.Empty;

                return primaryKey;
        }
    }

    string BuildHowToPlayKeybindSummary(GameType game)
    {
        bool isMultiplayer = GameSelectionManager.Instance != null &&
                             GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        string player1Key = BuildPlayerKeybindLabel(game, 1);
        string player2Key = BuildPlayerKeybindLabel(game, 2);

        if (isMultiplayer && !string.IsNullOrEmpty(player2Key))
            return "Tombol Saat Ini: " + player1Key + " | " + player2Key;

        return string.IsNullOrEmpty(player1Key)
            ? string.Empty
            : "Tombol Saat Ini: " + player1Key;
    }

    void PlayHowToPlayVideo(string videoUrl)
    {
        if (howToPlayVideoPlayer == null || howToPlayVideoImage == null)
            return;

        if (!howToPlayVideoPlayer.isActiveAndEnabled)
            return;

        EnsureHowToPlayRenderTexture();

        UnregisterHowToPlayVideoEvents();
        howToPlayVideoPlayer.Stop();
        howToPlayVideoPlayer.clip = null;
        howToPlayVideoPlayer.url = string.Empty;
        if (string.IsNullOrWhiteSpace(videoUrl))
            return;

        howToPlayVideoPlayer.source = VideoSource.Url;
        howToPlayVideoPlayer.url = videoUrl.Trim();

        howToPlayVideoPlayer.isLooping = true;
        howToPlayVideoPlayer.playOnAwake = false;
        howToPlayVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        howToPlayVideoPlayer.targetTexture = howToPlayRenderTexture;

        howToPlayVideoImage.texture = howToPlayRenderTexture;
        howToPlayVideoPlayer.prepareCompleted += OnHowToPlayVideoPrepared;
        howToPlayVideoPlayer.errorReceived += OnHowToPlayVideoError;
        isPreparingHowToPlayVideo = true;
        howToPlayVideoPlayer.Prepare();
    }

    void StopHowToPlayVideo()
    {
        isPreparingHowToPlayVideo = false;

        if (howToPlayVideoPlayer != null)
            howToPlayVideoPlayer.Stop();

        if (howToPlayVideoImage != null)
            howToPlayVideoImage.texture = null;
    }

    void OnHowToPlayVideoPrepared(VideoPlayer source)
    {
        if (source == null || !isPreparingHowToPlayVideo)
            return;

        isPreparingHowToPlayVideo = false;
        source.Play();
    }

    void OnHowToPlayVideoError(VideoPlayer source, string message)
    {
        isPreparingHowToPlayVideo = false;
        Debug.LogWarning("How To Play video failed to load: " + message);
    }

    void UnregisterHowToPlayVideoEvents()
    {
        if (howToPlayVideoPlayer == null)
            return;

        howToPlayVideoPlayer.prepareCompleted -= OnHowToPlayVideoPrepared;
        howToPlayVideoPlayer.errorReceived -= OnHowToPlayVideoError;
    }

    void EnsureHowToPlayRenderTexture()
    {
        int width = Mathf.Max(16, howToPlayVideoResolution.x);
        int height = Mathf.Max(16, howToPlayVideoResolution.y);

        if (howToPlayRenderTexture != null &&
            howToPlayRenderTexture.width == width &&
            howToPlayRenderTexture.height == height)
            return;

        ReleaseHowToPlayRenderTexture();

        howToPlayRenderTexture = new RenderTexture(width, height, 0)
        {
            name = "HowToPlayRenderTexture"
        };
        howToPlayRenderTexture.Create();
    }

    void ReleaseHowToPlayRenderTexture()
    {
        if (howToPlayRenderTexture == null)
            return;

        if (howToPlayVideoPlayer != null && howToPlayVideoPlayer.targetTexture == howToPlayRenderTexture)
            howToPlayVideoPlayer.targetTexture = null;

        howToPlayRenderTexture.Release();
        Destroy(howToPlayRenderTexture);
        howToPlayRenderTexture = null;
    }

    string FormatHowToPlay(string rawText, GameType game)
    {
        if (string.IsNullOrEmpty(rawText))
            return string.Empty;

        string modeSpecificText = ExtractModeSpecificHowToPlay(rawText);
        string formatted = ReplaceHowToPlayTokens(modeSpecificText, game);
        return ApplyLegacyKeyInsertion(formatted, game);
    }

    string ExtractModeSpecificHowToPlay(string rawText)
    {
        if (string.IsNullOrEmpty(rawText) || GameSelectionManager.Instance == null)
            return rawText;

        int singleStart = FindSectionIndex(rawText, "Singleplayer:");
        if (singleStart < 0)
            singleStart = FindSectionIndex(rawText, "Single Player:");

        int multiStart = FindSectionIndex(rawText, "Multiplayer:");
        if (multiStart < 0)
            multiStart = FindSectionIndex(rawText, "Multi Player:");

        if (singleStart < 0 && multiStart < 0)
            return rawText;

        bool isMultiplayer = GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        if (isMultiplayer)
        {
            if (multiStart < 0)
                return rawText;

            int contentStart = multiStart + GetSectionLabelLength(rawText, multiStart);
            return rawText.Substring(contentStart).Trim();
        }

        if (singleStart < 0)
            return rawText;

        int singleContentStart = singleStart + GetSectionLabelLength(rawText, singleStart);
        int singleContentLength = multiStart > singleContentStart
            ? multiStart - singleContentStart
            : rawText.Length - singleContentStart;

        return rawText.Substring(singleContentStart, singleContentLength).Trim();
    }

    int FindSectionIndex(string rawText, string label)
    {
        return rawText.IndexOf(label, System.StringComparison.OrdinalIgnoreCase);
    }

    int GetSectionLabelLength(string rawText, int labelStart)
    {
        int colonIndex = rawText.IndexOf(':', labelStart);
        if (colonIndex < 0)
            return 0;

        return (colonIndex - labelStart) + 1;
    }

    string ReplaceHowToPlayTokens(string rawText, GameType game)
    {
        return rawText
            .Replace("{KEY}", GetPrimaryKeyLabel(game, 1))
            .Replace("{P1_KEY}", GetPrimaryKeyLabel(game, 1))
            .Replace("{P2_KEY}", GetPrimaryKeyLabel(game, 2))
            .Replace("{P1_LEFT}", GetDirectionalKeyLabel(game, 1, true))
            .Replace("{P1_RIGHT}", GetDirectionalKeyLabel(game, 1, false))
            .Replace("{P2_LEFT}", GetDirectionalKeyLabel(game, 2, true))
            .Replace("{P2_RIGHT}", GetDirectionalKeyLabel(game, 2, false));
    }

    string ApplyLegacyKeyInsertion(string text, GameType game)
    {
        string primaryKey = GetPrimaryKeyLabel(game, 1);
        string secondaryKey = GetPrimaryKeyLabel(game, 2);

        switch (game)
        {
            case GameType.Egrang:
            case GameType.Bakiak:
                return ApplyDirectionalKeyInsertion(text, game);
            case GameType.LompatTali:
                return ApplyLompatTaliKeyInsertion(text, primaryKey, secondaryKey);
            default:
                return ApplySingleButtonKeyInsertion(text, primaryKey, secondaryKey);
        }
    }

    string ApplySingleButtonKeyInsertion(string text, string primaryKey, string secondaryKey)
    {
        if (string.IsNullOrEmpty(primaryKey))
            return text;

        string result = text;
        bool isMultiplayer = GameSelectionManager.Instance != null &&
                             GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;
        string multiplayerKeys = FormatMultiplayerSingleButtonKeys(primaryKey, secondaryKey);
        string keyLabelForSentence = isMultiplayer && !string.IsNullOrEmpty(secondaryKey)
            ? multiplayerKeys
            : primaryKey;

        if (isMultiplayer && !string.IsNullOrEmpty(secondaryKey) &&
            result.Contains("Setiap pemain menekan tombol untuk"))
        {
            result = result.Replace(
                "Setiap pemain menekan tombol untuk",
                "Tekan tombol " + multiplayerKeys + " untuk");
            return AppendMultiplayerSingleButtonSummaryIfMissing(result, primaryKey, secondaryKey);
        }

        if (result.Contains("Tekan dan tahan tombol untuk"))
            result = result.Replace("Tekan dan tahan tombol untuk", "Tekan dan tahan tombol " + keyLabelForSentence + " untuk");

        if (result.Contains("Tekan tombol untuk"))
            result = result.Replace("Tekan tombol untuk", "Tekan tombol " + keyLabelForSentence + " untuk");

        if (result.Contains("Tekan tombol saat"))
            result = result.Replace("Tekan tombol saat", "Tekan tombol " + keyLabelForSentence + " saat");

        if (result.Contains("Tekan tombol secepat mungkin"))
            result = result.Replace("Tekan tombol secepat mungkin", "Tekan tombol " + keyLabelForSentence + " secepat mungkin");

        if (result.Contains("lepaskan tombol untuk"))
            result = result.Replace("lepaskan tombol untuk", "lepaskan tombol " + keyLabelForSentence + " untuk");

        if (result.Contains("lalu lepaskan tombol untuk"))
            result = result.Replace("lalu lepaskan tombol untuk", "lalu lepaskan tombol " + keyLabelForSentence + " untuk");

        if (isMultiplayer && !string.IsNullOrEmpty(secondaryKey))
            result = AppendMultiplayerSingleButtonSummaryIfMissing(result, primaryKey, secondaryKey);

        return result;
    }

    string ApplyDirectionalKeyInsertion(string text, GameType game)
    {
        bool isMultiplayer = GameSelectionManager.Instance != null &&
                             GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        string p1Left = GetDirectionalKeyLabel(game, 1, true);
        string p1Right = GetDirectionalKeyLabel(game, 1, false);
        string p2Left = GetDirectionalKeyLabel(game, 2, true);
        string p2Right = GetDirectionalKeyLabel(game, 2, false);

        if (isMultiplayer)
        {
            if (text.Contains("Tekan tombol kiri dan kanan secara bergantian"))
            {
                return text.Replace(
                    "Tekan tombol kiri dan kanan secara bergantian",
                    "Player 1 menekan tombol kiri (" + p1Left + ") dan kanan (" + p1Right + "), sedangkan Player 2 menekan tombol kiri (" + p2Left + ") dan kanan (" + p2Right + ") secara bergantian");
            }

            return text + "\n\nTombol saat ini: Player 1 kiri (" + p1Left + "), kanan (" + p1Right + "). Player 2 kiri (" + p2Left + "), kanan (" + p2Right + ").";
        }

        if (text.Contains("Tekan tombol kiri dan kanan secara bergantian"))
        {
            return text.Replace(
                "Tekan tombol kiri dan kanan secara bergantian",
                "Tekan tombol kiri (" + p1Left + ") dan kanan (" + p1Right + ") secara bergantian");
        }

        return text + "\n\nTombol saat ini: kiri (" + p1Left + ") dan kanan (" + p1Right + ").";
    }

    string ApplyLompatTaliKeyInsertion(string text, string primaryKey, string secondaryKey)
    {
        if (GameSelectionManager.Instance != null &&
            GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer &&
            !string.IsNullOrEmpty(secondaryKey))
        {
            string multiplayerKeys = FormatMultiplayerSingleButtonKeys(primaryKey, secondaryKey);

            if (text.Contains("Setiap pemain menekan tombol untuk melompat"))
            {
                return text.Replace(
                    "Setiap pemain menekan tombol untuk melompat",
                    "Tekan tombol " + multiplayerKeys + " untuk melompat");
            }

            return text + "\n\nTombol saat ini: " + multiplayerKeys + ".";
        }

        if (text.Contains("Tekan tombol untuk melompat"))
            return text.Replace("Tekan tombol untuk melompat", "Tekan tombol (" + primaryKey + ") untuk melompat");

        return text;
    }

    string AppendMultiplayerSingleButtonSummaryIfMissing(string text, string primaryKey, string secondaryKey)
    {
        if (string.IsNullOrEmpty(secondaryKey))
            return text;

        string multiplayerKeys = FormatMultiplayerSingleButtonKeys(primaryKey, secondaryKey);
        string player2KeyMarker = secondaryKey + " (P2)";

        if (text.Contains(player2KeyMarker) || text.Contains(multiplayerKeys))
            return text;

        return text + "\n\nTombol saat ini: " + multiplayerKeys + ".";
    }

    string FormatMultiplayerSingleButtonKeys(string primaryKey, string secondaryKey)
    {
        if (string.IsNullOrEmpty(secondaryKey))
            return primaryKey;

        return primaryKey + " (P1) / " + secondaryKey + " (P2)";
    }

    string GetPrimaryKeyLabel(GameType game, int playerIndex)
    {
        KeybindManager keybindManager = KeybindManager.Instance;
        if (keybindManager == null)
            return string.Empty;

        bool isMultiplayer = GameSelectionManager.Instance != null &&
                             GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        KeyCode keyCode = KeyCode.None;

        switch (game)
        {
            case GameType.PanjatPinang:
                keyCode = isMultiplayer
                    ? (playerIndex == 2 ? keybindManager.panjatPinangPlayer2 : keybindManager.panjatPinangPlayer1)
                    : keybindManager.panjatPinangSingle;
                break;
            case GameType.BalapKarung:
                keyCode = isMultiplayer
                    ? (playerIndex == 2 ? keybindManager.balapKarungPlayer2 : keybindManager.balapKarungPlayer1)
                    : keybindManager.balapKarungSingle;
                break;
            case GameType.TarikTambang:
                keyCode = isMultiplayer
                    ? (playerIndex == 2 ? keybindManager.tarikTambangPlayer2 : keybindManager.tarikTambangPlayer1)
                    : keybindManager.tarikTambangSingle;
                break;
            case GameType.MakanKerupuk:
                keyCode = isMultiplayer
                    ? (playerIndex == 2 ? keybindManager.makanKerupukPlayer2 : keybindManager.makanKerupukPlayer1)
                    : keybindManager.makanKerupukSingle;
                break;
            case GameType.PakuBotol:
                keyCode = isMultiplayer
                    ? (playerIndex == 2 ? keybindManager.pakuBotolPlayer2 : keybindManager.pakuBotolPlayer1)
                    : keybindManager.pakuBotolSingle;
                break;
            case GameType.LemparSandal:
                keyCode = isMultiplayer
                    ? (playerIndex == 2 ? keybindManager.lemparSandalPlayer2 : keybindManager.lemparSandalPlayer1)
                    : keybindManager.lemparSandalSingle;
                break;
            case GameType.Kelereng:
                keyCode = isMultiplayer
                    ? (playerIndex == 2 ? keybindManager.kelerengPlayer2 : keybindManager.kelerengPlayer1)
                    : keybindManager.kelerengSingle;
                break;
            case GameType.LompatTali:
                keyCode = isMultiplayer
                    ? (playerIndex == 2 ? keybindManager.lompatTaliPlayer2 : keybindManager.lompatTaliPlayer1)
                    : keybindManager.lompatTaliSingle;
                break;
        }

        return FormatKeyCodeLabel(keyCode);
    }

    string GetDirectionalKeyLabel(GameType game, int playerIndex, bool isLeft)
    {
        KeybindManager keybindManager = KeybindManager.Instance;
        if (keybindManager == null)
            return string.Empty;

        bool isMultiplayer = GameSelectionManager.Instance != null &&
                             GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        KeyCode keyCode = KeyCode.None;

        switch (game)
        {
            case GameType.Egrang:
                if (!isMultiplayer)
                    keyCode = isLeft ? keybindManager.egrangSingleLeft : keybindManager.egrangSingleRight;
                else if (playerIndex == 2)
                    keyCode = isLeft ? keybindManager.egrangPlayer2Left : keybindManager.egrangPlayer2Right;
                else
                    keyCode = isLeft ? keybindManager.egrangPlayer1Left : keybindManager.egrangPlayer1Right;
                break;
            case GameType.Bakiak:
                if (!isMultiplayer)
                    keyCode = isLeft ? keybindManager.bakiakSingleLeft : keybindManager.bakiakSingleRight;
                else if (playerIndex == 2)
                    keyCode = isLeft ? keybindManager.bakiakPlayer2Left : keybindManager.bakiakPlayer2Right;
                else
                    keyCode = isLeft ? keybindManager.bakiakPlayer1Left : keybindManager.bakiakPlayer1Right;
                break;
        }

        return FormatKeyCodeLabel(keyCode);
    }

    string FormatKeyCodeLabel(KeyCode keyCode)
    {
        if (keyCode == KeyCode.None)
            return string.Empty;

        string raw = keyCode.ToString();

        if (raw.StartsWith("Alpha"))
            return raw.Substring(5);

        if (raw.StartsWith("Keypad"))
            return "Keypad " + raw.Substring(6);

        StringBuilder builder = new StringBuilder(raw.Length + 8);
        for (int i = 0; i < raw.Length; i++)
        {
            char current = raw[i];

            if (i > 0 && char.IsUpper(current) && !char.IsUpper(raw[i - 1]))
                builder.Append(' ');

            builder.Append(current);
        }

        return builder.ToString();
    }
}
