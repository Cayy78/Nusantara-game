using UnityEngine;

public class PlayerClimb : MonoBehaviour
{
    public float targetHeight = 10f;
    public bool lockSlipToStartPosition = true;
    public float minY;

    void Awake()
    {
        if (lockSlipToStartPosition)
            minY = transform.position.y;
    }

    public void Climb(float amount)
    {
        transform.position += new Vector3(0, amount, 0);
    }

    public void Slip(float amount)
    {
        float nextY = transform.position.y - amount;

        if (lockSlipToStartPosition)
            nextY = Mathf.Max(minY, nextY);

        transform.position = new Vector3(transform.position.x, nextY, transform.position.z);
    }

    public bool IsWin()
    {
        return transform.position.y >= targetHeight;
    }
}
