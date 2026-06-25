using UnityEngine;

public class PlayerStep : MonoBehaviour
{
    public float targetDistance = 10f;

    public void StepForward(float amount)
    {
        transform.position += new Vector3(amount, 0f, 0f);
    }

    public bool IsWin()
    {
        return transform.position.x >= targetDistance;
    }
}
