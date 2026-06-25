using TMPro;
using UnityEngine;

public class BakiakKeybindUI : MonoBehaviour
{
    public TMP_Text player1LeftText;
    public TMP_Text player1RightText;
    public TMP_Text player2LeftText;
    public TMP_Text player2RightText;

    private bool waitingForPlayer1Left;
    private bool waitingForPlayer1Right;
    private bool waitingForPlayer2Left;
    private bool waitingForPlayer2Right;

    void Start()
    {
        RefreshUI();
    }

    void Update()
    {
        if (KeybindManager.Instance == null)
            return;

        if (!waitingForPlayer1Left && !waitingForPlayer1Right &&
            !waitingForPlayer2Left && !waitingForPlayer2Right)
            return;

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (!Input.GetKeyDown(key))
                continue;

            if (waitingForPlayer1Left)
            {
                KeybindManager.Instance.bakiakPlayer1Left = key;
                waitingForPlayer1Left = false;
            }
            else if (waitingForPlayer1Right)
            {
                KeybindManager.Instance.bakiakPlayer1Right = key;
                waitingForPlayer1Right = false;
            }
            else if (waitingForPlayer2Left)
            {
                KeybindManager.Instance.bakiakPlayer2Left = key;
                waitingForPlayer2Left = false;
            }
            else if (waitingForPlayer2Right)
            {
                KeybindManager.Instance.bakiakPlayer2Right = key;
                waitingForPlayer2Right = false;
            }

            KeybindManager.Instance.SaveBindings();
            RefreshUI();
            break;
        }
    }

    public void RebindPlayer1Left()
    {
        ResetWaitingFlags();
        waitingForPlayer1Left = true;
        SetText(player1LeftText, "Press...");
    }

    public void RebindPlayer1Right()
    {
        ResetWaitingFlags();
        waitingForPlayer1Right = true;
        SetText(player1RightText, "Press...");
    }

    public void RebindPlayer2Left()
    {
        ResetWaitingFlags();
        waitingForPlayer2Left = true;
        SetText(player2LeftText, "Press...");
    }

    public void RebindPlayer2Right()
    {
        ResetWaitingFlags();
        waitingForPlayer2Right = true;
        SetText(player2RightText, "Press...");
    }

    void ResetWaitingFlags()
    {
        waitingForPlayer1Left = false;
        waitingForPlayer1Right = false;
        waitingForPlayer2Left = false;
        waitingForPlayer2Right = false;
    }

    public void RefreshUI()
    {
        if (KeybindManager.Instance == null)
            return;

        SetText(player1LeftText, KeybindManager.Instance.bakiakPlayer1Left.ToString());
        SetText(player1RightText, KeybindManager.Instance.bakiakPlayer1Right.ToString());
        SetText(player2LeftText, FormatKey(KeybindManager.Instance.bakiakPlayer2Left));
        SetText(player2RightText, FormatKey(KeybindManager.Instance.bakiakPlayer2Right));
    }

    void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }

    string FormatKey(KeyCode key)
    {
        if (key == KeyCode.LeftArrow)
            return "<-";

        if (key == KeyCode.RightArrow)
            return "->";

        return key.ToString();
    }
}
