using System.Collections;
using TMPro;
using UnityEngine;

public class BakiakSingleManager : MonoBehaviour, IGameplayCheatTarget
{
    const string WinResultLabel = "WIN";
    const string LoseResultLabel = "LOSE";
    const string DrawResultLabel = "DRAW";

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

    [Header("Move Animation")]
    public string moveFootParameter = "moveFoot";
    public int leftFootValue = 0;
    public int rightFootValue = 1;
    public string stepLeftTrigger = "StepLeft";
    public string stepRightTrigger = "StepRight";

    [Header("Bot")]
    public bool useBotForPlayer2 = true;
    public float botMinReactionDelay = 0.15f;
    public float botMaxReactionDelay = 0.45f;
    [Range(0f, 1f)] public float botMistakeChance = 0.2f;

    [HideInInspector] public bool isGameFinished;

    GameplayStartCountdown gameplayStartCountdown;
    bool player1RhythmActive;
    bool player2RhythmActive;
    bool player1CanInput = true;
    bool player2CanInput = true;
    bool player1ExpectingLeft = true;
    bool player2ExpectingLeft = true;
    bool player1WaitingForRhythmInput;
    bool player2WaitingForRhythmInput;
    bool player1Finished;
    bool player2Finished;
    float elapsedTime;
    float player1FinishTime = -1f;
    float player2FinishTime = -1f;
    Coroutine player1RhythmRoutine;
    Coroutine player2RhythmRoutine;
    Coroutine botRoutine;

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindFirstObjectByType<GameplayStartCountdown>();

        SetPlayer1RhythmUI(false);
        SetPlayer2RhythmUI(false);
        ShowSpecificResultPanels(string.Empty);

        if (resultPanel != null)
            resultPanel.SetActive(false);

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

        RestartBotRoutineIfNeeded();
    }

    void OnDisable()
    {
        StopAllManagedCoroutines();
    }

    void Update()
    {
        if (isGameFinished)
            return;

        if (gameplayStartCountdown != null && !gameplayStartCountdown.HasCountdownCompleted())
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();
        HandlePlayer1Input();
    }

    public bool IsPlayerFinished(int playerIndex)
    {
        return playerIndex == 1 ? player1Finished : player2Finished;
    }

    public bool CanBotAct()
    {
        return useBotForPlayer2 && !isGameFinished && !player2Finished && player2 != null;
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

    IEnumerator BotLoop()
    {
        while (enabled)
        {
            if (!CanBotAct() || !player2CanInput || !player2RhythmActive || !player2WaitingForRhythmInput)
            {
                yield return null;
                continue;
            }

            float delay = Random.Range(botMinReactionDelay, botMaxReactionDelay);
            yield return new WaitForSeconds(delay);

            if (!CanBotAct() || !player2CanInput || !player2RhythmActive || !player2WaitingForRhythmInput)
                continue;

            bool pressCorrectButton = Random.value >= botMistakeChance;
            HandlePlayer2Result(pressCorrectButton);
        }
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
            ApplySuccessfulStep(player1, anim1, player1ExpectingLeft);
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
            ApplySuccessfulStep(player2, anim2, player2ExpectingLeft);
            player2ExpectingLeft = !player2ExpectingLeft;
            UpdatePlayer2Hint();
        }
        else
        {
            StartCoroutine(Player2Fall());
        }
    }

    void ApplySuccessfulStep(PlayerStep player, Animator anim, bool expectingLeft)
    {
        if (player != null)
            player.StepForward(stepAmount);

        if (anim != null)
        {
            anim.SetInteger(moveFootParameter, expectingLeft ? leftFootValue : rightFootValue);

            if (expectingLeft)
            {
                anim.ResetTrigger(stepRightTrigger);
                anim.SetTrigger(stepLeftTrigger);
            }
            else
            {
                anim.ResetTrigger(stepLeftTrigger);
                anim.SetTrigger(stepRightTrigger);
            }

            anim.SetBool("isMoving", true);
            StartCoroutine(ResetMoveAnimation(anim));
        }

        if (sfxSource != null && stepSfx != null)
            sfxSource.PlayOneShot(stepSfx);
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

    void FinishMatch()
    {
        if (isGameFinished)
            return;

        isGameFinished = true;
        StopAllManagedCoroutines();

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

        ShowSpecificResultPanels(resultLabel);
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

    void SetRhythmUI(bool visible, GameObject rhythmUI, TMP_Text hintText)
    {
        if (rhythmUI != null)
            rhythmUI.SetActive(visible);

        if (hintText != null)
            hintText.gameObject.SetActive(visible);
    }

    void UpdatePlayer1Hint()
    {
        KeyCode currentKey = player1ExpectingLeft ? GetPlayer1LeftKey() : GetPlayer1RightKey();
        SetHintText(player1InputHintText, currentKey);
        SetHintText(player1InputHintWorldText, currentKey);
    }

    void UpdatePlayer2Hint()
    {
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
        return KeybindManager.Instance != null ? KeybindManager.Instance.bakiakSingleLeft : KeyCode.A;
    }

    KeyCode GetPlayer1RightKey()
    {
        return KeybindManager.Instance != null ? KeybindManager.Instance.bakiakSingleRight : KeyCode.D;
    }

    KeyCode GetPlayer2LeftKey()
    {
        return KeybindManager.Instance != null ? KeybindManager.Instance.bakiakPlayer2Left : KeyCode.LeftArrow;
    }

    KeyCode GetPlayer2RightKey()
    {
        return KeybindManager.Instance != null ? KeybindManager.Instance.bakiakPlayer2Right : KeyCode.RightArrow;
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

    void StopAllManagedCoroutines()
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

        StopBotRoutine();
    }
}
