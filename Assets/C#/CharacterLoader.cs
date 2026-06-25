using System;
using UnityEngine;

[System.Serializable]
public class CharacterEntry
{
    public string characterName;
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
}

public static class CharacterLoaderUtility
{
    public static void LoadCharacters<T>(
        SpriteRenderer player1Renderer,
        SpriteRenderer player2Renderer,
        Animator player1Animator,
        Animator player2Animator,
        GameObject player2Root,
        T[] player1Characters,
        T[] player2Characters,
        string warningLabel,
        Func<T, string> getName,
        Func<T, Sprite> getSprite,
        Func<T, RuntimeAnimatorController> getAnimatorController,
        bool keepPlayer2ActiveInSinglePlayer = false,
        string singlePlayerPlayer2CharacterNameOverride = "",
        bool useSinglePlayerBotPartnerMapping = false,
        PlayerArrowIndicatorSettings player1IndicatorSettings = null,
        PlayerArrowIndicatorSettings player2IndicatorSettings = null)
    {
        if (GameSelectionManager.Instance == null)
            return;

        ApplyCharacter(
            player1Renderer,
            player1Animator,
            player1Characters,
            GameSelectionManager.Instance.selectedCharacterPlayer1,
            warningLabel,
            getName,
            getSprite,
            getAnimatorController);

        PlayerArrowIndicatorUtility.EnsureIndicator(
            "Player1ArrowIndicator",
            player1Renderer,
            player1Renderer != null ? player1Renderer.gameObject : null,
            true,
            player1IndicatorSettings);

        if (GameSelectionManager.Instance.selectedMode == PlayMode.SinglePlayer)
        {
            if (!keepPlayer2ActiveInSinglePlayer)
            {
                PlayerArrowIndicatorUtility.EnsureIndicator(
                    "Player2ArrowIndicator",
                    player2Renderer,
                    player2Root != null ? player2Root : (player2Renderer != null ? player2Renderer.gameObject : null),
                    false,
                    player2IndicatorSettings);

                if (player2Root != null)
                    player2Root.SetActive(false);
                else if (player2Renderer != null)
                    player2Renderer.gameObject.SetActive(false);

                return;
            }

            if (player2Root != null)
                player2Root.SetActive(true);
            else if (player2Renderer != null)
                player2Renderer.gameObject.SetActive(true);

            string player2CharacterName = ResolveSinglePlayerPlayer2CharacterName(
                singlePlayerPlayer2CharacterNameOverride,
                useSinglePlayerBotPartnerMapping);

            T[] player2Entries = player2Characters != null && player2Characters.Length > 0
                ? player2Characters
                : player1Characters;

            ApplyCharacter(
                player2Renderer,
                player2Animator,
                player2Entries,
                player2CharacterName,
                warningLabel,
                getName,
                getSprite,
                getAnimatorController);

            PlayerArrowIndicatorUtility.EnsureIndicator(
                "Player2ArrowIndicator",
                player2Renderer,
                player2Root != null ? player2Root : (player2Renderer != null ? player2Renderer.gameObject : null),
                true,
                player2IndicatorSettings);

            return;
        }

        ApplyCharacter(
            player2Renderer,
            player2Animator,
            player2Characters,
            GameSelectionManager.Instance.selectedCharacterPlayer2,
            warningLabel,
            getName,
            getSprite,
            getAnimatorController);

        PlayerArrowIndicatorUtility.EnsureIndicator(
            "Player2ArrowIndicator",
            player2Renderer,
            player2Root != null ? player2Root : (player2Renderer != null ? player2Renderer.gameObject : null),
            true,
            player2IndicatorSettings);
    }

