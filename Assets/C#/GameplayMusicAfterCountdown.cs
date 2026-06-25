using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameplayMusicAfterCountdown : MonoBehaviour
{
    public GameplayStartCountdown gameplayStartCountdown;
    public AudioSource targetAudioSource;
    public bool playIfCountdownMissing = true;

    Coroutine waitRoutine;

    void Awake()
    {
        if (targetAudioSource == null)
            targetAudioSource = GetComponent<AudioSource>();

        if (gameplayStartCountdown == null)
            gameplayStartCountdown = FindFirstObjectByType<GameplayStartCountdown>();
    }

    void OnEnable()
    {
        if (targetAudioSource == null)
            return;

        if (gameplayStartCountdown == null)
        {
            if (playIfCountdownMissing && !targetAudioSource.isPlaying)
                targetAudioSource.Play();

            return;
        }

        targetAudioSource.Stop();

        if (waitRoutine != null)
            StopCoroutine(waitRoutine);

        waitRoutine = StartCoroutine(WaitForCountdownThenPlay());
    }

    void OnDisable()
    {
        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }
    }

    IEnumerator WaitForCountdownThenPlay()
    {
        while (gameplayStartCountdown != null && !gameplayStartCountdown.HasCountdownCompleted())
            yield return null;

        waitRoutine = null;

        if (targetAudioSource == null || targetAudioSource.isPlaying)
            yield break;

        targetAudioSource.Play();
    }
}
