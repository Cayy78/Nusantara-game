using System.Collections;
using TMPro;
using UnityEngine;

public class PanjatPinangSingleManager : MonoBehaviour, IGameplayCheatTarget
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
    public PlayerClimb player1;
    public PlayerClimb player2;

    [Header("Animation")]
    public Animator anim1;
    public Animator anim2;

    [Header("Power Bars")]
    public PanjatPinangPowerBar panjatPinangBar1;
    public PanjatPinangPowerBar panjatPinangBar2;

    [Header("Test Movement")]
    public bool useTapTestMovement = false;
    public bool isGameFinished;
    public float tapClimbAmount = 0.7f;
    public float tapAnimationDuration = 0.15f;

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

    [Header("Countdown")]
    public GameplayStartCountdown gameplayStartCountdown;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip climbSfx;
    public AudioClip resultSfx;

    [Header("Bot")]
    public bool useBotForPlayer2 = true;

    private Coroutine player1AnimRoutine;
    private Coroutine player2AnimRoutine;
    private float elapsedTime;
    private bool player1Finished;
    private bool player2Finished;
    private float player1FinishTime = -1f;
    private float player2FinishTime = -1f;

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindFirstObjectByType<GameplayStartCountdown>();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (winResultPanel != null)
            winResultPanel.SetActive(false);

        if (loseResultPanel != null)
            loseResultPanel.SetActive(false);

        if (drawResultPanel != null)
            drawResultPanel.SetActive(false);

        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameFinished)
            return;

        if (!IsGameplayReady())
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        if (isGameFinished)
            return;

        if (useTapTestMovement)
        {
            HandleTapTestInput(player1, anim1, GetPlayer1Key(), ref player1AnimRoutine);
            return;
        }

        KeyCode player1Key = GetPlayer1Key();

        if (Input.GetKeyDown(player1Key))
            StartCharge(1);

        if (Input.GetKeyUp(player1Key))
            ReleaseCharge(1);
    }

    public KeyCode GetPlayer1Key()
    {
        if (KeybindManager.Instance != null)
            return KeybindManager.Instance.panjatPinangSingle;

        return KeyCode.Space;
    }

    public bool IsPlayerFinished(int playerIndex)
    {
        return playerIndex == 1 ? player1Finished : player2Finished;
    }

    public bool CanBotAct()
    {
        return useBotForPlayer2 && !isGameFinished && !player2Finished && player2 != null && IsGameplayReady();
    }

    public void StartCharge(int playerIndex)
    {
        if (isGameFinished || IsPlayerFinished(playerIndex) || !IsGameplayReady())
            return;

        PanjatPinangPowerBar panjatPinangBar = GetPanjatPinangBar(playerIndex);
        Animator anim = GetAnimator(playerIndex);

        if (panjatPinangBar != null)
            panjatPinangBar.StartBar();

        if (anim != null)
            anim.SetBool("isClimbing", true);
    }

    public void ReleaseCharge(int playerIndex)
    {
        ReleaseCharge(playerIndex, null);
    }

    public void ReleaseCharge(int playerIndex, float? rawValueOverride)
    {
        if (isGameFinished || IsPlayerFinished(playerIndex) || !IsGameplayReady())
            return;

        PlayerClimb player = GetPlayer(playerIndex);
        Animator anim = GetAnimator(playerIndex);
        float stepValue = GetResultValue(playerIndex, rawValueOverride);

        ApplyStepResult(player, anim, stepValue);
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
        GameplayCheatUtilities.TeleportToFinish(player1 != null ? player1.transform : null, GameplayCheatAxis.Vertical);
        ForceWinnerResult(1, 1f);
    }

    public void CheatPlayer2Win()
    {
        GameplayCheatUtilities.TeleportToFinish(player2 != null ? player2.transform : null, GameplayCheatAxis.Vertical);
        ForceWinnerResult(2, 1f);
    }

    public void CheatDraw()
    {
        GameplayCheatUtilities.TeleportToFinish(player1 != null ? player1.transform : null, GameplayCheatAxis.Vertical);
        GameplayCheatUtilities.TeleportToFinish(player2 != null ? player2.transform : null, GameplayCheatAxis.Vertical);
        ForceDrawResult(1f);
    }

    void ApplyStepResult(PlayerClimb player, Animator anim, float stepValue)
    {
        if (player != null)
        {
            if (stepValue > 0f)
                player.Climb(stepValue);
            else if (stepValue < 0f)
                player.Slip(Mathf.Abs(stepValue));
        }

        if (anim != null)
            anim.SetBool("isClimbing", false);

        if (stepValue > 0f && sfxSource != null && climbSfx != null)
            sfxSource.PlayOneShot(climbSfx);
    }

    void HandleTapTestInput(PlayerClimb player, Animator anim, KeyCode key, ref Coroutine animationRoutine)
    {
        if (player == null || !Input.GetKeyDown(key))
            return;

        player.Climb(tapClimbAmount);

        if (anim == null)
            return;

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(PlayTapClimbAnimation(anim));
    }

    IEnumerator PlayTapClimbAnimation(Animator anim)
    {
        anim.SetBool("isClimbing", true);
        yield return new WaitForSeconds(tapAnimationDuration);
        anim.SetBool("isClimbing", false);
    }

    float GetResultValue(int playerIndex, float? rawValueOverride)
    {
        PanjatPinangPowerBar panjatPinangBar = GetPanjatPinangBar(playerIndex);

        if (panjatPinangBar != null)
        {
            float rawValue = rawValueOverride ?? panjatPinangBar.StopBar();
            panjatPinangBar.ResetBar();
            return panjatPinangBar.GetStepAmount(rawValue);
        }

        float fallbackRawValue = rawValueOverride ?? 0.8f;
        return MapFallbackStepAmount(fallbackRawValue);
    }

    float MapFallbackStepAmount(float rawValue)
    {
        PanjatPinangPowerBar referenceBar = panjatPinangBar2 != null ? panjatPinangBar2 : panjatPinangBar1;

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

    PlayerClimb GetPlayer(int playerIndex)
    {
        return playerIndex == 1 ? player1 : player2;
    }

    Animator GetAnimator(int playerIndex)
    {
        return playerIndex == 1 ? anim1 : anim2;
    }

    PanjatPinangPowerBar GetPanjatPinangBar(int playerIndex)
    {
        return playerIndex == 1 ? panjatPinangBar1 : panjatPinangBar2;
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

        if (anim1 != null)
            anim1.SetBool("isClimbing", false);

        if (anim2 != null)
            anim2.SetBool("isClimbing", false);

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

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = GameplayTimerFormatter.FormatElapsedTime(elapsedTime);
    }
}