    static string ResolveSinglePlayerPlayer2CharacterName(
        string singlePlayerPlayer2CharacterNameOverride,
        bool useSinglePlayerBotPartnerMapping)
    {
        if (!string.IsNullOrEmpty(singlePlayerPlayer2CharacterNameOverride))
            return singlePlayerPlayer2CharacterNameOverride;

        if (GameSelectionManager.Instance == null)
            return string.Empty;

        if (useSinglePlayerBotPartnerMapping)
        {
            string mappedName = GetSinglePlayerBotPartnerName(GameSelectionManager.Instance.selectedCharacterPlayer1);
            if (!string.IsNullOrEmpty(mappedName))
                return mappedName;
        }

        if (!string.IsNullOrEmpty(GameSelectionManager.Instance.selectedCharacterPlayer2))
            return GameSelectionManager.Instance.selectedCharacterPlayer2;

        return GameSelectionManager.Instance.selectedCharacterPlayer1;
    }

    static string GetSinglePlayerBotPartnerName(string player1CharacterName)
    {
        switch (player1CharacterName)
        {
            case "BIAN":
                return "JOKO";
            case "JOKO":
                return "BIAN";
            case "RANI":
                return "WATI";
            case "WATI":
                return "RANI";
            default:
                return string.Empty;
        }
    }

    static void ApplyCharacter<T>(
        SpriteRenderer targetRenderer,
        Animator targetAnimator,
        T[] characterEntries,
        string selectedCharacterName,
        string warningLabel,
        Func<T, string> getName,
        Func<T, Sprite> getSprite,
        Func<T, RuntimeAnimatorController> getAnimatorController)
    {
        if (targetRenderer == null || characterEntries == null || string.IsNullOrEmpty(selectedCharacterName))
            return;

        for (int i = 0; i < characterEntries.Length; i++)
        {
            T entry = characterEntries[i];
            if (entry == null || getName(entry) != selectedCharacterName)
                continue;

            Sprite sprite = getSprite(entry);
            if (sprite != null)
                targetRenderer.sprite = sprite;

            RuntimeAnimatorController controller = getAnimatorController(entry);
            if (targetAnimator != null && controller != null)
            {
                targetAnimator.runtimeAnimatorController = controller;
                targetAnimator.Rebind();
                targetAnimator.Update(0f);
            }

            return;
        }

        Debug.LogWarning(warningLabel + " tidak ketemu untuk nama: " + selectedCharacterName);
    }
}

public class CharacterLoader : MonoBehaviour
{
    public SpriteRenderer player1Renderer;
    public SpriteRenderer player2Renderer;
    public Animator player1Animator;
    public Animator player2Animator;
    public GameObject player2Root;

    public CharacterEntry[] player1Characters;
    public CharacterEntry[] player2Characters;

    [Tooltip("Nama label untuk warning di Console, misalnya 'Karakter Panjat Pinang' atau 'Karakter Egrang'.")]
    public string warningLabel = "Karakter";
    [Tooltip("Centang jika scene single player juga membutuhkan Player 2 aktif, misalnya untuk bot.")]
    public bool keepPlayer2ActiveInSinglePlayer;
    [Tooltip("Opsional: paksa karakter Player 2 saat single player. Kosongkan jika mau pakai karakter Player 2 terpilih atau fallback ke Player 1.")]
    public string singlePlayerPlayer2CharacterNameOverride = "";
    [Tooltip("Centang jika bot single player harus otomatis memakai pasangan karakter lawan: BIAN-JOKO dan RANI-WATI.")]
    public bool useSinglePlayerBotPartnerMapping;

    [Header("Player Indicators")]
    public PlayerArrowIndicatorSettings player1IndicatorSettings = new PlayerArrowIndicatorSettings
    {
        indicatorColor = new Color32(255, 0, 0, 255)
    };

    public PlayerArrowIndicatorSettings player2IndicatorSettings = new PlayerArrowIndicatorSettings
    {
        indicatorColor = new Color32(0, 26, 255, 255)
    };

    void Start()
    {
        CharacterLoaderUtility.LoadCharacters(
            player1Renderer,
            player2Renderer,
            player1Animator,
            player2Animator,
            player2Root,
            player1Characters,
            player2Characters,
            warningLabel,
            entry => entry.characterName,
            entry => entry.sprite,
            entry => entry.animatorController,
            keepPlayer2ActiveInSinglePlayer,
            singlePlayerPlayer2CharacterNameOverride,
            useSinglePlayerBotPartnerMapping,
            player1IndicatorSettings,
            player2IndicatorSettings);
    }
}
