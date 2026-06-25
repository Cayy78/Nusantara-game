using System.Collections;
using TMPro;
using UnityEngine;

public class KelerengSingleManager : MonoBehaviour, IGameplayCheatTarget
{
    const string WinResultLabel = "WIN";
    const string LoseResultLabel = "LOSE";
    const string DrawResultLabel = "DRAW";

    [Header("Spawn Points")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    [Header("Player Colliders")]
    public Collider2D player1Collider;
    public Collider2D player2Collider;

    [Header("Power Bars")]
    public KelerengPowerBar bar1;
    public KelerengPowerBar bar2;

    [Header("Player Animation")]
    public Animator player1Animator;
    public Animator player2Animator;
    public string throwTriggerName = "Throw";

    [Header("Trajectory Lines")]
    public LineRenderer trajectoryLine1;
    public LineRenderer trajectoryLine2;
    public int trajectoryPointCount = 20;
    public float trajectoryTimeStep = 0.1f;

    [Header("Projectile")]
    public GameObject sandalPrefab;
    public float minHorizontalForce = 4f;
    public float maxHorizontalForce = 12f;
    public float minVerticalForce = 3f;
    public float maxVerticalForce = 8f;
    public float projectileGravityScale = 1f;

    [Header("Score")]
    public int targetHitCount = 7;
    public TMP_Text successCountText1;
    public TMP_Text successCountText2;

    [Header("Targets")]
    public LemparSandalTargetMover target1Mover;
    public LemparSandalTargetMover target2Mover;
    public float[] targetPositionsX = { 3f, 4f, 5f, 6f, 7f, 8f, 9f };
    public float targetMoveDelay = 0.35f;

    [Header("Match")]
    public float maxGameTime = 60f;

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
    public AudioClip throwSfx;
    public AudioClip hitSfx;
    public AudioClip resultSfx;

    [Header("Countdown")]
    public GameplayStartCountdown gameplayStartCountdown;

    [Header("Bot")]
    public bool useBotForPlayer2 = true;
    public float minDecisionDelay = 0.2f;
    public float maxDecisionDelay = 0.6f;
    [Range(0f, 1f)] public float mistakeChance = 0.25f;
    [Range(8, 80)] public int botAimSampleCount = 40;
    [Range(0f, 0.5f)] public float botMissOffsetMin = 0.08f;
    [Range(0f, 0.5f)] public float botMissOffsetMax = 0.18f;

    [HideInInspector] public bool isGameFinished;

    int player1HitCount;
    int player2HitCount;
    bool player1ProjectileActive;
    bool player2ProjectileActive;
    int player1TargetIndex;
    int player2TargetIndex;
    float elapsedTime;
    float player1FinishTime = -1f;
    float player2FinishTime = -1f;
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
        UpdateTimerUI();
        ApplyInitialTargetPositions();
        SetTrajectoryVisible(trajectoryLine1, false);
        SetTrajectoryVisible(trajectoryLine2, false);
        hasInitialized = true;

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

        SetTrajectoryVisible(trajectoryLine1, false);
        SetTrajectoryVisible(trajectoryLine2, false);

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
        if (isGameFinished)
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        if (elapsedTime >= maxGameTime)
        {
            elapsedTime = maxGameTime;
            FinishGame(GetTimeoutResultLabel());
            return;
        }

        HandlePlayerInput(GetPlayer1Key(), bar1, spawnPoint1, trajectoryLine1, 1, player1ProjectileActive);
    }

    public bool CanBotAct()
    {
        return useBotForPlayer2 &&
               !isGameFinished &&
               !player2ProjectileActive &&
               player2HitCount < targetHitCount &&
               bar2 != null &&
               spawnPoint2 != null &&
               trajectoryLine2 != null;
    }

    public void CheatPlayer1Win()
    {
        if (isGameFinished)
            return;

        player1HitCount = targetHitCount;
        player1FinishTime = elapsedTime;
        UpdateScoreUI();
        FinishGame(WinResultLabel);
    }

    public void CheatPlayer2Win()
    {
        if (isGameFinished)
            return;

        player2HitCount = targetHitCount;
        player2FinishTime = elapsedTime;
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
        UpdateScoreUI();
        FinishGame(DrawResultLabel);
    }

    public void RegisterHit()
    {
        RegisterHit(1);
    }

    public void RegisterHit(int playerIndex)
    {
        if (isGameFinished)
            return;

        if (playerIndex == 1)
        {
            player1HitCount++;
            MoveTargetToNextStep(target1Mover, ref player1TargetIndex, player1HitCount);
        }
        else if (playerIndex == 2)
        {
            player2HitCount++;
            MoveTargetToNextStep(target2Mover, ref player2TargetIndex, player2HitCount);
        }

        if (sfxSource != null && hitSfx != null)
            sfxSource.PlayOneShot(hitSfx);

        UpdateScoreUI();
        CheckWinner();
    }

    public void NotifyProjectileFinished()
    {
        NotifyProjectileFinished(1);
    }

