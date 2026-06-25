using System.Collections;
using UnityEngine;

public class LompatTaliPlayerJumper : MonoBehaviour
{
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;
    public float hitStunDuration = 0.5f;

    private float groundY;
    private Coroutine jumpRoutine;
    private Coroutine hitRoutine;
    private bool canJump = true;

    public bool IsJumping => jumpRoutine != null;

    void Start()
    {
        groundY = transform.position.y;
    }

    public void TryJump()
    {
        if (!canJump || jumpRoutine != null)
            return;

        jumpRoutine = StartCoroutine(JumpRoutine());
    }

    public void OnHit()
    {
        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        if (jumpRoutine != null)
        {
            StopCoroutine(jumpRoutine);
            jumpRoutine = null;
        }

        Vector3 position = transform.position;
        position.y = groundY;
        transform.position = position;

        hitRoutine = StartCoroutine(HitRoutine());
    }

    public void ResetForNextRound()
    {
        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
            hitRoutine = null;
        }

        if (jumpRoutine != null)
        {
            StopCoroutine(jumpRoutine);
            jumpRoutine = null;
        }

        Vector3 position = transform.position;
        position.y = groundY;
        transform.position = position;
        canJump = true;
    }

    IEnumerator JumpRoutine()
    {
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / jumpDuration);
            float arcHeight = 4f * jumpHeight * normalizedTime * (1f - normalizedTime);

            Vector3 position = startPosition;
            position.y = groundY + arcHeight;
            transform.position = position;

            yield return null;
        }

        Vector3 finalPosition = transform.position;
        finalPosition.y = groundY;
        transform.position = finalPosition;
        jumpRoutine = null;
    }

    IEnumerator HitRoutine()
    {
        canJump = false;
        yield return new WaitForSeconds(hitStunDuration);
        canJump = true;
        hitRoutine = null;
    }
}
