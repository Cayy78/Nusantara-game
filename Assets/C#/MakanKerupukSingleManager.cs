using System.Collections;
using TMPro;
using UnityEngine;

public class MakanKerupukSingleManager : MonoBehaviour, IGameplayCheatTarget
{
    const string WinResultLabel = "WIN";
    const string LoseResultLabel = "LOSE";
    const string DrawResultLabel = "DRAW";

    [Header("Alignment Triggers")]
    public Collider2D player1TriggerLine;
    public Collider2D player2TriggerLine;
    public Collider2D kerupukRopeCollider1;
    public Collider2D kerupukRopeCollider2;
    public float alignmentToleranceX = 0.2f;

    [Header("Kerupuk Visuals")]
    public SpriteRenderer kerupukRenderer1;
    public SpriteRenderer kerupukRenderer2;
    public MakanKerupukSwing kerupukSwing1;
    public MakanKerupukSwing kerupukSwing2;
    public Sprite[] biteSprites;
    public bool hideKerupukWhenFinished = true;

    [Header("Score")]
    public int targetHitCount = 7;
    public TMP_Text successCountText1;
    public TMP_Text successCountText2;

    [Header("Bite Timing")]
    [Min(0f)] public float chewDelayAfterSuccessfulBite = 0.35f;

    [Header("Miss Timing")]
    [Min(0f)] public float missPauseDelayAfterFailedBite = 0.2f;

    [Header("Character Animation")]
    public Animator player1Animator;
    public Animator player2Animator;
    public string successfulBiteTriggerName = "Bite";

    [Header("Result Panels")]
    public GameObject resultPanel;
    public GameObject winResultPanel;
    public GameObject loseResultPanel;
    public GameObject drawResultPanel;

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
    public AudioSource biteSfxSource;
    public AudioClip biteSfx;
    [Min(0f)] public float biteSfxStopDelay = 0f;
    public AudioClip resultSfx;

    [Header("Countdown")]
    public GameplayStartCountdown gameplayStartCountdown;

    [Header("Bot")]
    public bool useBotForPlayer2 = true;
    public float minDecisionDelay = 0.08f;
    public float maxDecisionDelay = 0.2f;
    [Range(0f, 1f)] public float mistakeChance = 0.2f;

    [HideInInspector] public bool isGameFinished;

    int player1HitCount;
    int player2HitCount;
    bool player1MustLeaveZone;
    bool player2MustLeaveZone;
    float elapsedTime;
    float player1FinishTime = -1f;
    float player2FinishTime = -1f;
    float player1NextAllowedBiteTime;
    float player2NextAllowedBiteTime;
    Coroutine biteSfxStopRoutine;
    Coroutine botRoutine;
    bool hasInitialized;

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindObjectOfType<GameplayStartCountdown>();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        ShowSpecificResultPanels(string.Empty);
        UpdateScoreUI();
        ApplyBiteVisual(kerupukRenderer1, player1HitCount);
        ApplyBiteVisual(kerupukRenderer2, player2HitCount);
        hasInitialized = true;
        UpdateTimerUI();

