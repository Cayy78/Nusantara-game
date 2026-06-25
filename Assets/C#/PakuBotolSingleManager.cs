using System.Collections;
using TMPro;
using UnityEngine;

public class PakuBotolSingleManager : MonoBehaviour, IGameplayCheatTarget
{
    const string WinResultLabel = "WIN";
    const string LoseResultLabel = "LOSE";
    const string DrawResultLabel = "DRAW";

    [Header("Alignment Triggers")]
    public Collider2D bottleTriggerLine1;
    public Collider2D bottleTriggerLine2;
    public Collider2D playerSwingCollider1;
    public Collider2D playerSwingCollider2;
    public float alignmentToleranceX = 0.2f;

    [Header("Bottle Progress Visuals")]
    public SpriteRenderer bottleRenderer1;
    public SpriteRenderer bottleRenderer2;
    public MakanKerupukSwing bottleSwing1;
    public MakanKerupukSwing bottleSwing2;
    public Sprite[] progressSprites;
    public bool hideBottleWhenFinished = false;

    [Header("Score")]
    public int targetHitCount = 7;
    public TMP_Text successCountText1;
    public TMP_Text successCountText2;

    [Header("Hit Timing")]
    [Min(0f)] public float nailDropDelayAfterSuccessfulHit = 0.35f;

    [Header("Miss Timing")]
    [Min(0f)] public float missPauseDelayAfterFailedHit = 0.2f;

    [Header("Character Animation")]
    public Animator player1Animator;
    public Animator player2Animator;
    public string successfulHitTriggerName = "Hit";

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
    public AudioClip hitSfx;
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
    float player1NextAllowedHitTime;
    float player2NextAllowedHitTime;
    Coroutine botRoutine;
    bool hasInitialized;

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindObjectOfType<GameplayStartCountdown>();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        ResolveSwingReferences();
        ShowSpecificResultPanels(string.Empty);
        UpdateScoreUI();
        ApplyProgressVisual(bottleRenderer1, player1HitCount);
        ApplyProgressVisual(bottleRenderer2, player2HitCount);
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

