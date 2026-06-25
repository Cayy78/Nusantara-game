using System.Collections;
using TMPro;
using UnityEngine;

public class BalapKarungSingleManager : MonoBehaviour, IGameplayCheatTarget
{
    const string WinResultLabel = "WIN";
    const string LoseResultLabel = "LOSE";
    const string DrawResultLabel = "DRAW";

    [Header("Result Panels")]
    public GameObject resultPanel;
    public GameObject winResultPanel;
    public GameObject loseResultPanel;
    public GameObject drawResultPanel;

    [Header("Players")]
    public PlayerJump player1;
    public PlayerJump player2;
    public Animator anim1;
    public Animator anim2;

    [Header("Power Bars")]
    public PanjatPinangPowerBar bar1;
    public PanjatPinangPowerBar bar2;

    [Header("Versus Result UI")]
    public TMP_Text winnerText;
    public TMP_Text player1TimeText;
    public TMP_Text player2TimeText;
    public string player1Label = "Player 1";
    public string player2Label = "Bot";

    [Header("Gameplay Timer")]
    public TMP_Text timerText;

    [Header("Audio")]
    public AudioSource gameplayMusic;
    public AudioSource sfxSource;
    public AudioClip jumpSfx;
    public AudioClip resultSfx;

    [Header("Fall Animation")]
    [Min(0f)] public float fallCooldown = 0.75f;
    public string fallTriggerName = "Fall";
    public string fallBoolName = "isFalling";

    [Header("Countdown")]
    public GameplayStartCountdown gameplayStartCountdown;

    [Header("Bot")]
    public bool useBotForPlayer2 = true;
    public float minDecisionDelay = 0.2f;
    public float maxDecisionDelay = 0.6f;
    [Range(0f, 1f)] public float mistakeChance = 0.25f;
    [Range(0f, 0.2f)] public float zonePadding = 0.02f;

    [HideInInspector] public bool isGameFinished;

