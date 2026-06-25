using UnityEngine;

[System.Serializable]
public class CharacterSpriteEntry
{
    public string characterName;
    public Sprite sprite;
    public RuntimeAnimatorController animatorController;
}

public class EgrangCharacterLoader : MonoBehaviour
{
    public SpriteRenderer player1Renderer;
    public SpriteRenderer player2Renderer;
    public Animator player1Animator;
    public Animator player2Animator;

    public CharacterSpriteEntry[] player1Characters;
    public CharacterSpriteEntry[] player2Characters;

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
            null,
            player1Characters,
            player2Characters,
            "Karakter Egrang",
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
