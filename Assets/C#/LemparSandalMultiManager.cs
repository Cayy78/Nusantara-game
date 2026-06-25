using TMPro;
using UnityEngine;

public class LemparSandalMultiManager : MonoBehaviour, IGameplayCheatTarget
{
    [Header("Spawn Points")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;

    [Header("Player Colliders")]
    public Collider2D player1Collider;
    public Collider2D player2Collider;

    [Header("Power Bars")]
    public LemparSandalPowerBar bar1;
    public LemparSandalPowerBar bar2;

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
    public AudioClip throwSfx;
    public AudioClip hitSfx;
    public AudioClip resultSfx;

    private int player1HitCount;
    private int player2HitCount;
    private bool isGameFinished;
    private bool player1ProjectileActive;
    private bool player2ProjectileActive;
    private int player1TargetIndex;
    private int player2TargetIndex;
    private float elapsedTime;
    private float player1FinishTime = -1f;
    private float player2FinishTime = -1f;

    void Start()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);

        UpdateScoreUI();
        ApplyInitialTargetPositions();
        SetTrajectoryVisible(trajectoryLine1, false);
        SetTrajectoryVisible(trajectoryLine2, false);
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameFinished)
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        HandlePlayerInput(GetPlayer1Key(), bar1, spawnPoint1, trajectoryLine1, 1, player1ProjectileActive);
        HandlePlayerInput(GetPlayer2Key(), bar2, spawnPoint2, trajectoryLine2, 2, player2ProjectileActive);
    }

    public void CheatPlayer1Win()
    {
        if (isGameFinished)
            return;

        player1HitCount = targetHitCount;
        player1FinishTime = elapsedTime;
        UpdateScoreUI();
        FinishGame("Player 1");
    }

    public void CheatPlayer2Win()
    {
        if (isGameFinished)
            return;

        player2HitCount = targetHitCount;
        player2FinishTime = elapsedTime;
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
        UpdateScoreUI();
        FinishGame("Draw");
    }

    void HandlePlayerInput(KeyCode key, LemparSandalPowerBar bar, Transform spawnPoint, LineRenderer trajectoryLine, int playerIndex, bool projectileActive)
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
            ThrowSandal(playerIndex, spawnPoint.position, rawValue);
        }
    }

    void PlayThrowAnimation(int playerIndex)
    {
        Animator targetAnimator = playerIndex == 1 ? player1Animator : player2Animator;

        if (targetAnimator == null || string.IsNullOrEmpty(throwTriggerName))
            return;

        targetAnimator.SetTrigger(throwTriggerName);
    }

    void ThrowSandal(int playerIndex, Vector3 spawnPosition, float rawValue)
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
            projectile.manager = this;
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

    public void NotifyProjectileFinished(int playerIndex)
    {
        if (playerIndex == 1)
            player1ProjectileActive = false;
        else if (playerIndex == 2)
            player2ProjectileActive = false;
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
        SetTrajectoryVisible(trajectoryLine1, false);
        SetTrajectoryVisible(trajectoryLine2, false);

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

    void UpdateScoreUI()
    {
        if (successCountText1 != null)
            successCountText1.text = player1HitCount + "/" + targetHitCount;

        if (successCountText2 != null)
            successCountText2.text = player2HitCount + "/" + targetHitCount;
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

        int nextIndex = Mathf.Min(currentIndex + 1, targetPositionsX.Length - 1);
        currentIndex = nextIndex;
        targetMover.SetTargetX(targetPositionsX[currentIndex], targetMoveDelay);
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

    KeyCode GetPlayer1Key()
    {
        return KeybindManager.Instance != null
            ? KeybindManager.Instance.lemparSandalPlayer1
            : KeyCode.Space;
    }

    KeyCode GetPlayer2Key()
    {
        return KeybindManager.Instance != null
            ? KeybindManager.Instance.lemparSandalPlayer2
            : KeyCode.RightShift;
    }
}