    Coroutine botRoutine;
    float elapsedTime;
    bool player1Finished;
    bool player2Finished;
    float player1FinishTime = -1f;
    float player2FinishTime = -1f;
    bool player1Falling;
    bool player2Falling;
    Coroutine player1FallRoutine;
    Coroutine player2FallRoutine;

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindFirstObjectByType<GameplayStartCountdown>();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        ShowResultPanel(string.Empty);
        UpdateTimerUI();
    }

    void OnEnable()
    {
        RestartBotRoutineIfNeeded();
    }

    void OnDisable()
    {
        StopBotRoutine();
    }

    void Update()
    {
        if (isGameFinished)
            return;

        if (!IsGameplayReady())
            return;

        if (player1Finished && player2Finished)
        {
            FinishMatch();
            return;
        }

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        if (isGameFinished)
            return;

        KeyCode player1Key = GetPlayer1Key();

        if (Input.GetKeyDown(player1Key))
            StartCharge(1);

        if (Input.GetKeyUp(player1Key))
            ReleaseCharge(1);
    }

    public KeyCode GetPlayer1Key()
    {
        if (KeybindManager.Instance != null)
            return KeybindManager.Instance.balapKarungSingle;

        return KeyCode.Space;
    }

    public bool IsPlayerFinished(int playerIndex)
    {
        return playerIndex == 1 ? player1Finished : player2Finished;
    }

    public bool CanBotAct()
    {
        return useBotForPlayer2 && !isGameFinished && !player2Finished && !player2Falling && player2 != null && IsGameplayReady();
    }

    public void StartCharge(int playerIndex)
    {
        if (isGameFinished || IsPlayerFinished(playerIndex) || IsPlayerFalling(playerIndex) || !IsGameplayReady())
            return;

        PanjatPinangPowerBar powerBar = GetBar(playerIndex);
        Animator anim = GetAnimator(playerIndex);

        if (powerBar != null)
            powerBar.StartBar();

        if (anim != null)
            anim.SetBool("isMoving", true);
    }

    public void ReleaseCharge(int playerIndex)
    {
        ReleaseCharge(playerIndex, null);
    }

    public void ReleaseCharge(int playerIndex, float? rawValueOverride)
    {
        if (isGameFinished || IsPlayerFinished(playerIndex) || IsPlayerFalling(playerIndex) || !IsGameplayReady())
            return;

        PlayerJump player = GetPlayer(playerIndex);
        Animator anim = GetAnimator(playerIndex);
        PanjatPinangPowerBar powerBar = GetBar(playerIndex);
        float rawValue = powerBar != null ? (rawValueOverride ?? powerBar.StopBar()) : (rawValueOverride ?? 0.8f);
        PanjatPinangPowerBar.PowerZone resultZone = GetResultZone(playerIndex, rawValue, powerBar);
        float stepValue = GetResultValue(playerIndex, rawValue, powerBar);

        if (powerBar != null)
            powerBar.ResetBar();

        ApplyStepResult(playerIndex, player, anim, stepValue, resultZone);
    }

    public void RegisterPlayerFinish(int playerIndex)
    {
        if (isGameFinished)
            return;

        if (playerIndex == 1 && !player1Finished)
        {
            player1Finished = true;
            player1FinishTime = elapsedTime;
        }
        else if (playerIndex == 2 && !player2Finished)
        {
            player2Finished = true;
            player2FinishTime = elapsedTime;
        }

        if (player1Finished || player2Finished)
            FinishMatch();
    }

    public virtual void StopGameplay()
    {
        if (player1 != null && !player1Finished)
            RegisterPlayerFinish(1);

        if ((player2 == null || !useBotForPlayer2) && !isGameFinished)
            FinishMatch();
    }

    public void CheatPlayer1Win()
    {
        GameplayCheatUtilities.TeleportToFinish(player1 != null ? player1.transform : null, GameplayCheatAxis.Horizontal);
        ForceWinnerResult(1, 1f);
    }

    public void CheatPlayer2Win()
    {
        GameplayCheatUtilities.TeleportToFinish(player2 != null ? player2.transform : null, GameplayCheatAxis.Horizontal);
        ForceWinnerResult(2, 1f);
    }

    public void CheatDraw()
    {
        GameplayCheatUtilities.TeleportToFinish(player1 != null ? player1.transform : null, GameplayCheatAxis.Horizontal);
        GameplayCheatUtilities.TeleportToFinish(player2 != null ? player2.transform : null, GameplayCheatAxis.Horizontal);
        ForceDrawResult(1f);
    }

    void ApplyStepResult(int playerIndex, PlayerJump player, Animator anim, float stepValue, PanjatPinangPowerBar.PowerZone resultZone)
    {
        if (resultZone == PanjatPinangPowerBar.PowerZone.Red)
        {
            TriggerFall(playerIndex, anim);
            return;
        }

        if (player != null)
        {
            if (stepValue > 0f)
                player.JumpForward(stepValue);
            else if (stepValue < 0f)
                player.SlipBackward(Mathf.Abs(stepValue));
        }

        if (anim != null)
            anim.SetBool("isMoving", false);

        if (stepValue > 0f && sfxSource != null && jumpSfx != null)
            sfxSource.PlayOneShot(jumpSfx);
    }

    float GetResultValue(int playerIndex, float rawValue, PanjatPinangPowerBar powerBar)
    {
        if (powerBar != null)
        {
            return powerBar.GetStepAmount(rawValue);
        }

        return MapFallbackStepAmount(rawValue);
    }

    float MapFallbackStepAmount(float rawValue)
    {
        PanjatPinangPowerBar referenceBar = bar2 != null ? bar2 : bar1;

        if (referenceBar == null)
        {
            if (rawValue <= 0.2f)
                return 0f;

            if (rawValue <= 0.5f)
                return 0.3f;

            return 0.5f;
        }

        return referenceBar.GetStepAmount(rawValue);
    }

    PlayerJump GetPlayer(int playerIndex)
    {
        return playerIndex == 1 ? player1 : player2;
    }

    Animator GetAnimator(int playerIndex)
    {
        return playerIndex == 1 ? anim1 : anim2;
    }

    PanjatPinangPowerBar GetBar(int playerIndex)
    {
        return playerIndex == 1 ? bar1 : bar2;
    }

    bool IsPlayerFalling(int playerIndex)
    {
        return playerIndex == 1 ? player1Falling : player2Falling;
    }

    PanjatPinangPowerBar.PowerZone GetResultZone(int playerIndex, float rawValue, PanjatPinangPowerBar powerBar)
    {
        if (powerBar != null)
            return powerBar.GetZone(rawValue);

        PanjatPinangPowerBar referenceBar = GetBar(playerIndex == 1 ? 2 : 1);
        if (referenceBar == null)
            referenceBar = bar1 != null ? bar1 : bar2;

        if (referenceBar != null)
            return referenceBar.GetZone(rawValue);

        if (rawValue <= 0.3f)
            return PanjatPinangPowerBar.PowerZone.Red;

        if (rawValue <= 0.7f)
            return PanjatPinangPowerBar.PowerZone.Yellow;

        return PanjatPinangPowerBar.PowerZone.Green;
    }

    void TriggerFall(int playerIndex, Animator anim)
    {
        if (fallCooldown <= 0f)
            return;

        if (playerIndex == 1)
        {
            if (player1FallRoutine != null)
                StopCoroutine(player1FallRoutine);

            player1FallRoutine = StartCoroutine(PlayFallRoutine(1, anim));
            return;
        }

        if (player2FallRoutine != null)
            StopCoroutine(player2FallRoutine);

        player2FallRoutine = StartCoroutine(PlayFallRoutine(2, anim));
    }

    IEnumerator PlayFallRoutine(int playerIndex, Animator anim)
    {
        SetPlayerFalling(playerIndex, true);

        if (anim != null)
        {
            anim.SetBool("isMoving", false);

            if (!string.IsNullOrEmpty(fallBoolName))
                anim.SetBool(fallBoolName, true);

            if (!string.IsNullOrEmpty(fallTriggerName))
                anim.SetTrigger(fallTriggerName);
        }

        yield return new WaitForSeconds(fallCooldown);

        if (anim != null && !string.IsNullOrEmpty(fallBoolName))
            anim.SetBool(fallBoolName, false);

        SetPlayerFalling(playerIndex, false);

        if (playerIndex == 1)
            player1FallRoutine = null;
        else
            player2FallRoutine = null;
    }

    void SetPlayerFalling(int playerIndex, bool isFallingNow)
    {
        if (playerIndex == 1)
            player1Falling = isFallingNow;
        else
            player2Falling = isFallingNow;
    }

    bool IsGameplayReady()
    {
        if (gameplayStartCountdown == null)
            return true;

        return gameplayStartCountdown.HasCountdownCompleted();
    }

    void FinishMatch()
    {
        if (isGameFinished)
            return;

        isGameFinished = true;
        StopBotRoutine();

        if (anim1 != null)
        {
            anim1.SetBool("isMoving", false);
            if (!string.IsNullOrEmpty(fallBoolName))
                anim1.SetBool(fallBoolName, false);
        }

        if (anim2 != null)
        {
            anim2.SetBool("isMoving", false);
            if (!string.IsNullOrEmpty(fallBoolName))
                anim2.SetBool(fallBoolName, false);
        }

        string resultLabel = GetWinnerLabel();

        if (winnerText != null)
            winnerText.text = resultLabel;

        if (player1TimeText != null)
        {
            player1TimeText.gameObject.SetActive(true);
            player1TimeText.text = player1Label + ": " + FormatFinishTime(player1FinishTime);
        }

        if (player2TimeText != null)
        {
            player2TimeText.gameObject.SetActive(true);
            player2TimeText.text = player2Label + ": " + FormatFinishTime(player2FinishTime);
        }

        if (gameplayMusic != null)
            gameplayMusic.Stop();

        if (sfxSource != null && resultSfx != null)
            sfxSource.PlayOneShot(resultSfx);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        ShowResultPanel(resultLabel);
    }

    void ForceWinnerResult(int winnerPlayerIndex, float winnerFinishTime)
    {
        if (isGameFinished)
            return;

        player1Finished = winnerPlayerIndex == 1;
        player2Finished = winnerPlayerIndex == 2;
        player1FinishTime = winnerPlayerIndex == 1 ? winnerFinishTime : -1f;
        player2FinishTime = winnerPlayerIndex == 2 ? winnerFinishTime : -1f;
        elapsedTime = Mathf.Max(elapsedTime, winnerFinishTime);
        FinishMatch();
    }

    void ForceDrawResult(float sharedFinishTime)
    {
        if (isGameFinished)
            return;

        player1Finished = true;
        player2Finished = true;
        player1FinishTime = sharedFinishTime;
        player2FinishTime = sharedFinishTime;
        elapsedTime = Mathf.Max(elapsedTime, sharedFinishTime);
        FinishMatch();
    }

    string GetWinnerLabel()
    {
        if (player1Finished && !player2Finished)
            return WinResultLabel;

        if (player2Finished && !player1Finished)
            return LoseResultLabel;

        if (player1Finished && player2Finished)
        {
            if (player1FinishTime < player2FinishTime)
                return WinResultLabel;

            if (player2FinishTime < player1FinishTime)
                return LoseResultLabel;

            return DrawResultLabel;
        }

        return string.Empty;
    }

    string FormatFinishTime(float finishTime)
    {
        if (finishTime < 0f)
            return "-";

        return finishTime.ToString("F2") + " s";
    }

    void ShowResultPanel(string resultLabel)
    {
        if (winResultPanel != null)
            winResultPanel.SetActive(resultLabel == WinResultLabel);

        if (loseResultPanel != null)
            loseResultPanel.SetActive(resultLabel == LoseResultLabel);

        if (drawResultPanel != null)
            drawResultPanel.SetActive(resultLabel == DrawResultLabel);
    }

    void RestartBotRoutineIfNeeded()
    {
        StopBotRoutine();

        if (useBotForPlayer2)
            botRoutine = StartCoroutine(BotLoop());
    }

    void StopBotRoutine()
    {
        if (botRoutine == null)
            return;

        StopCoroutine(botRoutine);
        botRoutine = null;
    }

    IEnumerator BotLoop()
    {
        while (enabled)
        {
            if (!CanBotAct())
            {
                yield return null;
                continue;
            }

            float decisionDelay = Random.Range(minDecisionDelay, maxDecisionDelay);
            yield return new WaitForSeconds(decisionDelay);

            if (!CanBotAct())
                continue;

            float rawValue = ChooseBotRawValue();

            StartCharge(2);
            yield return new WaitForSeconds(GetHoldDuration(rawValue));

            if (!CanBotAct())
                continue;

            ReleaseCharge(2, rawValue);
        }
    }

    float ChooseBotRawValue()
    {
        PanjatPinangPowerBar referenceBar = bar2 != null ? bar2 : bar1;
        PanjatPinangPowerBar.PowerZone targetZone = PanjatPinangPowerBar.PowerZone.Green;

        if (Random.value < mistakeChance)
            targetZone = GetMistakeZone(targetZone);

        GetZoneBounds(targetZone, referenceBar, out float minValue, out float maxValue);
        return Random.Range(minValue, maxValue);
    }

    float GetHoldDuration(float rawValue)
    {
        PanjatPinangPowerBar referenceBar = bar2 != null ? bar2 : bar1;
        float speed = referenceBar != null ? Mathf.Max(0.01f, referenceBar.speed) : 1f;
        return rawValue / speed;
    }

    PanjatPinangPowerBar.PowerZone GetMistakeZone(PanjatPinangPowerBar.PowerZone targetZone)
    {
        switch (targetZone)
        {
            case PanjatPinangPowerBar.PowerZone.Green:
                return Random.value < 0.5f ? PanjatPinangPowerBar.PowerZone.Yellow : PanjatPinangPowerBar.PowerZone.Red;
            case PanjatPinangPowerBar.PowerZone.Yellow:
                return Random.value < 0.5f ? PanjatPinangPowerBar.PowerZone.Red : PanjatPinangPowerBar.PowerZone.Green;
            default:
                return Random.value < 0.5f ? PanjatPinangPowerBar.PowerZone.Yellow : PanjatPinangPowerBar.PowerZone.Green;
        }
    }

    void GetZoneBounds(PanjatPinangPowerBar.PowerZone zone, PanjatPinangPowerBar referenceBar, out float minValue, out float maxValue)
    {
        float redMax = referenceBar != null ? Mathf.Clamp01(referenceBar.redMax) : 0.3f;
        float yellowMax = referenceBar != null ? Mathf.Clamp01(referenceBar.yellowMax) : 0.7f;
        float padding = Mathf.Max(0f, zonePadding);

        switch (zone)
        {
            case PanjatPinangPowerBar.PowerZone.Red:
                minValue = padding;
                maxValue = Mathf.Max(minValue, redMax - padding);
                break;

            case PanjatPinangPowerBar.PowerZone.Yellow:
                minValue = Mathf.Clamp01(redMax + padding);
                maxValue = Mathf.Max(minValue, yellowMax - padding);
                break;

            default:
                minValue = Mathf.Clamp01(yellowMax + padding);
                maxValue = Mathf.Max(minValue, 1f - padding);
                break;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = GameplayTimerFormatter.FormatElapsedTime(elapsedTime);
    }
}