        if (Input.GetKeyDown(keybindManager.pakuBotolSingle))
            TryHitPlayer1();
    }

    public bool CanBotAct()
    {
        return useBotForPlayer2 &&
               !isGameFinished &&
               player2HitCount < targetHitCount &&
               bottleTriggerLine2 != null &&
                playerSwingCollider2 != null;
    }

    public void CheatPlayer1Win()
    {
        if (isGameFinished)
            return;

        player1HitCount = targetHitCount;
        player1FinishTime = elapsedTime;
        ApplyProgressVisual(bottleRenderer1, player1HitCount);
        UpdateScoreUI();
        FinishGame(WinResultLabel);
    }

    public void CheatPlayer2Win()
    {
        if (isGameFinished)
            return;

        player2HitCount = targetHitCount;
        player2FinishTime = elapsedTime;
        ApplyProgressVisual(bottleRenderer2, player2HitCount);
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
        ApplyProgressVisual(bottleRenderer1, player1HitCount);
        ApplyProgressVisual(bottleRenderer2, player2HitCount);
        UpdateScoreUI();
        FinishGame(DrawResultLabel);
    }

    void TryHitPlayer1()
    {
        if (player1MustLeaveZone)
            return;

        if (elapsedTime < player1NextAllowedHitTime)
            return;

        if (!IsAligned(bottleTriggerLine1, playerSwingCollider1))
        {
            RegisterFailedHit(1);
            return;
        }

        RegisterSuccessfulHit(1);
    }

    void TryHitPlayer2()
    {
        if (player2MustLeaveZone)
            return;

        if (elapsedTime < player2NextAllowedHitTime)
            return;

        if (!IsAligned(bottleTriggerLine2, playerSwingCollider2))
        {
            RegisterFailedHit(2);
            return;
        }

        RegisterSuccessfulHit(2);
    }

    void RegisterSuccessfulHit(int playerIndex)
    {
        if (playerIndex == 1)
        {
            player1HitCount++;
            player1MustLeaveZone = true;
            player1NextAllowedHitTime = elapsedTime + nailDropDelayAfterSuccessfulHit;
            ApplyProgressVisual(bottleRenderer1, player1HitCount);
            PauseBottleSwing(bottleSwing1, bottleTriggerLine1, playerSwingCollider1);
        }
        else
        {
            player2HitCount++;
            player2MustLeaveZone = true;
            player2NextAllowedHitTime = elapsedTime + nailDropDelayAfterSuccessfulHit;
            ApplyProgressVisual(bottleRenderer2, player2HitCount);
            PauseBottleSwing(bottleSwing2, bottleTriggerLine2, playerSwingCollider2);
        }

        PlaySuccessfulHitAnimation(playerIndex);
        HandleSuccessfulHit();
        CheckWinner();
    }

    void RegisterFailedHit(int playerIndex)
    {
        if (missPauseDelayAfterFailedHit <= 0f)
            return;

        if (playerIndex == 1)
        {
            player1NextAllowedHitTime = Mathf.Max(player1NextAllowedHitTime, elapsedTime + missPauseDelayAfterFailedHit);
            PauseBottleSwing(bottleSwing1, bottleTriggerLine1, playerSwingCollider1, missPauseDelayAfterFailedHit);
        }
        else
        {
            player2NextAllowedHitTime = Mathf.Max(player2NextAllowedHitTime, elapsedTime + missPauseDelayAfterFailedHit);
            PauseBottleSwing(bottleSwing2, bottleTriggerLine2, playerSwingCollider2, missPauseDelayAfterFailedHit);
        }
    }

    void HandleSuccessfulHit()
    {
        UpdateScoreUI();

        if (sfxSource != null && hitSfx != null)
            sfxSource.PlayOneShot(hitSfx);
    }

    void RefreshAlignmentLocks()
    {
        if (player1MustLeaveZone && !IsAligned(bottleTriggerLine1, playerSwingCollider1))
            player1MustLeaveZone = false;

        if (player2MustLeaveZone && !IsAligned(bottleTriggerLine2, playerSwingCollider2))
            player2MustLeaveZone = false;
    }

    bool IsAligned(Collider2D bottleTrigger, Collider2D playerSwingCollider)
    {
        if (bottleTrigger == null || playerSwingCollider == null)
            return false;

        float bottleX = bottleTrigger.bounds.center.x;
        float swingX = playerSwingCollider.bounds.center.x;
        return Mathf.Abs(bottleX - swingX) <= alignmentToleranceX;
    }

    void ResolveSwingReferences()
    {
        if (bottleSwing1 == null)
            bottleSwing1 = FindSwingReference(bottleTriggerLine1, playerSwingCollider1);

        if (bottleSwing2 == null)
            bottleSwing2 = FindSwingReference(bottleTriggerLine2, playerSwingCollider2);
    }

    MakanKerupukSwing FindSwingReference(Collider2D bottleTrigger, Collider2D playerSwingCollider)
    {
        if (bottleTrigger != null)
        {
            MakanKerupukSwing swingFromBottle = bottleTrigger.GetComponentInParent<MakanKerupukSwing>();
            if (swingFromBottle != null)
                return swingFromBottle;
        }

        if (playerSwingCollider != null)
            return playerSwingCollider.GetComponentInParent<MakanKerupukSwing>();

        return null;
    }

    void PauseBottleSwing(MakanKerupukSwing targetSwing, Collider2D bottleTrigger, Collider2D playerSwingCollider)
    {
        PauseBottleSwing(targetSwing, bottleTrigger, playerSwingCollider, nailDropDelayAfterSuccessfulHit);
    }

    void PauseBottleSwing(MakanKerupukSwing targetSwing, Collider2D bottleTrigger, Collider2D playerSwingCollider, float pauseDuration)
    {
        if (targetSwing == null)
            targetSwing = FindSwingReference(bottleTrigger, playerSwingCollider);

        if (targetSwing != null)
            targetSwing.PauseSwing(pauseDuration);
    }

    void ApplyProgressVisual(SpriteRenderer targetRenderer, int hitCount)
    {
        if (targetRenderer == null)
            return;

        if (progressSprites != null && progressSprites.Length > 0)
        {
            int spriteIndex = Mathf.Clamp(hitCount, 0, progressSprites.Length - 1);
            targetRenderer.sprite = progressSprites[spriteIndex];
        }

        if (hideBottleWhenFinished && hitCount >= targetHitCount)
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

            if (player2MustLeaveZone || elapsedTime < player2NextAllowedHitTime || !IsAligned(bottleTriggerLine2, playerSwingCollider2))
            {
                yield return null;
                continue;
            }

            float decisionDelay = Random.Range(minDecisionDelay, maxDecisionDelay);
            yield return new WaitForSeconds(decisionDelay);

            if (!CanBotAct())
                continue;

            if (player2MustLeaveZone || elapsedTime < player2NextAllowedHitTime || !IsAligned(bottleTriggerLine2, playerSwingCollider2))
                continue;

            if (Random.value < mistakeChance)
            {
                RegisterFailedHit(2);
                continue;
            }

            TryHitPlayer2();
        }
    }

    bool ShouldStartGameplayLogic()
    {
        if (gameplayStartCountdown == null)
            return true;

        return gameplayStartCountdown.HasCountdownCompleted();
    }

    void PlaySuccessfulHitAnimation(int playerIndex)
    {
        if (string.IsNullOrEmpty(successfulHitTriggerName))
            return;

        Animator targetAnimator = playerIndex == 1 ? player1Animator : player2Animator;
        if (targetAnimator == null)
            return;

        targetAnimator.SetTrigger(successfulHitTriggerName);
    }
}
