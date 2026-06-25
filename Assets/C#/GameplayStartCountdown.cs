using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayStartCountdown : MonoBehaviour
{
    public GameObject countdownPanel;
    public TMP_Text countdownText;
    public Image countdownImage;

    public Sprite readySprite;
    public Sprite setSprite;
    public Sprite goSprite;

    public AudioSource sfxSource;
    public AudioClip readySetGoClip;

    public float setTextTime = 1f;
    public float goTextTime = 2f;
    public float countdownEndTime = 3f;

    public MonoBehaviour[] behavioursToEnableAfterCountdown;

    private Coroutine countdownRoutine;
    private bool countdownCompleted;
    private bool isCountingDown;

    void Start()
    {
        StartCountdown();
    }

    public bool IsCountdownRunning()
    {
        return isCountingDown && !countdownCompleted;
    }

    public bool HasCountdownCompleted()
    {
        return countdownCompleted;
    }

    public void StartCountdown()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        countdownCompleted = false;
        isCountingDown = false;

        if (countdownRoutine != null)
            StopCoroutine(countdownRoutine);

        SetGameplayEnabled(false);
        countdownRoutine = StartCoroutine(PlayCountdown());
    }

    public void PauseCountdown()
    {
        if (!IsCountdownRunning())
            return;

        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }

        isCountingDown = false;

        if (sfxSource != null)
            sfxSource.Stop();

        SetCountdownPanelVisible(false);
    }

    public void ResumeCountdownFromStart()
    {
        if (countdownCompleted)
            return;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StartCountdown();
    }

    IEnumerator PlayCountdown()
    {
        isCountingDown = true;

        SetCountdownPanelVisible(true);

        ShowCountdown("READY", readySprite);

        if (sfxSource != null && readySetGoClip != null)
        {
            sfxSource.Stop();
            sfxSource.clip = readySetGoClip;
            sfxSource.Play();
        }

        yield return new WaitForSeconds(setTextTime);

        ShowCountdown("SET", setSprite);

        yield return new WaitForSeconds(goTextTime - setTextTime);

        ShowCountdown("GO!", goSprite);

        yield return new WaitForSeconds(countdownEndTime - goTextTime);

        SetCountdownPanelVisible(false);

        if (sfxSource != null)
            sfxSource.Stop();

        countdownCompleted = true;
        isCountingDown = false;
        countdownRoutine = null;
        SetGameplayEnabled(true);
    }

    void SetGameplayEnabled(bool enabledValue)
    {
        if (behavioursToEnableAfterCountdown == null)
            return;

        for (int i = 0; i < behavioursToEnableAfterCountdown.Length; i++)
        {
            if (behavioursToEnableAfterCountdown[i] != null)
                behavioursToEnableAfterCountdown[i].enabled = enabledValue;
        }
    }

    void ShowCountdown(string fallbackText, Sprite sprite)
    {
        if (countdownImage != null)
        {
            countdownImage.sprite = sprite;
            countdownImage.enabled = sprite != null;
        }

        if (countdownText != null)
        {
            countdownText.text = fallbackText;
            countdownText.enabled = countdownImage == null || sprite == null;
        }
    }

    void SetCountdownPanelVisible(bool visible)
    {
        if (countdownPanel == null)
            return;

        if (countdownPanel != gameObject)
        {
            countdownPanel.SetActive(visible);
            return;
        }

        Graphic panelGraphic = countdownPanel.GetComponent<Graphic>();
        if (panelGraphic != null)
            panelGraphic.enabled = visible;

        if (countdownImage != null)
            countdownImage.enabled = visible && countdownImage.sprite != null;

        if (countdownText != null)
            countdownText.enabled = visible && (countdownImage == null || countdownImage.sprite == null);
    }
}
