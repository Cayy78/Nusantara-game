using UnityEngine;

public class LemparSandalProjectile : MonoBehaviour
{
    public int ownerPlayerIndex;
    public LemparSandalMultiManager manager;
    public KelerengMultiManager kelerengManager;
    public LemparSandalSingleManager singleManager;
    public KelerengSingleManager kelerengSingleManager;
    public float lifeTime = 5f;

    private bool hasResolved;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasResolved)
            return;

        if (IsTarget(other))
        {
            ResolveProjectile(true);
            return;
        }

        if (IsBlockingSurface(other))
        {
            ResolveProjectile(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasResolved)
            return;

        Collider2D other = collision.collider;

        if (IsTarget(other))
        {
            ResolveProjectile(true);
            return;
        }

        if (IsBlockingSurface(other))
            ResolveProjectile(false);
    }

    void OnDestroy()
    {
        if (!hasResolved && manager != null)
            manager.NotifyProjectileFinished(ownerPlayerIndex);

        if (!hasResolved && kelerengManager != null)
            kelerengManager.NotifyProjectileFinished(ownerPlayerIndex);

        if (!hasResolved && singleManager != null)
            singleManager.NotifyProjectileFinished(ownerPlayerIndex);

        if (!hasResolved && kelerengSingleManager != null)
            kelerengSingleManager.NotifyProjectileFinished(ownerPlayerIndex);
    }

    void ResolveProjectile(bool hitTarget)
    {
        hasResolved = true;

        if (manager != null)
        {
            if (hitTarget)
                manager.RegisterHit(ownerPlayerIndex);

            manager.NotifyProjectileFinished(ownerPlayerIndex);
        }

        if (kelerengManager != null)
        {
            if (hitTarget)
                kelerengManager.RegisterHit(ownerPlayerIndex);

            kelerengManager.NotifyProjectileFinished(ownerPlayerIndex);
        }

        if (singleManager != null)
        {
            if (hitTarget)
                singleManager.RegisterHit(ownerPlayerIndex);

            singleManager.NotifyProjectileFinished(ownerPlayerIndex);
        }

        if (kelerengSingleManager != null)
        {
            if (hitTarget)
                kelerengSingleManager.RegisterHit(ownerPlayerIndex);

            kelerengSingleManager.NotifyProjectileFinished(ownerPlayerIndex);
        }

        Destroy(gameObject);
    }

    bool IsTarget(Collider2D other)
    {
        return other != null && other.CompareTag("LemparSandalTarget");
    }

    bool IsBlockingSurface(Collider2D other)
    {
        return other != null && (other.CompareTag("Ground") || other.CompareTag("Obstacle"));
    }
}
