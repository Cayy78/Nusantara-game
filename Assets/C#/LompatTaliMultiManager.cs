using TMPro;
using UnityEngine;

public class LompatTaliMultiManager : MonoBehaviour, IGameplayCheatTarget
{
    [Header("Players")]
    public LompatTaliPlayerJumper player1;
    public LompatTaliPlayerJumper player2;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text winnerText;
    public TMP_Text player1TimeText;
    public TMP_Text player2TimeText;

    [Header("Gameplay Timer")]
    public TMP_Text timerText;

    [Header("Best Of 3")]
    public int roundsToWin = 2;
    [Min(0f)] public float nextRoundDelay = 1.25f;
    [Min(0f)] public float simultaneousHitWindow = 0.1f;
    public TMP_Text player1RoundScoreText;
    public TMP_Text roundInfoText;
    public TMP_Text player2RoundScoreText;
    public string player1RoundLabel = "P1";
    public string player2RoundLabel = "P2";

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

    private bool isGameFinished;
    private bool player1HitThisCycle;
    private bool player2HitThisCycle;
    private bool ropeDangerPhaseActive;
    private bool roundTransitionActive;
    private float elapsedTime;
    private float player1FinishTime = -1f;
    private float player2FinishTime = -1f;
    private int player1RoundWins;
    private int player2RoundWins;
    private int currentRound = 1;
    private Coroutine roundResolutionRoutine;