    public void NotifyProjectileFinished(int playerIndex)
    {
        if (playerIndex == 1)
            player1ProjectileActive = false;
        else if (playerIndex == 2)
            player2ProjectileActive = false;
    }

    void HandlePlayerInput(KeyCode key, KelerengPowerBar bar, Transform spawnPoint, LineRenderer trajectoryLine, int playerIndex, bool projectileActive)
    {
        if (bar == null || spawnPoint == null || trajectoryLine == null)
            return;

        if (projectileActive)
        {
            bar.ResetBar();
            SetTrajectoryVisible(trajectoryLine, false);
            return;
        }

        if (Input.GetKeyDown(key))
        {
            bar.StartBar();
            SetTrajectoryVisible(trajectoryLine, true);
        }

        if (Input.GetKey(key))
            UpdateTrajectoryLine(trajectoryLine, spawnPoint.position, bar.GetCurrentValue());

        if (Input.GetKeyUp(key))
        {
            float rawValue = bar.StopBar();
            bar.ResetBar();
            SetTrajectoryVisible(trajectoryLine, false);
            PlayThrowAnimation(playerIndex);
            ThrowProjectile(playerIndex, spawnPoint.position, rawValue);
        }
    }

    void PlayThrowAnimation(int playerIndex)
    {
        Animator targetAnimator = playerIndex == 1 ? player1Animator : player2Animator;

        if (targetAnimator == null || string.IsNullOrEmpty(throwTriggerName))
            return;

        targetAnimator.SetTrigger(throwTriggerName);
    }

