using System.Collections;
using TMPro;
using UnityEngine;

public class BakiakMultiManager : MonoBehaviour, IGameplayCheatTarget
{
    [Header("Players")]
    public PlayerStep player1;
    public PlayerStep player2;
    public Animator anim1;
    public Animator anim2;

    [Header("Rhythm UI")]
    public GameObject player1AttentionRhythmUI;
    public TMP_Text player1InputHintText;
    public GameObject player2AttentionRhythmUI;
    public TMP_Text player2InputHintText;
    public GameObject player1AttentionRhythmWorldUI;
    public TMP_Text player1InputHintWorldText;
    public GameObject player2AttentionRhythmWorldUI;
    public TMP_Text player2InputHintWorldText;

    [Header("Rhythm Timing")]
    public float rhythmVisibleDuration = 1f;
    public float nextRhythmDelay = 1f;
    public float stepAmount = 1f;

    [Header("Fall")]
    public float fallCooldown = 1f;
    public AudioSource sfxSource;
    public AudioClip stepSfx;
    public AudioClip fallSfx;
    public AudioClip resultSfx;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text winnerText;
    public TMP_Text player1TimeText;
    public TMP_Text player2TimeText;

    [Header("Gameplay Timer")]
    public TMP_Text timerText;

    [Header("Audio")]
    public AudioSource gameplayMusic;

    [Header("Move Animation")]
    public string moveFootParameter = "moveFoot";
    public int leftFootValue = 0;
    public int rightFootValue = 1;
    public string stepLeftTrigger = "StepLeft";
    public string stepRightTrigger = "StepRight";

    GameplayStartCountdown gameplayStartCountdown;
    private float elapsedTime;

    private bool isGameFinished;

    private bool player1RhythmActive;
    private bool player2RhythmActive;
    private bool player1CanInput = true;
    private bool player2CanInput = true;

    private bool player1ExpectingLeft = true;
    private bool player2ExpectingLeft = true;

    private bool player1WaitingForRhythmInput;
    private bool player2WaitingForRhythmInput;

    private bool player1Finished;
    private bool player2Finished;
    private float player1FinishTime = -1f;
    private float player2FinishTime = -1f;
    private Coroutine player1RhythmRoutine;
    private Coroutine player2RhythmRoutine;

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindFirstObjectByType<GameplayStartCountdown>();

