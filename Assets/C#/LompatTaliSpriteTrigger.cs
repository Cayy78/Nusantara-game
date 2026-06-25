using UnityEngine;

public class LompatTaliSpriteTrigger : MonoBehaviour
{
    public Collider2D ropeCollider;
    public SpriteRenderer ropeRenderer;
    public Animator ropeAnimator;
    public LompatTaliMultiManager manager;
    public LompatTaliSingleManager singleManager;
    public int frontSortingOrder = 10;
    public int backSortingOrder = -10;
    [Range(0f, 1f)] public float frontAlpha = 1f;
    [Range(0f, 1f)] public float backAlpha = 0.35f;
    [Min(0f)] public float ropeAnimationSpeed = 1f;

    void Awake()
    {
        CacheReferences();
        ResolveManagers();
        ApplyAnimationSpeed();
        SetBackState();
    }

    void OnEnable()
    {
        CacheReferences();
        ResolveManagers();
        ApplyAnimationSpeed();
        SetBackState();
    }

    public void EnableRopeTrigger()
    {
        ResolveManagers();
        SetFrontState();

        if (manager != null)
            manager.BeginRopeDangerPhase();

        if (singleManager != null)
            singleManager.BeginRopeDangerPhase();
    }

    public void DisableRopeTrigger()
    {
        ResolveManagers();

        if (manager != null)
            manager.EndRopeDangerPhase();

        if (singleManager != null)
            singleManager.EndRopeDangerPhase();

        SetBackState();
    }

    public void ShowFrontRope()
    {
        SetFrontState();
    }

    public void ShowBackRope()
    {
        SetBackState();
    }

    void CacheReferences()
    {
        if (ropeCollider == null)
            ropeCollider = GetComponent<Collider2D>();

        if (ropeRenderer == null)
            ropeRenderer = GetComponent<SpriteRenderer>();

        if (ropeAnimator == null)
            ropeAnimator = GetComponent<Animator>();
    }

    void ResolveManagers()
    {
        if (singleManager == null)
            singleManager = FindObjectOfType<LompatTaliSingleManager>();

        if (manager == null && singleManager == null)
            manager = FindObjectOfType<LompatTaliMultiManager>();
    }

    void ApplyAnimationSpeed()
    {
        if (ropeAnimator != null)
            ropeAnimator.speed = ropeAnimationSpeed;
    }

    void SetFrontState()
    {
        if (ropeCollider != null)
            ropeCollider.enabled = true;

        if (ropeRenderer != null)
        {
            ropeRenderer.sortingOrder = frontSortingOrder;
            Color color = ropeRenderer.color;
            color.a = frontAlpha;
            ropeRenderer.color = color;
        }
    }

    void SetBackState()
    {
        if (ropeCollider != null)
            ropeCollider.enabled = false;

        if (ropeRenderer != null)
        {
            ropeRenderer.sortingOrder = backSortingOrder;
            Color color = ropeRenderer.color;
            color.a = backAlpha;
            ropeRenderer.color = color;
        }
    }
}
