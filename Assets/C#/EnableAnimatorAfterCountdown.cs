using UnityEngine;

public class EnableAnimatorAfterCountdown : MonoBehaviour
{
    public Animator targetAnimator;
    public float resumeAnimatorSpeed = 1f;
    private bool shouldEnableAnimatorOnNextEnable;
    private LompatTaliSpriteTrigger ropeTrigger;

    void Awake()
    {
        CacheAnimator();
        CacheDependencies();
        PauseAnimatorAtStart();
        shouldEnableAnimatorOnNextEnable = false;
    }

    void OnEnable()
    {
        CacheAnimator();
        CacheDependencies();

        if (targetAnimator != null && shouldEnableAnimatorOnNextEnable)
            targetAnimator.speed = GetResumeSpeed();
    }

    void OnDisable()
    {
        if (targetAnimator != null)
            targetAnimator.speed = 0f;

        shouldEnableAnimatorOnNextEnable = true;
    }

    void CacheAnimator()
    {
        if (targetAnimator == null)
            targetAnimator = GetComponent<Animator>();
    }

    void CacheDependencies()
    {
        if (ropeTrigger == null)
            ropeTrigger = GetComponent<LompatTaliSpriteTrigger>();
    }

    float GetResumeSpeed()
    {
        if (ropeTrigger != null)
            return ropeTrigger.ropeAnimationSpeed;

        return resumeAnimatorSpeed;
    }

    void PauseAnimatorAtStart()
    {
        if (targetAnimator == null)
            return;

        targetAnimator.speed = 0f;
        targetAnimator.Play(0, 0, 0f);
        targetAnimator.Update(0f);
    }
}
