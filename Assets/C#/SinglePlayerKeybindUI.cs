using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SinglePlayerKeybindUI : MonoBehaviour
{
    public TMP_Text panjatPinangText;
    public TMP_Text balapKarungText;
    public TMP_Text tarikTambangText;
    public TMP_Text makanKerupukText;
    public TMP_Text pakuBotolText;
    public TMP_Text lemparSandalText;
    public TMP_Text kelerengText;
    public TMP_Text lompatTaliText;
    public TMP_Text egrangLeftText;
    public TMP_Text egrangRightText;
    public TMP_Text bakiakLeftText;
    public TMP_Text bakiakRightText;

    private enum WaitingKey
    {
        None,
        PanjatPinang,
        BalapKarung,
        TarikTambang,
        MakanKerupuk,
        PakuBotol,
        LemparSandal,
        Kelereng,
        LompatTali,
        EgrangLeft,
        EgrangRight,
        BakiakLeft,
        BakiakRight
    }

    private WaitingKey waitingKey = WaitingKey.None;
    private bool ignoreCurrentFrameInput;
    private bool waitForMouseRelease;

    void Start()
    {
        RefreshUI();
    }

    void Update()
    {
        if (waitingKey == WaitingKey.None)
            return;

        if (KeybindManager.Instance == null)
            return;

        if (ignoreCurrentFrameInput)
        {
            ignoreCurrentFrameInput = false;
            return;
        }

        if (waitForMouseRelease)
        {
            if (IsAnyMouseButtonPressed())
                return;

            waitForMouseRelease = false;
        }

        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (IsMouseKey(key))
                continue;

            if (!Input.GetKeyDown(key))
                continue;

            ApplyKey(key);
            KeybindManager.Instance.SaveBindings();
            waitingKey = WaitingKey.None;
            RefreshUI();
            break;
        }
    }

    public void RebindPanjatPinang() { StartRebind(WaitingKey.PanjatPinang, panjatPinangText, "Press key..."); }
    public void RebindBalapKarung() { StartRebind(WaitingKey.BalapKarung, balapKarungText, "Press key..."); }
    public void RebindTarikTambang() { StartRebind(WaitingKey.TarikTambang, tarikTambangText, "Press key..."); }
    public void RebindMakanKerupuk() { StartRebind(WaitingKey.MakanKerupuk, makanKerupukText, "Press key..."); }
    public void RebindPakuBotol() { StartRebind(WaitingKey.PakuBotol, pakuBotolText, "Press key..."); }
    public void RebindLemparSandal() { StartRebind(WaitingKey.LemparSandal, lemparSandalText, "Press key..."); }
    public void RebindKelereng() { StartRebind(WaitingKey.Kelereng, kelerengText, "Press key..."); }
    public void RebindLompatTali() { StartRebind(WaitingKey.LompatTali, lompatTaliText, "Press key..."); }
    public void RebindEgrangLeft() { StartRebind(WaitingKey.EgrangLeft, egrangLeftText, "Press..."); }
    public void RebindEgrangRight() { StartRebind(WaitingKey.EgrangRight, egrangRightText, "Press..."); }
    public void RebindBakiakLeft() { StartRebind(WaitingKey.BakiakLeft, bakiakLeftText, "Press..."); }
    public void RebindBakiakRight() { StartRebind(WaitingKey.BakiakRight, bakiakRightText, "Press..."); }

    void ApplyKey(KeyCode key)
    {
        switch (waitingKey)
        {
            case WaitingKey.PanjatPinang:
                KeybindManager.Instance.panjatPinangSingle = key;
                break;
            case WaitingKey.BalapKarung:
                KeybindManager.Instance.balapKarungSingle = key;
                break;
            case WaitingKey.TarikTambang:
                KeybindManager.Instance.tarikTambangSingle = key;
                break;
            case WaitingKey.MakanKerupuk:
                KeybindManager.Instance.makanKerupukSingle = key;
                break;
            case WaitingKey.PakuBotol:
                KeybindManager.Instance.pakuBotolSingle = key;
                break;
            case WaitingKey.LemparSandal:
                KeybindManager.Instance.lemparSandalSingle = key;
                break;
            case WaitingKey.Kelereng:
                KeybindManager.Instance.kelerengSingle = key;
                break;
            case WaitingKey.LompatTali:
                KeybindManager.Instance.lompatTaliSingle = key;
                break;
            case WaitingKey.EgrangLeft:
                KeybindManager.Instance.egrangSingleLeft = key;
                break;
            case WaitingKey.EgrangRight:
                KeybindManager.Instance.egrangSingleRight = key;
                break;
            case WaitingKey.BakiakLeft:
                KeybindManager.Instance.bakiakSingleLeft = key;
                break;
            case WaitingKey.BakiakRight:
                KeybindManager.Instance.bakiakSingleRight = key;
                break;
        }
    }

    public void RefreshUI()
    {
        if (KeybindManager.Instance == null)
            return;

        SetText(panjatPinangText, FormatKey(KeybindManager.Instance.panjatPinangSingle));
        SetText(balapKarungText, FormatKey(KeybindManager.Instance.balapKarungSingle));
        SetText(tarikTambangText, FormatKey(KeybindManager.Instance.tarikTambangSingle));
        SetText(makanKerupukText, FormatKey(KeybindManager.Instance.makanKerupukSingle));
        SetText(pakuBotolText, FormatKey(KeybindManager.Instance.pakuBotolSingle));
        SetText(lemparSandalText, FormatKey(KeybindManager.Instance.lemparSandalSingle));
        SetText(kelerengText, FormatKey(KeybindManager.Instance.kelerengSingle));
        SetText(lompatTaliText, FormatKey(KeybindManager.Instance.lompatTaliSingle));
        SetText(egrangLeftText, FormatKey(KeybindManager.Instance.egrangSingleLeft));
        SetText(egrangRightText, FormatKey(KeybindManager.Instance.egrangSingleRight));
        SetText(bakiakLeftText, FormatKey(KeybindManager.Instance.bakiakSingleLeft));
        SetText(bakiakRightText, FormatKey(KeybindManager.Instance.bakiakSingleRight));
    }

    void SetText(TMP_Text target, string value)
    {
        if (target != null)
            target.text = value;
    }

    void StartRebind(WaitingKey targetKey, TMP_Text targetText, string prompt)
    {
        waitingKey = targetKey;
        ignoreCurrentFrameInput = true;
        waitForMouseRelease = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        SetText(targetText, prompt);
    }

    bool IsMouseKey(KeyCode key)
    {
        return key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6;
    }

    bool IsAnyMouseButtonPressed()
    {
        return Input.GetMouseButton(0) ||
               Input.GetMouseButton(1) ||
               Input.GetMouseButton(2) ||
               Input.GetMouseButton(3) ||
               Input.GetMouseButton(4) ||
               Input.GetMouseButton(5) ||
               Input.GetMouseButton(6);
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
