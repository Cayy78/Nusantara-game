using UnityEngine;

public class LemparSandalTargetMover : MonoBehaviour
{
    private Coroutine moveDelayCoroutine;
    private Renderer[] cachedRenderers;
    private Collider2D[] cachedColliders;

    void Awake()
    {
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        cachedColliders = GetComponentsInChildren<Collider2D>(true);
    }

    public void SetTargetX(float newX, float delay = 0f)
    {
        if (moveDelayCoroutine != null)
        {
            StopCoroutine(moveDelayCoroutine);
            moveDelayCoroutine = null;
        }

        if (delay <= 0f)
        {
            SetTargetVisible(true);
            ApplyTargetX(newX);
            return;
        }

        moveDelayCoroutine = StartCoroutine(HideMoveShowAfterDelay(newX, delay));
    }

    void ApplyTargetX(float newX)
    {
        Vector3 position = transform.position;
        position.x = newX;
        transform.position = position;
    }

    void SetTargetVisible(bool visible)
    {
        if (cachedRenderers != null)
        {
            foreach (Renderer targetRenderer in cachedRenderers)
            {
                if (targetRenderer != null)
                    targetRenderer.enabled = visible;
            }
        }

        if (cachedColliders != null)
        {
            foreach (Collider2D targetCollider in cachedColliders)
            {
                if (targetCollider != null)
                    targetCollider.enabled = visible;
            }
        }
    }

    System.Collections.IEnumerator HideMoveShowAfterDelay(float newX, float delay)
    {
        SetTargetVisible(false);
        yield return new WaitForSeconds(delay);
        ApplyTargetX(newX);
        SetTargetVisible(true);
        moveDelayCoroutine = null;
    }
}
