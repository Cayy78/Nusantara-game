using System.Collections;
using TMPro;
using UnityEngine;

public class TarikTambangSingleManager : MonoBehaviour
{
    const string WinResultLabel = "WIN";
    const string LoseResultLabel = "LOSE";
    const string DrawResultLabel = "DRAW";

    [Header("Group")]
    public Transform tugGroup;
    public float pullStep = 0.2f;
    public float minX = -3f;
    public float maxX = 3f;

    [Header("Players")]
    public Collider2D player1Collider;
    public Collider2D player2Collider;
    public Animator anim1;
    public Animator anim2;

    [Header("Winning Areas")]
    public Collider2D winningAreaLeft;
    public Collider2D winningAreaRight;

    [Header("Countdown")]
    public GameplayStartCountdown gameplayStartCountdown;

    [Header("Result Panels")]
    public GameObject resultPanel;
    public GameObject winResultPanel;
    public GameObject loseResultPanel;
    public GameObject drawResultPanel;

    [Header("Audio")]
    public AudioSource gameplayMusic;
    public AudioSource sfxSource;
    public AudioClip pullSfx;
    public AudioClip resultSfx;

    [Header("Bot")]
    public bool useBotForPlayer2 = true;
    public float minDecisionDelay = 0.2f;
    public float maxDecisionDelay = 0.6f;

    [HideInInspector] public bool isGameFinished;

    Coroutine botRoutine;

    void Start()
    {
        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindFirstObjectByType<GameplayStartCountdown>();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        ShowSpecificResultPanels(string.Empty);
    }

    void OnEnable()
    {
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

        if (!IsGameplayReady())
            return;

        KeyCode player1Key = GetPlayer1Key();

        if (Input.GetKeyDown(player1Key))
            PullLeft();

        CheckWinCondition();
    }

    public KeyCode GetPlayer1Key()
    {
        if (KeybindManager.Instance != null)
            return KeybindManager.Instance.tarikTambangSingle;

        return KeyCode.Space;
    }

    public bool CanBotAct()
    {
        return useBotForPlayer2 && !isGameFinished && tugGroup != null && IsGameplayReady();
    }

    public void PullLeft()
    {
        if (tugGroup == null || isGameFinished || !IsGameplayReady())
            return;

        Vector3 pos = tugGroup.position;
        pos.x -= pullStep;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        tugGroup.position = pos;

        if (anim1 != null)
            anim1.SetBool("isPulling", true);

        if (anim2 != null)
            anim2.SetBool("isPulling", false);

        if (sfxSource != null && pullSfx != null)
            sfxSource.PlayOneShot(pullSfx);
    }

    public void PullRight()
    {
        if (tugGroup == null || isGameFinished || !IsGameplayReady())
            return;

        Vector3 pos = tugGroup.position;
        pos.x += pullStep;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        tugGroup.position = pos;

        if (anim2 != null)
            anim2.SetBool("isPulling", true);

        if (anim1 != null)
            anim1.SetBool("isPulling", false);

        if (sfxSource != null && pullSfx != null)
            sfxSource.PlayOneShot(pullSfx);
    }

    void CheckWinCondition()
    {
        if (player1Collider != null && winningAreaLeft != null && player1Collider.IsTouching(winningAreaLeft))
        {
            FinishGame(WinResultLabel);
            return;
        }

        if (player2Collider != null && winningAreaRight != null && player2Collider.IsTouching(winningAreaRight))
        {
            FinishGame(LoseResultLabel);
        }
    }

    void FinishGame(string resultLabel)
    {
        if (isGameFinished)
            return;

        isGameFinished = true;
        StopBotRoutine();

        if (anim1 != null)
            anim1.SetBool("isPulling", false);

        if (anim2 != null)
            anim2.SetBool("isPulling", false);

        if (gameplayMusic != null)
            gameplayMusic.Stop();

        if (sfxSource != null && resultSfx != null)
            sfxSource.PlayOneShot(resultSfx);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        ShowSpecificResultPanels(resultLabel);
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

            float delay = Random.Range(minDecisionDelay, maxDecisionDelay);
            yield return new WaitForSeconds(delay);

            if (!CanBotAct())
                continue;

            PullRight();
        }
    }

    bool IsGameplayReady()
    {
        if (gameplayStartCountdown == null)
            return true;

        return gameplayStartCountdown.HasCountdownCompleted();
    }

}
