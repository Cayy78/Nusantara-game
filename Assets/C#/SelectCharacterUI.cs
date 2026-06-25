using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectCharacterUI : MonoBehaviour
{
    const string SelectLabel = "SELECT";
    const string CancelLabel = "CANCEL";

    [Header("Panels")]
    public GameObject singlePlayerPanel;
    public GameObject multiPlayerPanel;

    [Header("Buttons")]
    public Button nextButton;

    [Header("Single Player UI")]
    public Image singlePlayerPreviewImage;
    public TMP_Text singlePlayerCharacterText;
    public TMP_Text singlePlayerStatusText;

    [Header("Multiplayer UI")]
    public Image player1PreviewImage;
    public Image player2PreviewImage;
    public TMP_Text player1CharacterText;
    public TMP_Text player2CharacterText;
    public TMP_Text player1StatusText;
    public TMP_Text player2StatusText;

    [Header("Single Player Character Data")]
    public string[] singlePlayerCharacterNames;
    public Sprite[] singlePlayerCharacterSprites;

    [Header("Player 1 Character Data")]
    public string[] player1CharacterNames;
    public Sprite[] player1CharacterSprites;

    [Header("Player 2 Character Data")]
    public string[] player2CharacterNames;
    public Sprite[] player2CharacterSprites;

    private int singlePlayerIndex;
    private int player1Index;
    private int player2Index;

    private bool singlePlayerSelected;
    private bool player1Selected;
    private bool player2Selected;

    void Start()
    {
        ShowPanelForMode();
        RefreshUI();
        UpdateNextButton();
    }

    public void SinglePlayerLeft()
    {
        if (singlePlayerSelected || singlePlayerCharacterNames == null || singlePlayerCharacterNames.Length == 0)
            return;

        singlePlayerIndex--;
        if (singlePlayerIndex < 0)
            singlePlayerIndex = singlePlayerCharacterNames.Length - 1;

        RefreshUI();
    }

    public void SinglePlayerRight()
    {
        if (singlePlayerSelected || singlePlayerCharacterNames == null || singlePlayerCharacterNames.Length == 0)
            return;

        singlePlayerIndex++;
        if (singlePlayerIndex >= singlePlayerCharacterNames.Length)
            singlePlayerIndex = 0;

        RefreshUI();
    }

    public void SelectSinglePlayerCharacter()
    {
        if (GameSelectionManager.Instance == null || singlePlayerCharacterNames == null || singlePlayerCharacterNames.Length == 0)
            return;

        if (singlePlayerSelected)
        {
            GameSelectionManager.Instance.SetPlayer1Character(string.Empty);
            GameSelectionManager.Instance.SetPlayer2Character(string.Empty);
            singlePlayerSelected = false;
        }
        else
        {
            GameSelectionManager.Instance.SetPlayer1Character(singlePlayerCharacterNames[singlePlayerIndex]);
            GameSelectionManager.Instance.SetPlayer2Character(string.Empty);
            singlePlayerSelected = true;
        }

        RefreshUI();
        UpdateNextButton();
    }

    public void Player1Left()
    {
        if (player1Selected || player1CharacterNames == null || player1CharacterNames.Length == 0)
            return;

        player1Index--;
        if (player1Index < 0)
            player1Index = player1CharacterNames.Length - 1;

        RefreshUI();
    }

    public void Player1Right()
    {
        if (player1Selected || player1CharacterNames == null || player1CharacterNames.Length == 0)
            return;

        player1Index++;
        if (player1Index >= player1CharacterNames.Length)
            player1Index = 0;

        RefreshUI();
    }

    public void Player2Left()
    {
        if (player2Selected || player2CharacterNames == null || player2CharacterNames.Length == 0)
            return;

        player2Index--;
        if (player2Index < 0)
            player2Index = player2CharacterNames.Length - 1;

        RefreshUI();
    }

    public void Player2Right()
    {
        if (player2Selected || player2CharacterNames == null || player2CharacterNames.Length == 0)
            return;

        player2Index++;
        if (player2Index >= player2CharacterNames.Length)
            player2Index = 0;

        RefreshUI();
    }

    public void SelectPlayer1Character()
    {
        if (GameSelectionManager.Instance == null || player1CharacterNames == null || player1CharacterNames.Length == 0)
            return;

        if (player1Selected)
        {
            GameSelectionManager.Instance.SetPlayer1Character(string.Empty);
            player1Selected = false;
        }
        else
        {
            GameSelectionManager.Instance.SetPlayer1Character(player1CharacterNames[player1Index]);
            player1Selected = true;
        }

        RefreshUI();
        UpdateNextButton();
    }

    public void SelectPlayer2Character()
    {
        if (GameSelectionManager.Instance == null || player2CharacterNames == null || player2CharacterNames.Length == 0)
            return;

        if (player2Selected)
        {
            GameSelectionManager.Instance.SetPlayer2Character(string.Empty);
            player2Selected = false;
        }
        else
        {
            GameSelectionManager.Instance.SetPlayer2Character(player2CharacterNames[player2Index]);
            player2Selected = true;
        }

        RefreshUI();
        UpdateNextButton();
    }

    public void GoToGameplayInfo()
    {
        if (!CanGoNext() || GameSelectionManager.Instance == null)
            return;

        string targetScene = string.IsNullOrEmpty(GameSelectionManager.Instance.pendingGameplayInfoScene)
            ? "GameplayInfo"
            : GameSelectionManager.Instance.pendingGameplayInfoScene;

        SceneManager.LoadScene(targetScene);
    }

    public void BackToSelectDifficulty()
    {
        if (GameSelectionManager.Instance == null)
            return;

        string targetScene =
            GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer
                ? "SelectDifficultyMultiPlayer"
                : "SelectDifficultySinglePlayer";

        SceneManager.LoadScene(targetScene);
    }

    void ShowPanelForMode()
    {
        bool isMultiPlayer =
            GameSelectionManager.Instance != null &&
            GameSelectionManager.Instance.selectedMode == PlayMode.MultiPlayer;

        if (singlePlayerPanel != null)
            singlePlayerPanel.SetActive(!isMultiPlayer);

        if (multiPlayerPanel != null)
            multiPlayerPanel.SetActive(isMultiPlayer);
    }

    void RefreshUI()
    {
        if (singlePlayerCharacterText != null &&
            singlePlayerCharacterNames != null &&
            singlePlayerCharacterNames.Length > singlePlayerIndex)
        {
            singlePlayerCharacterText.text = singlePlayerCharacterNames[singlePlayerIndex];
        }

        if (singlePlayerPreviewImage != null &&
            singlePlayerCharacterSprites != null &&
            singlePlayerCharacterSprites.Length > singlePlayerIndex)
        {
            singlePlayerPreviewImage.sprite = singlePlayerCharacterSprites[singlePlayerIndex];
        }

        if (singlePlayerStatusText != null)
            singlePlayerStatusText.text = singlePlayerSelected ? CancelLabel : SelectLabel;

        if (player1CharacterText != null &&
            player1CharacterNames != null &&
            player1CharacterNames.Length > player1Index)
        {
            player1CharacterText.text = player1CharacterNames[player1Index];
        }

        if (player2CharacterText != null &&
            player2CharacterNames != null &&
            player2CharacterNames.Length > player2Index)
        {
            player2CharacterText.text = player2CharacterNames[player2Index];
        }

        if (player1PreviewImage != null &&
            player1CharacterSprites != null &&
            player1CharacterSprites.Length > player1Index)
        {
            player1PreviewImage.sprite = player1CharacterSprites[player1Index];
        }

        if (player2PreviewImage != null &&
            player2CharacterSprites != null &&
            player2CharacterSprites.Length > player2Index)
        {
            player2PreviewImage.sprite = player2CharacterSprites[player2Index];
        }

        if (player1StatusText != null)
            player1StatusText.text = player1Selected ? CancelLabel : SelectLabel;

        if (player2StatusText != null)
            player2StatusText.text = player2Selected ? CancelLabel : SelectLabel;
    }

    void UpdateNextButton()
    {
        if (nextButton != null)
            nextButton.interactable = CanGoNext();
    }

    bool CanGoNext()
    {
        if (GameSelectionManager.Instance == null)
            return false;

        if (GameSelectionManager.Instance.selectedMode == PlayMode.SinglePlayer)
            return singlePlayerSelected;

        return player1Selected && player2Selected;
    }
}
