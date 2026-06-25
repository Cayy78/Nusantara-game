using UnityEngine;

[System.Serializable]
public class PanjatPinangCharacterEntry
{
    public string characterName;
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
}

public class PanjatPinangCharacterLoader : MonoBehaviour
{
    public SpriteRenderer player1Renderer;
    public SpriteRenderer player2Renderer;
    public Animator player1Animator;
    public Animator player2Animator;
    public GameObject player2Root;

    public PanjatPinangCharacterEntry[] player1Characters;
    public PanjatPinangCharacterEntry[] player2Characters;

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
            "Karakter Panjat Pinang",
            entry => entry.characterName,
            entry => entry.sprite,
            entry => entry.animatorController,
            false,
            string.Empty,
            false,
            player1IndicatorSettings,
            player2IndicatorSettings);
    }
}