        if (ShouldStartGameplayLogic())
            RestartBotRoutineIfNeeded();
    }

    void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindObjectOfType<GameplayStartCountdown>();

        if (!hasInitialized)
            return;

        if (!ShouldStartGameplayLogic())
            return;

        RestartBotRoutineIfNeeded();
    }

    void OnDisable()
    {
        StopBotRoutine();
    }

    void Update()
    {
        if (isGameFinished || !ShouldStartGameplayLogic())
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        RefreshAlignmentLocks();

        KeybindManager keybindManager = KeybindManager.Instance;
        if (keybindManager == null)
            return;

        if (Input.GetKeyDown(keybindManager.makanKerupukSingle))
            TryBitePlayer1();
    }

    public bool CanBotAct()
    {
        return useBotForPlayer2 &&
               !isGameFinished &&
               player2HitCount < targetHitCount &&
               player2TriggerLine != null &&
                kerupukRopeCollider2 != null;
    }

    public void CheatPlayer1Win()
    {
        if (isGameFinished)
            return;

        player1HitCount = targetHitCount;
        player1FinishTime = elapsedTime;
        ApplyBiteVisual(kerupukRenderer1, player1HitCount);
        UpdateScoreUI();
        FinishGame(WinResultLabel);
    }

    public void CheatPlayer2Win()
    {
        if (isGameFinished)
            return;

        player2HitCount = targetHitCount;
        player2FinishTime = elapsedTime;
        ApplyBiteVisual(kerupukRenderer2, player2HitCount);
        UpdateScoreUI();
        FinishGame(LoseResultLabel);
    }

    public void CheatDraw()
    {
        if (isGameFinished)
            return;

        player1HitCount = targetHitCount;
        player2HitCount = targetHitCount;
        player1FinishTime = elapsedTime;
        player2FinishTime = elapsedTime;
        ApplyBiteVisual(kerupukRenderer1, player1HitCount);
        ApplyBiteVisual(kerupukRenderer2, player2HitCount);
        UpdateScoreUI();
        FinishGame(DrawResultLabel);
    }

    void TryBitePlayer1()
    {
        if (player1MustLeaveZone)
            return;

        if (elapsedTime < player1NextAllowedBiteTime)
            return;

        if (!IsAligned(player1TriggerLine, kerupukRopeCollider1))
        {
            RegisterFailedBite(1);
            return;
        }

        RegisterSuccessfulBite(1);
    }

    void TryBitePlayer2()
    {
        if (player2MustLeaveZone)
            return;

        if (elapsedTime < player2NextAllowedBiteTime)
            return;

        if (!IsAligned(player2TriggerLine, kerupukRopeCollider2))
        {
            RegisterFailedBite(2);
            return;
        }

        RegisterSuccessfulBite(2);
    }

    void RegisterSuccessfulBite(int playerIndex)
    {
        if (playerIndex == 1)
        {
            player1HitCount++;
            player1MustLeaveZone = true;
            player1NextAllowedBiteTime = elapsedTime + chewDelayAfterSuccessfulBite;
            ApplyBiteVisual(kerupukRenderer1, player1HitCount);
            PauseSwing(kerupukSwing1);
        }
        else
        {
            player2HitCount++;
            player2MustLeaveZone = true;
            player2NextAllowedBiteTime = elapsedTime + chewDelayAfterSuccessfulBite;
            ApplyBiteVisual(kerupukRenderer2, player2HitCount);
            PauseSwing(kerupukSwing2);
        }

        PlaySuccessfulBiteAnimation(playerIndex);
        HandleSuccessfulBite();
        CheckWinner();
    }

    void RegisterFailedBite(int playerIndex)
    {
        if (missPauseDelayAfterFailedBite <= 0f)
            return;

        if (playerIndex == 1)
        {
            player1NextAllowedBiteTime = Mathf.Max(player1NextAllowedBiteTime, elapsedTime + missPauseDelayAfterFailedBite);
            PauseSwing(kerupukSwing1, missPauseDelayAfterFailedBite);
        }
        else
        {
            player2NextAllowedBiteTime = Mathf.Max(player2NextAllowedBiteTime, elapsedTime + missPauseDelayAfterFailedBite);
            PauseSwing(kerupukSwing2, missPauseDelayAfterFailedBite);
        }
    }

    void HandleSuccessfulBite()
    {
        UpdateScoreUI();

        if (biteSfxSource != null && biteSfx != null)
        {
            biteSfxSource.PlayOneShot(biteSfx);
            ScheduleBiteSfxStop();
        }
    }

    void RefreshAlignmentLocks()
    {
        if (player1MustLeaveZone && !IsAligned(player1TriggerLine, kerupukRopeCollider1))
            player1MustLeaveZone = false;

        if (player2MustLeaveZone && !IsAligned(player2TriggerLine, kerupukRopeCollider2))
            player2MustLeaveZone = false;
    }

    bool IsAligned(Collider2D playerTrigger, Collider2D ropeCollider)
    {
        if (playerTrigger == null || ropeCollider == null)
            return false;

        float playerX = playerTrigger.bounds.center.x;
        float ropeX = ropeCollider.bounds.center.x;
        return Mathf.Abs(playerX - ropeX) <= alignmentToleranceX;
    }

    void PauseSwing(MakanKerupukSwing targetSwing)
    {
        PauseSwing(targetSwing, chewDelayAfterSuccessfulBite);
    }

    void PauseSwing(MakanKerupukSwing targetSwing, float pauseDuration)
    {
        if (targetSwing != null)
            targetSwing.PauseSwing(pauseDuration);
    }

    void ScheduleBiteSfxStop()
    {
        if (biteSfxStopDelay <= 0f)
            return;

        if (biteSfxStopRoutine != null)
            StopCoroutine(biteSfxStopRoutine);

        biteSfxStopRoutine = StartCoroutine(StopBiteSfxAfterDelay());
    }

    IEnumerator StopBiteSfxAfterDelay()
    {
        yield return new WaitForSeconds(biteSfxStopDelay);

        if (biteSfxSource != null)
            biteSfxSource.Stop();

        biteSfxStopRoutine = null;
    }

    void ApplyBiteVisual(SpriteRenderer targetRenderer, int hitCount)
    {
        if (targetRenderer == null)
            return;

        if (biteSprites != null && biteSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(hitCount, 0, biteSprites.Length - 1);
            targetRenderer.sprite = biteSprites[spriteIndex];
        }

        if (hideKerupukWhenFinished && hitCount >= targetHitCount)
            targetRenderer.enabled = false;
    }

    void UpdateScoreUI()
    {
        if (successCountText1 != null)
            successCountText1.text = player1HitCount + "/" + targetHitCount;

        if (successCountText2 != null)
            successCountText2.text = player2HitCount + "/" + targetHitCount;
    }

    void CheckWinner()
    {
        bool player1ReachedTarget = player1HitCount >= targetHitCount;
        bool player2ReachedTarget = player2HitCount >= targetHitCount;

        if (!player1ReachedTarget && !player2ReachedTarget)
            return;

        if (player1ReachedTarget && player1FinishTime < 0f)
            player1FinishTime = elapsedTime;

        if (player2ReachedTarget && player2FinishTime < 0f)
            player2FinishTime = elapsedTime;

        if (player1ReachedTarget && player2ReachedTarget)
        {
            FinishGame(DrawResultLabel);
            return;
        }

        if (player1ReachedTarget)
        {
            FinishGame(WinResultLabel);
            return;
        }

        FinishGame(LoseResultLabel);
    }

    void FinishGame(string resultLabel)
    {
        if (isGameFinished)
            return;

        isGameFinished = true;
        StopBotRoutine();

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

        ShowSpecificResultPanels(resultLabel);
    }

    string FormatFinishTime(float finishTime)
    {
        if (finishTime < 0f)
            return "-";

        return finishTime.ToString("F2") + " s";
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = GameplayTimerFormatter.FormatElapsedTime(elapsedTime);
    }

    void ShowSpecificResultPanels(string resultLabel)
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

            if (player2MustLeaveZone || elapsedTime < player2NextAllowedBiteTime || !IsAligned(player2TriggerLine, kerupukRopeCollider2))
            {
                yield return null;
                continue;
            }

            float decisionDelay = Random.Range(minDecisionDelay, maxDecisionDelay);
            yield return new WaitForSeconds(decisionDelay);

            if (!CanBotAct())
                continue;

            if (player2MustLeaveZone || elapsedTime < player2NextAllowedBiteTime || !IsAligned(player2TriggerLine, kerupukRopeCollider2))
                continue;

            if (Random.value < mistakeChance)
            {
                RegisterFailedBite(2);
                continue;
            }

            TryBitePlayer2();
        }
    }

    bool ShouldStartGameplayLogic()
    {
        if (gameplayStartCountdown == null)
            return true;

        return gameplayStartCountdown.HasCountdownCompleted();
    }

    void PlaySuccessfulBiteAnimation(int playerIndex)
    {
        if (string.IsNullOrEmpty(successfulBiteTriggerName))
            return;

        Animator targetAnimator = playerIndex == 1 ? player1Animator : player2Animator;
        if (targetAnimator == null)
            return;

        targetAnimator.SetTrigger(successfulBiteTriggerName);
    }
}