    void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        UpdateRoundUI();
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameFinished || KeybindManager.Instance == null)
            return;

        if (!roundTransitionActive)
            elapsedTime += Time.deltaTime;

        UpdateTimerUI();

        if (roundTransitionActive)
            return;

        if (Input.GetKeyDown(KeybindManager.Instance.lompatTaliPlayer1))
            HandleJump(player1, jumpSfx);

        if (Input.GetKeyDown(KeybindManager.Instance.lompatTaliPlayer2))
            HandleJump(player2, jumpSfx);
    }

    void HandleJump(LompatTaliPlayerJumper player, AudioClip sfx)
    {
        if (player == null)
            return;

        bool wasJumping = player.IsJumping;
        player.TryJump();

        if (!wasJumping && player.IsJumping && sfxSource != null && sfx != null)
            sfxSource.PlayOneShot(sfx);
    }

    public void CheatPlayer1Win()
    {
        if (isGameFinished)
            return;

        player1RoundWins = roundsToWin;
        player2RoundWins = 0;
        FinishMatch("Player 1");
    }

    public void CheatPlayer2Win()
    {
        if (isGameFinished)
            return;

        player1RoundWins = 0;
        player2RoundWins = roundsToWin;
        FinishMatch("Player 2");
    }

    public void CheatDraw()
    {
        if (isGameFinished)
            return;

        player1RoundWins = roundsToWin;
        player2RoundWins = roundsToWin;
        FinishMatch("Draw");
    }

    public void RegisterPlayerHit(int playerIndex)
    {
        if (isGameFinished || roundTransitionActive || !ropeDangerPhaseActive)
            return;

        if (playerIndex == 1)
        {
            if (player1HitThisCycle)
                return;

            player1HitThisCycle = true;
            if (player1 != null)
                player1.OnHit();
            SpawnHitEffect(1);
        }
        else if (playerIndex == 2)
        {
            if (player2HitThisCycle)
                return;

            player2HitThisCycle = true;
            if (player2 != null)
                player2.OnHit();
            SpawnHitEffect(2);
        }

        if (sfxSource != null && hitSfx != null)
            sfxSource.PlayOneShot(hitSfx);

        if (roundResolutionRoutine == null)
            roundResolutionRoutine = StartCoroutine(ResolveRoundAfterHitWindow());
    }

    public void BeginRopeDangerPhase()
    {
        if (isGameFinished || roundTransitionActive)
            return;

        ropeDangerPhaseActive = true;
        player1HitThisCycle = false;
        player2HitThisCycle = false;
    }

    public void EndRopeDangerPhase()
    {
        if (isGameFinished || roundTransitionActive || !ropeDangerPhaseActive)
            return;

        ropeDangerPhaseActive = false;
        player1HitThisCycle = false;
        player2HitThisCycle = false;
    }

    public void RegisterRopePass(int playerIndex)
    {
        // Legacy no-op.
    }

    System.Collections.IEnumerator ResolveRoundAfterHitWindow()
    {
        yield return new WaitForSeconds(simultaneousHitWindow);
        roundResolutionRoutine = null;

        if (isGameFinished || roundTransitionActive)
            yield break;

        ropeDangerPhaseActive = false;

        bool player1Hit = player1HitThisCycle;
        bool player2Hit = player2HitThisCycle;
        player1HitThisCycle = false;
        player2HitThisCycle = false;

        if (player1Hit && player2Hit)
        {
            UpdateRoundUI("Round " + currentRound + ": Draw");
            StartCoroutine(ResetRoundRoutine(false));
            yield break;
        }

        if (player1Hit)
        {
            player2RoundWins++;
            UpdateRoundUI("Round " + currentRound + ": Player 2 Wins");
            TryFinishOrAdvanceRound("Player 2");
            yield break;
        }

        if (player2Hit)
        {
            player1RoundWins++;
            UpdateRoundUI("Round " + currentRound + ": Player 1 Wins");
            TryFinishOrAdvanceRound("Player 1");
        }
    }

    void TryFinishOrAdvanceRound(string roundWinner)
    {
        if (player1RoundWins >= roundsToWin)
        {
            FinishMatch("Player 1");
            return;
        }

        if (player2RoundWins >= roundsToWin)
        {
            FinishMatch("Player 2");
            return;
        }

        StartCoroutine(ResetRoundRoutine(true));
    }

    System.Collections.IEnumerator ResetRoundRoutine(bool advanceRoundNumber)
    {
        roundTransitionActive = true;
        yield return new WaitForSeconds(nextRoundDelay);

        if (isGameFinished)
            yield break;

        if (advanceRoundNumber)
            currentRound++;

        ResetPlayersForNextRound();
        player1HitThisCycle = false;
        player2HitThisCycle = false;
        ropeDangerPhaseActive = false;
        roundTransitionActive = false;
        UpdateRoundUI("Round " + currentRound + " - Jump!");
    }

    void ResetPlayersForNextRound()
    {
        if (player1 != null)
            player1.ResetForNextRound();

        if (player2 != null)
            player2.ResetForNextRound();
    }

    void FinishMatch(string result)
    {
        if (isGameFinished)
            return;

        isGameFinished = true;
        roundTransitionActive = false;
        ropeDangerPhaseActive = false;

        if (player1RoundWins > player2RoundWins)
        {
            player1FinishTime = elapsedTime;
            player2FinishTime = -1f;
        }
        else if (player2RoundWins > player1RoundWins)
        {
            player1FinishTime = -1f;
            player2FinishTime = elapsedTime;
        }
        else
        {
            player1FinishTime = elapsedTime;
            player2FinishTime = elapsedTime;
        }

        if (winnerText != null)
            winnerText.text = result;

        if (player1TimeText != null)
        {
            player1TimeText.gameObject.SetActive(true);
            player1TimeText.text = "Player 1 Rounds: " + player1RoundWins;
        }

        if (player2TimeText != null)
        {
            player2TimeText.gameObject.SetActive(true);
            player2TimeText.text = "Player 2 Rounds: " + player2RoundWins;
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

    void UpdateRoundUI(string roundStatusOverride = null)
    {
        if (player1RoundScoreText != null)
            player1RoundScoreText.text = player1RoundLabel + ": " + player1RoundWins;

        if (player2RoundScoreText != null)
            player2RoundScoreText.text = player2RoundLabel + ": " + player2RoundWins;

        if (roundInfoText != null)
            roundInfoText.text = string.IsNullOrEmpty(roundStatusOverride)
                ? "Best of " + ((roundsToWin * 2) - 1) + " - Round " + currentRound
                : roundStatusOverride;
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
}
