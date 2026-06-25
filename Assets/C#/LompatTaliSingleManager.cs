using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class LompatTaliSingleManager : MonoBehaviour, IGameplayCheatTarget
{
    const string WinResultLabel = "WIN";
    const string LoseResultLabel = "LOSE";
    const string DrawResultLabel = "DRAW";

    [Header("Players")]
    public LompatTaliPlayerJumper player1;
    public LompatTaliPlayerJumper player2;

    [Header("Result UI")]
    public GameObject resultPanel;
    public GameObject winResultPanel;
    public GameObject loseResultPanel;
    public GameObject drawResultPanel;
    public TMP_Text winnerText;
    public TMP_Text timerText;
    [FormerlySerializedAs("player1TimeText")] public TMP_Text player1TotalHitText;
    [FormerlySerializedAs("player2TimeText")] public TMP_Text player2TotalHitText;
    public string player1Label = "Player 1";
    public string player2Label = "Bot";

    [Header("Gameplay Hit Count UI")]
    public TMP_Text player1HitCountText;
    public TMP_Text player2HitCountText;

    [Header("Audio")]
    public AudioSource gameplayMusic;
    public AudioSource sfxSource;
    public AudioClip jumpSfx;
    public AudioClip hitSfx;
    public AudioClip resultSfx;

    [Header("Hit Effect")]
    public GameObject hitEffectPrefab;
    public GameObject player1HitEffectOverride;
    public GameObject player2HitEffectOverride;
    public Vector3 hitEffectOffset = Vector3.up * 0.5f;
    public float hitEffectLifetime = 0.5f;
    public bool parentHitEffectToPlayer;

    [Header("Countdown")]
    public GameplayStartCountdown gameplayStartCountdown;

    [Header("Match")]
    public float matchDuration = 30f;

    [Header("Bot")]
    public bool useBotForPlayer2 = true;
    public float reactionDelay = 0.02f;
    [Range(0f, 1f)] public float mistakeChance = 0.1f;

    bool isGameFinished;
    bool player1HitThisCycle;
    bool player2HitThisCycle;
    bool ropeDangerPhaseActive;
    int dangerPhaseSequence;
    float elapsedTime;
    int player1HitCount;
    int player2HitCount;
    Coroutine botRoutine;
    bool hasInitialized;
    LompatTaliSpriteTrigger[] ropeSpriteTriggers;

    public bool IsGameFinished => isGameFinished;
    public bool IsRopeDangerPhaseActive => ropeDangerPhaseActive;
    public int DangerPhaseSequence => dangerPhaseSequence;

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindObjectOfType<GameplayStartCountdown>();

        CacheSceneReferences();
        AutoAssignSceneReferences();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        ShowSpecificResultPanels(string.Empty);
        UpdateTimerText();
        UpdateHitCountUI();
        hasInitialized = true;
    }

    void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindObjectOfType<GameplayStartCountdown>();

        CacheSceneReferences();
        AutoAssignSceneReferences();

        if (!hasInitialized)
            return;

        if (!ShouldStartGameplayLogic())
            return;
    }

    void OnDisable()
    {
        StopBotRoutine();
    }

    void Update()
    {
        if (isGameFinished || !ShouldStartGameplayLogic())
            return;

        SyncDangerPhaseFromScene();

        elapsedTime += Time.deltaTime;
        UpdateTimerText();

        if (elapsedTime >= matchDuration)
        {
            FinishMatchByHitCount();
            return;
        }

        if (Input.GetKeyDown(GetPlayer1Key()))
            TryPlayer1Jump();
    }

    public KeyCode GetPlayer1Key()
    {
        if (KeybindManager.Instance != null)
            return KeybindManager.Instance.lompatTaliSingle;

        return KeyCode.Space;
    }

    public bool CanBotAct()
    {
        return useBotForPlayer2 && !isGameFinished && player2 != null;
    }

    public void CheatPlayer1Win()
    {
        if (isGameFinished)
            return;

        player1HitCount = 0;
        player2HitCount = Mathf.Max(player2HitCount, 1);
        UpdateHitCountUI();
        FinishGame(WinResultLabel);
    }

    public void CheatPlayer2Win()
    {
        if (isGameFinished)
            return;

        player1HitCount = Mathf.Max(player1HitCount, 1);
        player2HitCount = 0;
        UpdateHitCountUI();
        FinishGame(LoseResultLabel);
    }

    public void CheatDraw()
    {
        if (isGameFinished)
            return;

        int sharedHitCount = Mathf.Max(Mathf.Max(player1HitCount, player2HitCount), 1);
        player1HitCount = sharedHitCount;
        player2HitCount = sharedHitCount;
        UpdateHitCountUI();
        FinishGame(DrawResultLabel);
    }

    public void TryPlayer1Jump()
    {
        HandleJump(player1);
    }

    public void TryBotJump()
    {
        if (!CanBotAct())
            return;

        HandleJump(player2);
    }

    void HandleJump(LompatTaliPlayerJumper player)
    {
        if (player == null)
            return;

        bool wasJumping = player.IsJumping;
        player.TryJump();

        if (!wasJumping && player.IsJumping && sfxSource != null && jumpSfx != null)
            sfxSource.PlayOneShot(jumpSfx);
    }

    public void RegisterPlayerHit()
    {
        RegisterPlayerHit(1);
    }

    public void RegisterPlayerHit(int playerIndex)
    {
        if (isGameFinished || !ropeDangerPhaseActive)
            return;

        if (playerIndex == 1)
        {
            if (player1HitThisCycle)
                return;

            player1HitThisCycle = true;
            player1HitCount++;
            if (player1 != null)
                player1.OnHit();
            SpawnHitEffect(1);
        }
        else if (playerIndex == 2)
        {
            if (player2HitThisCycle)
                return;

            player2HitThisCycle = true;
            player2HitCount++;
            if (player2 != null)
                player2.OnHit();
            SpawnHitEffect(2);
        }
        else
        {
            return;
        }

        UpdateHitCountUI();

        if (sfxSource != null && hitSfx != null)
            sfxSource.PlayOneShot(hitSfx);
    }

    public void BeginRopeDangerPhase()
    {
        if (isGameFinished || ropeDangerPhaseActive)
            return;

        ropeDangerPhaseActive = true;
        player1HitThisCycle = false;
        player2HitThisCycle = false;
        dangerPhaseSequence++;
        ScheduleBotJumpForDangerPhase();
    }

    public void EndRopeDangerPhase()
    {
        if (isGameFinished || !ropeDangerPhaseActive)
            return;

        ropeDangerPhaseActive = false;
        player1HitThisCycle = false;
        player2HitThisCycle = false;
        StopBotRoutine();
    }

    public void RegisterRopePass(int playerIndex)
    {
        // Legacy no-op.
    }

    void FinishMatchByHitCount()
    {
        if (player1HitCount < player2HitCount)
        {
            FinishGame(WinResultLabel);
            return;
        }

        if (player1HitCount > player2HitCount)
        {
            FinishGame(LoseResultLabel);
            return;
        }

        FinishGame(DrawResultLabel);
    }

    void FinishGame(string result)
    {
        if (isGameFinished)
            return;

        isGameFinished = true;
        ropeDangerPhaseActive = false;
        player1HitThisCycle = false;
        player2HitThisCycle = false;
        StopBotRoutine();

        if (winnerText != null)
            winnerText.text = result;

        if (player1TotalHitText != null)
        {
            player1TotalHitText.gameObject.SetActive(true);
            player1TotalHitText.text = player1Label + " Total Hit: " + player1HitCount;
        }

        if (player2TotalHitText != null)
        {
            player2TotalHitText.gameObject.SetActive(true);
            player2TotalHitText.text = player2Label + " Total Hit: " + player2HitCount;
        }

        if (gameplayMusic != null)
            gameplayMusic.Stop();

        if (sfxSource != null && resultSfx != null)
            sfxSource.PlayOneShot(resultSfx);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        ShowSpecificResultPanels(result);
        UpdateTimerText();
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

    void UpdateTimerText()
    {
        if (timerText == null)
            return;

        float remainingTime = Mathf.Max(0f, matchDuration - elapsedTime);
        timerText.text = remainingTime.ToString("F2");
    }

    void UpdateHitCountUI()
    {
        if (player1HitCountText != null)
            player1HitCountText.text = player1Label + " Hit: " + player1HitCount;

        if (player2HitCountText != null)
            player2HitCountText.text = player2Label + " Hit: " + player2HitCount;
    }

    void StopBotRoutine()
    {
        if (botRoutine == null)
            return;

        StopCoroutine(botRoutine);
        botRoutine = null;
    }

    void ScheduleBotJumpForDangerPhase()
    {
        if (!CanBotAct() || !ShouldStartGameplayLogic())
            return;

        StopBotRoutine();
        botRoutine = StartCoroutine(BotJumpForDangerPhase(dangerPhaseSequence));
    }

    IEnumerator BotJumpForDangerPhase(int phaseSequence)
    {
        if (Random.value < mistakeChance)
        {
            botRoutine = null;
            yield break;
        }

        yield return new WaitForSeconds(GetAdaptiveBotReactionDelay());

        if (!CanBotAct() || !ropeDangerPhaseActive || phaseSequence != dangerPhaseSequence)
        {
            botRoutine = null;
            yield break;
        }

        TryBotJump();
        botRoutine = null;
    }

    bool ShouldStartGameplayLogic()
    {
        if (gameplayStartCountdown == null)
            return true;

        return gameplayStartCountdown.HasCountdownCompleted();
    }

    float GetAdaptiveBotReactionDelay()
    {
        float baseDelay = Mathf.Max(0f, reactionDelay);
        float ropeSpeed = 1f;

        if (ropeSpriteTriggers != null)
        {
            for (int i = 0; i < ropeSpriteTriggers.Length; i++)
            {
                LompatTaliSpriteTrigger trigger = ropeSpriteTriggers[i];
                if (trigger == null)
                    continue;

                ropeSpeed = Mathf.Max(ropeSpeed, Mathf.Max(0.01f, trigger.ropeAnimationSpeed));
            }
        }

        float jumpLeadTime = player2 != null ? player2.jumpDuration * 0.2f : 0f;
        return Mathf.Max(0f, (baseDelay / ropeSpeed) - jumpLeadTime);
    }

    void SpawnHitEffect(int playerIndex)
    {
        Transform target = GetPlayerTransform(playerIndex);
        if (target == null)
            return;

        GameObject effectPrefab = GetHitEffectPrefab(playerIndex);
        if (effectPrefab == null)
            return;

        Transform parent = parentHitEffectToPlayer ? target : null;
        GameObject effectInstance = Instantiate(effectPrefab, target.position + hitEffectOffset, Quaternion.identity, parent);

        if (hitEffectLifetime > 0f)
            Destroy(effectInstance, hitEffectLifetime);
    }

    GameObject GetHitEffectPrefab(int playerIndex)
    {
        if (playerIndex == 1 && player1HitEffectOverride != null)
            return player1HitEffectOverride;

        if (playerIndex == 2 && player2HitEffectOverride != null)
            return player2HitEffectOverride;

        return hitEffectPrefab;
    }

    Transform GetPlayerTransform(int playerIndex)
    {
        if (playerIndex == 1)
            return player1 != null ? player1.transform : null;

        if (playerIndex == 2)
            return player2 != null ? player2.transform : null;

        return null;
    }

    void CacheSceneReferences()
    {
        ropeSpriteTriggers = FindObjectsOfType<LompatTaliSpriteTrigger>(true);
    }

    void AutoAssignSceneReferences()
    {
        if (ropeSpriteTriggers == null || ropeSpriteTriggers.Length == 0)
            CacheSceneReferences();

        for (int i = 0; i < ropeSpriteTriggers.Length; i++)
        {
            if (ropeSpriteTriggers[i] == null)
                continue;

            ropeSpriteTriggers[i].singleManager = this;
            ropeSpriteTriggers[i].manager = null;
        }

        LompatTaliPlayerHitDetector[] hitDetectors = FindObjectsOfType<LompatTaliPlayerHitDetector>(true);
        for (int i = 0; i < hitDetectors.Length; i++)
        {
            if (hitDetectors[i] == null)
                continue;

            hitDetectors[i].singleManager = this;
            hitDetectors[i].manager = null;
        }
    }

    void SyncDangerPhaseFromScene()
    {
        if (ropeSpriteTriggers == null || ropeSpriteTriggers.Length == 0)
            return;

        bool anyDangerPhaseActive = false;

        for (int i = 0; i < ropeSpriteTriggers.Length; i++)
        {
            LompatTaliSpriteTrigger trigger = ropeSpriteTriggers[i];
            if (trigger == null || trigger.ropeCollider == null)
                continue;

            if (trigger.ropeCollider.enabled)
            {
                anyDangerPhaseActive = true;
                break;
            }
        }

        if (anyDangerPhaseActive)
        {
            BeginRopeDangerPhase();
            return;
        }

        EndRopeDangerPhase();
    }
}
