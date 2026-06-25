using UnityEngine;

public class BakiakRhythmMovement : MonoBehaviour
{
    [Header("Movement")]
    public float stepDistance = 0.3f;

    [Header("Component")]
    public Animator animator;

    private bool nextIsLeft = true;
    private float idleTimer;
    public float idleDelay = 0.1f;

    void Update()
    {
        // Hitung waktu tanpa input
        idleTimer += Time.deltaTime;

        // Kalau tidak ada input → balik idle
        if (animator != null && idleTimer >= idleDelay)
        {
            animator.SetBool("isWalking", false);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            Step(true);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Step(false);
        }
    }

    void Step(bool isLeft)
    {
        if (isLeft == nextIsLeft)
        {
            // MAJU
            transform.Translate(Vector2.right * stepDistance);

            // SET JALAN
            idleTimer = 0f;
            if (animator != null)
                animator.SetBool("isWalking", true);

            // Ganti kaki
            nextIsLeft = !nextIsLeft;
        }
        else
        {
            Debug.Log("Ritme salah");
        }
    }
}
