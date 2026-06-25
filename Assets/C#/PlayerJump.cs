using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float targetDistance = 10f;
    public bool lockBackwardToStartPosition = true;
    public float minX;

    void Awake()
    {
        if (lockBackwardToStartPosition)
            minX = transform.position.x;
    }

    public void JumpForward(float amount)
    {
        transform.position += new Vector3(amount, 0f, 0f);
    }

    public void SlipBackward(float amount)
    {
        float nextX = transform.position.x - amount;

        if (lockBackwardToStartPosition)
            nextX = Mathf.Max(minX, nextX);

        transform.position = new Vector3(nextX, transform.position.y, transform.position.z);
    }

    public bool IsWin()
    {
        return transform.position.x >= targetDistance;
    }
}
