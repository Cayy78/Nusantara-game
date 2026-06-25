using UnityEngine;
using TMPro;

public class BalapKarungMultiplayerManager : MonoBehaviour, IGameplayCheatTarget
{
    [Header("Players")]
    public PlayerJump player1;
    public PlayerJump player2;

    public Animator anim1;
    public Animator anim2;

    [Header("Power Bars")]
    public PanjatPinangPowerBar bar1;
    public PanjatPinangPowerBar bar2;

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
    public AudioClip jumpSfx;
    public AudioClip resultSfx;

    [Header("Fall Animation")]
    [Min(0f)] public float fallCooldown = 0.75f;
    public string fallTriggerName = "Fall";
    public string fallBoolName = "isFalling";

    private float elapsedTime;

    private bool isGameFinished;
    private bool player1Finished;
    private bool player2Finished;
    private float player1FinishTime = -1f;
    private float player2FinishTime = -1f;
    private bool player1Falling;
    private bool player2Falling;
    private Coroutine player1FallRoutine;
    private Coroutine player2FallRoutine;

    void Start()
    {
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameFinished)
            return;

        elapsedTime += Time.deltaTime;
        UpdateTimerUI();

        if (KeybindManager.Instance == null)
            return;

        HandlePlayerInput(
            KeybindManager.Instance.balapKarungPlayer1,
            player1,
            anim1,
            bar1,
            player1Finished
        );

        HandlePlayerInput(
            KeybindManager.Instance.balapKarungPlayer2,
            player2,
            anim2,
            bar2,
            player2Finished
        );
    }

    void HandlePlayerInput(KeyCode key, PlayerJump player, Animator anim, PanjatPinangPowerBar bar, bool alreadyFinished)
    {
        bool isPlayer1Lane = player == player1;
        bool isFalling = isPlayer1Lane ? player1Falling : player2Falling;

        if (alreadyFinished || isFalling)
            return;

        if (Input.GetKeyDown(key))
        {
            if (bar != null)
                bar.StartBar();

            if (anim != null)
                anim.SetBool("isMoving", true);
        }

        if (Input.GetKeyUp(key))
        {
            float stepValue = 0f;
            float rawValue = 0.8f;
            PanjatPinangPowerBar.PowerZone resultZone = PanjatPinangPowerBar.PowerZone.Green;

            if (bar != null)
            {
                rawValue = bar.StopBar();
                bar.ResetBar();
                stepValue = bar.GetStepAmount(rawValue);
                resultZone = bar.GetZone(rawValue);
            }

            if (resultZone == PanjatPinangPowerBar.PowerZone.Red)
            {
                TriggerFall(isPlayer1Lane ? 1 : 2, anim);
            }
            else if (player != null)
            {
                if (stepValue > 0f)
                    player.JumpForward(stepValue);
                else if (stepValue < 0f)
                    player.SlipBackward(Mathf.Abs(stepValue));
            }

            if (anim != null)
                anim.SetBool("isMoving", false);

            if (stepValue > 0f && sfxSource != null && jumpSfx != null)
                sfxSource.PlayOneShot(jumpSfx);
        }
    }

    void TriggerFall(int playerIndex, Animator anim)
    {
        if (fallCooldown <= 0f)
            return;

        if (playerIndex == 1)
        {
            if (player1FallRoutine != null)
                StopCoroutine(player1FallRoutine);

            player1FallRoutine = StartCoroutine(PlayFallRoutine(1, anim));
            return;
        }

        if (player2FallRoutine != null)
            StopCoroutine(player2FallRoutine);

        player2FallRoutine = StartCoroutine(PlayFallRoutine(2, anim));
    }

    System.Collections.IEnumerator PlayFallRoutine(int playerIndex, Animator anim)
    {
        SetPlayerFalling(playerIndex, true);

        if (anim != null)
        {
            anim.SetBool("isMoving", false);

            if (!string.IsNullOrEmpty(fallBoolName))
                anim.SetBool(fallBoolName, true);

            if (!string.IsNullOrEmpty(fallTriggerName))
                anim.SetTrigger(fallTriggerName);
        }

        yield return new WaitForSeconds(fallCooldown);

        if (anim != null && !string.IsNullOrEmpty(fallBoolName))
            anim.SetBool(fallBoolName, false);

        SetPlayerFalling(playerIndex, false);

        if (playerIndex == 1)
            player1FallRoutine = null;
        else
            player2FallRoutine = null;
    }

    void SetPlayerFalling(int playerIndex, bool isFallingNow)
    {
        if (playerIndex == 1)
            player1Falling = isFallingNow;
        else
            player2Falling = isFallingNow;
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

        if (anim1 != null)
        {
            anim1.SetBool("isMoving", false);
            if (!string.IsNullOrEmpty(fallBoolName))
                anim1.SetBool(fallBoolName, false);
        }

        if (anim2 != null)
        {
            anim2.SetBool("isMoving", false);
            if (!string.IsNullOrEmpty(fallBoolName))
                anim2.SetBool(fallBoolName, false);
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
}
