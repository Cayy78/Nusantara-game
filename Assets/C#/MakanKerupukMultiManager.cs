using TMPro;
using UnityEngine;
using System.Collections;

public class MakanKerupukMultiManager : MonoBehaviour, IGameplayCheatTarget
{
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

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text winnerText;
    public TMP_Text player1TimeText;
    public TMP_Text player2TimeText;

    [Header("Gameplay Timer")]
    public TMP_Text timerText;

    [Header("Audio")]
    public AudioSource gameplayMusic;
    public AudioSource sfxSource;
    public AudioSource biteSfxSource;
    public AudioClip biteSfx;
    [Min(0f)] public float biteSfxStopDelay = 0f;
    public AudioClip resultSfx;

    private int player1HitCount;
    private int player2HitCount;
    private bool isGameFinished;
    private bool player1MustLeaveZone;
    private bool player2MustLeaveZone;
    private float elapsedTime;
    private float player1FinishTime = -1f;
    private float player2FinishTime = -1f;
    private float player1NextAllowedBiteTime;
    private float player2NextAllowedBiteTime;
    private Coroutine biteSfxStopRoutine;

    void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        UpdateScoreUI();
        ApplyBiteVisual(kerupukRenderer1, player1HitCount);
        ApplyBiteVisual(kerupukRenderer2, player2HitCount);
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameFinished || KeybindManager.Instance == null)
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        RefreshAlignmentLocks();

        if (Input.GetKeyDown(KeybindManager.Instance.makanKerupukPlayer1))
            TryBitePlayer1();

        if (Input.GetKeyDown(KeybindManager.Instance.makanKerupukPlayer2))
            TryBitePlayer2();
    }

    public void CheatPlayer1Win()
    {
        if (isGameFinished)
            return;

        player1HitCount = targetHitCount;
        player1FinishTime = elapsedTime;
        ApplyBiteVisual(kerupukRenderer1, player1HitCount);
        UpdateScoreUI();
        FinishGame("Player 1");
    }

    public void CheatPlayer2Win()
    {
        if (isGameFinished)
            return;

        player2HitCount = targetHitCount;
        player2FinishTime = elapsedTime;
        ApplyBiteVisual(kerupukRenderer2, player2HitCount);
        UpdateScoreUI();
        FinishGame("Player 2");
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
        FinishGame("Draw");
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

        player1HitCount++;
        player1MustLeaveZone = true;
        player1NextAllowedBiteTime = elapsedTime + chewDelayAfterSuccessfulBite;
        ApplyBiteVisual(kerupukRenderer1, player1HitCount);
        PauseSwing(kerupukSwing1);
        PlaySuccessfulBiteAnimation(1);
        HandleSuccessfulBite();
        CheckWinner();
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

        player2HitCount++;
        player2MustLeaveZone = true;
        player2NextAllowedBiteTime = elapsedTime + chewDelayAfterSuccessfulBite;
        ApplyBiteVisual(kerupukRenderer2, player2HitCount);
        PauseSwing(kerupukSwing2);
        PlaySuccessfulBiteAnimation(2);
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

        if (player1ReachedTarget)
        {
            if (player1FinishTime < 0f)
                player1FinishTime = elapsedTime;
        }

        if (player2ReachedTarget)
        {
            if (player2FinishTime < 0f)
                player2FinishTime = elapsedTime;
        }

        if (player1ReachedTarget && player2ReachedTarget)
        {
            if (Mathf.Approximately(player1FinishTime, player2FinishTime))
            {
                FinishGame("Draw");
                return;
            }

            if (player1FinishTime < player2FinishTime)
            {
                FinishGame("Player 1");
                return;
            }

            FinishGame("Player 2");
            return;
        }

        if (player1ReachedTarget)
        {
            FinishGame("Player 1");
            return;
        }

        FinishGame("Player 2");
    }

    void FinishGame(string result)
    {
        if (isGameFinished)
            return;

        isGameFinished = true;

        if (winnerText != null)
            winnerText.text = result;

        if (player1TimeText != null)
        {
            player1TimeText.gameObject.SetActive(true);
            player1TimeText.text = "Player 1: " + FormatFinishTime(player1FinishTime);
        }

        if (player2TimeText != null)
        {
            player2TimeText.gameObject.SetActive(true);
            player2TimeText.text = "Player 2: " + FormatFinishTime(player2FinishTime);
        }

        if (gameplayMusic != null)
            gameplayMusic.Stop();

        if (sfxSource != null && resultSfx != null)
            sfxSource.PlayOneShot(resultSfx);

        if (resultPanel != null)
            resultPanel.SetActive(true);
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