        SetPlayer1RhythmUI(false);
        SetPlayer2RhythmUI(false);
        UpdateTimerUI();
    }

    void OnEnable()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindFirstObjectByType<GameplayStartCountdown>();

        if (isGameFinished)
            return;

        if (gameplayStartCountdown != null && !gameplayStartCountdown.HasCountdownCompleted())
            return;

        SetPlayer1RhythmUI(false);
        SetPlayer2RhythmUI(false);

        UpdatePlayer1Hint();
        UpdatePlayer2Hint();

        if (player1RhythmRoutine == null)
            player1RhythmRoutine = StartCoroutine(Player1RhythmLoop());

        if (player2RhythmRoutine == null)
            player2RhythmRoutine = StartCoroutine(Player2RhythmLoop());
    }

    void OnDisable()
    {
        if (player1RhythmRoutine != null)
        {
            StopCoroutine(player1RhythmRoutine);
            player1RhythmRoutine = null;
        }

        if (player2RhythmRoutine != null)
        {
            StopCoroutine(player2RhythmRoutine);
            player2RhythmRoutine = null;
        }
    }

    void Update()
    {
        if (isGameFinished)
            return;

        if (gameplayStartCountdown != null && !gameplayStartCountdown.HasCountdownCompleted())
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        if (KeybindManager.Instance == null)
            return;

        HandlePlayer1Input();
        HandlePlayer2Input();
    }

    IEnumerator Player1RhythmLoop()
    {
        while (!isGameFinished)
        {
            yield return new WaitForSeconds(nextRhythmDelay);

            if (isGameFinished)
                yield break;

            player1RhythmActive = true;
            player1WaitingForRhythmInput = true;
            SetPlayer1RhythmUI(true);

            float timer = 0f;
            while (timer < rhythmVisibleDuration && player1WaitingForRhythmInput && !isGameFinished)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            player1RhythmActive = false;
            player1WaitingForRhythmInput = false;
            SetPlayer1RhythmUI(false);
        }

        player1RhythmRoutine = null;
    }

    IEnumerator Player2RhythmLoop()
    {
        while (!isGameFinished)
        {
            yield return new WaitForSeconds(nextRhythmDelay);

            if (isGameFinished)
                yield break;

            player2RhythmActive = true;
            player2WaitingForRhythmInput = true;
            SetPlayer2RhythmUI(true);

            float timer = 0f;
            while (timer < rhythmVisibleDuration && player2WaitingForRhythmInput && !isGameFinished)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            player2RhythmActive = false;
            player2WaitingForRhythmInput = false;
            SetPlayer2RhythmUI(false);
        }

        player2RhythmRoutine = null;
    }

    void HandlePlayer1Input()
    {
        if (!player1CanInput || player1Finished)
            return;

        KeyCode expectedKey = player1ExpectingLeft ? GetPlayer1LeftKey() : GetPlayer1RightKey();
        KeyCode wrongKey = player1ExpectingLeft ? GetPlayer1RightKey() : GetPlayer1LeftKey();

        if (Input.GetKeyDown(expectedKey))
            HandlePlayer1Result(true);
        else if (Input.GetKeyDown(wrongKey))
            HandlePlayer1Result(false);
    }

    void HandlePlayer2Input()
    {
        if (!player2CanInput || player2Finished)
            return;

        KeyCode expectedKey = player2ExpectingLeft ? GetPlayer2LeftKey() : GetPlayer2RightKey();
        KeyCode wrongKey = player2ExpectingLeft ? GetPlayer2RightKey() : GetPlayer2LeftKey();

        if (Input.GetKeyDown(expectedKey))
            HandlePlayer2Result(true);
        else if (Input.GetKeyDown(wrongKey))
            HandlePlayer2Result(false);
    }

    void HandlePlayer1Result(bool correctButton)
    {
        if (!player1RhythmActive)
        {
            StartCoroutine(Player1Fall());
            return;
        }

        player1WaitingForRhythmInput = false;
        player1RhythmActive = false;
        SetPlayer1RhythmUI(false);

        if (correctButton)
        {
            if (player1 != null)
                player1.StepForward(stepAmount);

            if (anim1 != null)
            {
                anim1.SetInteger(moveFootParameter, player1ExpectingLeft ? leftFootValue : rightFootValue);
                if (player1ExpectingLeft)
                {
                    anim1.ResetTrigger(stepRightTrigger);
                    anim1.SetTrigger(stepLeftTrigger);
                }
                else
                {
                    anim1.ResetTrigger(stepLeftTrigger);
                    anim1.SetTrigger(stepRightTrigger);
                }
                anim1.SetBool("isMoving", true);
                StartCoroutine(ResetMoveAnimation(anim1));
            }

            if (sfxSource != null && stepSfx != null)
                sfxSource.PlayOneShot(stepSfx);

            player1ExpectingLeft = !player1ExpectingLeft;
            UpdatePlayer1Hint();
        }
        else
        {
            StartCoroutine(Player1Fall());
        }
    }

    void HandlePlayer2Result(bool correctButton)
    {
        if (!player2RhythmActive)
        {
            StartCoroutine(Player2Fall());
            return;
        }

        player2WaitingForRhythmInput = false;
        player2RhythmActive = false;
        SetPlayer2RhythmUI(false);

        if (correctButton)
        {
            if (player2 != null)
                player2.StepForward(stepAmount);

            if (anim2 != null)
            {
                anim2.SetInteger(moveFootParameter, player2ExpectingLeft ? leftFootValue : rightFootValue);
                if (player2ExpectingLeft)
                {
                    anim2.ResetTrigger(stepRightTrigger);
                    anim2.SetTrigger(stepLeftTrigger);
                }
                else
                {
                    anim2.ResetTrigger(stepLeftTrigger);
                    anim2.SetTrigger(stepRightTrigger);
                }
                anim2.SetBool("isMoving", true);
                StartCoroutine(ResetMoveAnimation(anim2));
            }

            if (sfxSource != null && stepSfx != null)
                sfxSource.PlayOneShot(stepSfx);

            player2ExpectingLeft = !player2ExpectingLeft;
            UpdatePlayer2Hint();
        }
        else
        {
            StartCoroutine(Player2Fall());
        }
    }

    IEnumerator Player1Fall()
    {
        player1CanInput = false;
        player1ExpectingLeft = true;
        UpdatePlayer1Hint();

        if (anim1 != null)
        {
            anim1.SetBool("isMoving", false);
            anim1.SetBool("isFalling", true);
            anim1.SetTrigger("Fall");
        }

        if (sfxSource != null && fallSfx != null)
            sfxSource.PlayOneShot(fallSfx);

        yield return new WaitForSeconds(fallCooldown);

        if (anim1 != null)
            anim1.SetBool("isFalling", false);

        player1CanInput = true;
    }

    IEnumerator Player2Fall()
    {
        player2CanInput = false;
        player2ExpectingLeft = true;
        UpdatePlayer2Hint();

        if (anim2 != null)
        {
            anim2.SetBool("isMoving", false);
            anim2.SetBool("isFalling", true);
            anim2.SetTrigger("Fall");
        }

        if (sfxSource != null && fallSfx != null)
            sfxSource.PlayOneShot(fallSfx);

        yield return new WaitForSeconds(fallCooldown);

        if (anim2 != null)
            anim2.SetBool("isFalling", false);

        player2CanInput = true;
    }

    IEnumerator ResetMoveAnimation(Animator anim)
    {
        yield return new WaitForSeconds(0.15f);

        if (anim != null)
            anim.SetBool("isMoving", false);
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

    void FinishMatch()
    {
        if (isGameFinished)
            return;

        isGameFinished = true;

        SetPlayer1RhythmUI(false);
        SetPlayer2RhythmUI(false);

        if (anim1 != null)
        {
            anim1.SetBool("isMoving", false);
            anim1.SetBool("isFalling", false);
        }

        if (anim2 != null)
        {
            anim2.SetBool("isMoving", false);
            anim2.SetBool("isFalling", false);
        }

        if (gameplayMusic != null)
            gameplayMusic.Stop();

        if (sfxSource != null && resultSfx != null)
            sfxSource.PlayOneShot(resultSfx);

        if (winnerText != null)
            winnerText.text = GetWinnerLabel();

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

        if (resultPanel != null)
            resultPanel.SetActive(true);
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
            return "Player 1";

        if (player2Finished && !player1Finished)
            return "Player 2";

        if (player1Finished && player2Finished)
        {
            if (player1FinishTime < player2FinishTime)
                return "Player 1";

            if (player2FinishTime < player1FinishTime)
                return "Player 2";

            return "Draw";
        }

        return "";
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

    void SetRhythmUI(bool visible, GameObject rhythmUI, TMP_Text hintText)
    {
        if (rhythmUI != null)
            rhythmUI.SetActive(visible);

        if (hintText != null)
            hintText.gameObject.SetActive(visible);
    }

    void SetPlayer1RhythmUI(bool visible)
    {
        SetRhythmUI(visible, player1AttentionRhythmUI, player1InputHintText);
        SetRhythmUI(visible, player1AttentionRhythmWorldUI, player1InputHintWorldText);
    }

    void SetPlayer2RhythmUI(bool visible)
    {
        SetRhythmUI(visible, player2AttentionRhythmUI, player2InputHintText);
        SetRhythmUI(visible, player2AttentionRhythmWorldUI, player2InputHintWorldText);
    }

    void UpdatePlayer1Hint()
    {
        if (KeybindManager.Instance == null)
            return;

        KeyCode currentKey = player1ExpectingLeft ? GetPlayer1LeftKey() : GetPlayer1RightKey();
        SetHintText(player1InputHintText, currentKey);
        SetHintText(player1InputHintWorldText, currentKey);
    }

    void UpdatePlayer2Hint()
    {
        if (KeybindManager.Instance == null)
            return;

        KeyCode currentKey = player2ExpectingLeft ? GetPlayer2LeftKey() : GetPlayer2RightKey();
        SetHintText(player2InputHintText, currentKey);
        SetHintText(player2InputHintWorldText, currentKey);
    }

    void SetHintText(TMP_Text targetText, KeyCode key)
    {
        if (targetText != null)
            targetText.text = key.ToString();
    }

    KeyCode GetPlayer1LeftKey()
    {
        return KeybindManager.Instance != null ? KeybindManager.Instance.bakiakPlayer1Left : KeyCode.A;
    }

    KeyCode GetPlayer1RightKey()
    {
        return KeybindManager.Instance != null ? KeybindManager.Instance.bakiakPlayer1Right : KeyCode.D;
    }

    KeyCode GetPlayer2LeftKey()
    {
        return KeybindManager.Instance != null ? KeybindManager.Instance.bakiakPlayer2Left : KeyCode.LeftArrow;
    }

    KeyCode GetPlayer2RightKey()
    {
        return KeybindManager.Instance != null ? KeybindManager.Instance.bakiakPlayer2Right : KeyCode.RightArrow;
    }
}
