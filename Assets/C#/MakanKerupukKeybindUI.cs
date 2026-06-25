using TMPro;
using UnityEngine;

public class MakanKerupukKeybindUI : MonoBehaviour
{
    public TMP_Text player1Text;
    public TMP_Text player2Text;

    private bool waitingForPlayer1;
    private bool waitingForPlayer2;

    void Start()
    {
        RefreshUI();
    }

    void Update()
    {
        if (KeybindManager.Instance == null)
            return;

        if (!waitingForPlayer1 && !waitingForPlayer2)
            return;

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                if (waitingForPlayer1)
                {
                    KeybindManager.Instance.makanKerupukPlayer1 = key;
                    waitingForPlayer1 = false;
                }
                else if (waitingForPlayer2)
                {
                    KeybindManager.Instance.makanKerupukPlayer2 = key;
                    waitingForPlayer2 = false;
                }

                RefreshUI();
                KeybindManager.Instance.SaveBindings();
                break;
            }
        }
    }

    public void RebindPlayer1()
    {
        waitingForPlayer1 = true;
        waitingForPlayer2 = false;
        SetText(player1Text, "Press key...");
    }

    public void RebindPlayer2()
    {
        waitingForPlayer2 = true;
        waitingForPlayer1 = false;
        SetText(player2Text, "Press key...");
    }

    public void RefreshUI()
    {
        if (KeybindManager.Instance == null)
            return;

        SetText(player1Text, KeybindManager.Instance.makanKerupukPlayer1.ToString());
        SetText(player2Text, KeybindManager.Instance.makanKerupukPlayer2.ToString());
    }

    void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }
}