    void ThrowProjectile(int playerIndex, Vector3 spawnPosition, float rawValue)
    {
        if (sandalPrefab == null)
            return;

        GameObject projectileObject = Instantiate(sandalPrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = projectileObject.GetComponent<Rigidbody2D>();
        Collider2D projectileCollider = projectileObject.GetComponent<Collider2D>();
        LemparSandalProjectile projectile = projectileObject.GetComponent<LemparSandalProjectile>();

        if (projectile != null)
        {
            projectile.ownerPlayerIndex = playerIndex;
            projectile.kelerengSingleManager = this;
            projectile.manager = null;
            projectile.kelerengManager = null;
            projectile.singleManager = null;
        }

        IgnoreOwnerCollision(playerIndex, projectileCollider);

        Vector2 velocity = GetLaunchVelocity(rawValue);

        if (rb != null)
        {
            rb.gravityScale = projectileGravityScale;
            rb.velocity = velocity;
        }

        if (playerIndex == 1)
            player1ProjectileActive = true;
        else
            player2ProjectileActive = true;

        if (sfxSource != null && throwSfx != null)
            sfxSource.PlayOneShot(throwSfx);
    }

    Vector2 GetLaunchVelocity(float rawValue)
    {
        float horizontalForce = Mathf.Lerp(minHorizontalForce, maxHorizontalForce, rawValue);
        float verticalForce = Mathf.Lerp(minVerticalForce, maxVerticalForce, rawValue);
        return new Vector2(horizontalForce, verticalForce);
    }

    void UpdateTrajectoryLine(LineRenderer line, Vector3 startPosition, float rawValue)
    {
        if (line == null)
            return;

        line.positionCount = trajectoryPointCount;

        Vector2 launchVelocity = GetLaunchVelocity(rawValue);
        Vector2 gravity = Physics2D.gravity * projectileGravityScale;

        for (int i = 0; i < trajectoryPointCount; i++)
        {
            float time = i * trajectoryTimeStep;
            Vector2 point = (Vector2)startPosition + (launchVelocity * time) + (0.5f * gravity * time * time);
            line.SetPosition(i, point);
        }
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

        SetTrajectoryVisible(trajectoryLine1, false);
        SetTrajectoryVisible(trajectoryLine2, false);

        if (resultLabel == DrawResultLabel)
        {
            if (player1FinishTime < 0f)
                player1FinishTime = elapsedTime;

            if (player2FinishTime < 0f)
                player2FinishTime = elapsedTime;
        }

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

    string GetTimeoutResultLabel()
    {
        if (player1HitCount > player2HitCount)
        {
            if (player1FinishTime < 0f)
                player1FinishTime = elapsedTime;

            return WinResultLabel;
        }

        if (player2HitCount > player1HitCount)
        {
            if (player2FinishTime < 0f)
                player2FinishTime = elapsedTime;

            return LoseResultLabel;
        }

        return DrawResultLabel;
    }

    void UpdateScoreUI()
    {
        if (successCountText1 != null)
            successCountText1.text = player1HitCount + "/" + targetHitCount;

        if (successCountText2 != null)
            successCountText2.text = player2HitCount + "/" + targetHitCount;
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = GameplayTimerFormatter.FormatElapsedTime(elapsedTime);
    }

    void SetTrajectoryVisible(LineRenderer line, bool visible)
    {
        if (line != null)
            line.enabled = visible;
    }

    void IgnoreOwnerCollision(int playerIndex, Collider2D projectileCollider)
    {
        if (projectileCollider == null)
            return;

        Collider2D ownerCollider = playerIndex == 1 ? player1Collider : player2Collider;

        if (ownerCollider != null)
            Physics2D.IgnoreCollision(projectileCollider, ownerCollider, true);
    }

    void ApplyInitialTargetPositions()
    {
        if (targetPositionsX == null || targetPositionsX.Length == 0)
            return;

        if (target1Mover != null)
            target1Mover.SetTargetX(targetPositionsX[0]);

        if (target2Mover != null)
            target2Mover.SetTargetX(targetPositionsX[0]);
    }

    void MoveTargetToNextStep(LemparSandalTargetMover targetMover, ref int currentIndex, int currentHitCount)
    {
        if (targetMover == null || targetPositionsX == null || targetPositionsX.Length == 0)
            return;

        if (currentHitCount >= targetHitCount)
            return;

        currentIndex = Mathf.Min(currentIndex + 1, targetPositionsX.Length - 1);
        targetMover.SetTargetX(targetPositionsX[currentIndex], targetMoveDelay);
    }

    string FormatFinishTime(float finishTime)
    {
        if (finishTime < 0f)
            return "-";

        return finishTime.ToString("F2") + " s";
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

            float decisionDelay = Random.Range(minDecisionDelay, maxDecisionDelay);
            yield return new WaitForSeconds(decisionDelay);

            if (!CanBotAct())
                continue;

            float rawValue = ChooseBotRawValue();
            bar2.StartBar();
            SetTrajectoryVisible(trajectoryLine2, true);
            UpdateTrajectoryLine(trajectoryLine2, spawnPoint2.position, rawValue);
            yield return new WaitForSeconds(GetHoldDuration(rawValue));

            if (!CanBotAct())
                continue;

            float stoppedValue = bar2.StopBar();
            bar2.ResetBar();
            SetTrajectoryVisible(trajectoryLine2, false);
            PlayThrowAnimation(2);
            ThrowProjectile(2, spawnPoint2.position, stoppedValue > 0f ? stoppedValue : rawValue);
        }
    }

    float ChooseBotRawValue()
    {
        float idealValue = FindBestBotRawValue();

        if (Random.value >= mistakeChance)
            return idealValue;

        float missOffset = Random.Range(botMissOffsetMin, botMissOffsetMax);
        if (Random.value < 0.5f)
            missOffset *= -1f;

        return Mathf.Clamp01(idealValue + missOffset);
    }

    float GetHoldDuration(float rawValue)
    {
        KelerengPowerBar referenceBar = bar2 != null ? bar2 : bar1;
        float speed = referenceBar != null ? Mathf.Max(0.01f, referenceBar.speed) : 1f;
        return rawValue / speed;
    }

    float FindBestBotRawValue()
    {
        if (spawnPoint2 == null)
            return 0.5f;

        Vector2 targetPosition = GetCurrentBotTargetPosition();
        int sampleCount = Mathf.Max(8, botAimSampleCount);
        float bestRawValue = 0.5f;
        float bestError = float.MaxValue;

        for (int i = 0; i <= sampleCount; i++)
        {
            float rawValue = i / (float)sampleCount;
            float error = GetShotError(rawValue, spawnPoint2.position, targetPosition);

            if (error < bestError)
            {
                bestError = error;
                bestRawValue = rawValue;
            }
        }

        return bestRawValue;
    }

    Vector2 GetCurrentBotTargetPosition()
    {
        if (target2Mover != null)
            return target2Mover.transform.position;

        if (targetPositionsX != null && targetPositionsX.Length > 0)
        {
            int targetIndex = Mathf.Clamp(player2TargetIndex, 0, targetPositionsX.Length - 1);
            float fallbackX = targetPositionsX[targetIndex];
            float fallbackY = spawnPoint2 != null ? spawnPoint2.position.y : 0f;
            return new Vector2(fallbackX, fallbackY);
        }

        return spawnPoint2 != null ? spawnPoint2.position : Vector2.zero;
    }

    float GetShotError(float rawValue, Vector2 spawnPosition, Vector2 targetPosition)
    {
        Vector2 launchVelocity = GetLaunchVelocity(rawValue);
        if (launchVelocity.x <= 0.001f)
            return float.MaxValue;

        float travelTime = (targetPosition.x - spawnPosition.x) / launchVelocity.x;
        if (travelTime <= 0f)
            return float.MaxValue;

        Vector2 gravity = Physics2D.gravity * projectileGravityScale;
        float projectileY = spawnPosition.y + (launchVelocity.y * travelTime) + (0.5f * gravity.y * travelTime * travelTime);
        return Mathf.Abs(projectileY - targetPosition.y);
    }

    KeyCode GetPlayer1Key()
    {
        return KeybindManager.Instance != null
            ? KeybindManager.Instance.kelerengSingle
            : KeyCode.Space;
    }

    bool ShouldStartGameplayLogic()
    {
        if (gameplayStartCountdown == null)
            return true;

        return gameplayStartCountdown.HasCountdownCompleted();
    }
}
