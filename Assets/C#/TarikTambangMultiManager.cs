using UnityEngine;
using TMPro;

public class TarikTambangMultiManager : MonoBehaviour
{
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

    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text winnerText;

    [Header("Audio")]
    public AudioSource gameplayMusic;
    public AudioSource sfxSource;
    public AudioClip pullSfx;
    public AudioClip resultSfx;

    private bool isGameFinished;

    void Update()
    {
        if (isGameFinished)
            return;

        if (KeybindManager.Instance == null)
            return;

        if (Input.GetKeyDown(KeybindManager.Instance.tarikTambangPlayer1))
            PullLeft();

        if (Input.GetKeyDown(KeybindManager.Instance.tarikTambangPlayer2))
            PullRight();

        CheckWinCondition();
    }

    void PullLeft()
    {
        if (tugGroup == null)
            return;

        Vector3 pos = tugGroup.position;
        pos.x -= pullStep;
        tugGroup.position = pos;

        if (anim1 != null)
            anim1.SetBool("isPulling", true);

        if (anim2 != null)
            anim2.SetBool("isPulling", false);

        if (sfxSource != null && pullSfx != null)
            sfxSource.PlayOneShot(pullSfx);
    }

    void PullRight()
    {
        if (tugGroup == null)
            return;

       Vector3 pos = tugGroup.position;
        pos.x += pullStep;
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
            FinishGame("Player 1");
            return;
        }

        if (player2Collider != null && winningAreaRight != null && player2Collider.IsTouching(winningAreaRight))
        {
            FinishGame("Player 2 ");
            return;
        }
    }

    void FinishGame(string resultText)
    {
        if (isGameFinished)
            return;

        isGameFinished = true;

        if (anim1 != null)
            anim1.SetBool("isPulling", false);

        if (anim2 != null)
            anim2.SetBool("isPulling", false);

        if (gameplayMusic != null)
            gameplayMusic.Stop();

        if (sfxSource != null && resultSfx != null)
            sfxSource.PlayOneShot(resultSfx);

        if (winnerText != null)
            winnerText.text = resultText;

        if (resultPanel != null)
            resultPanel.SetActive(true);
    }
}
