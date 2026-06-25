using UnityEngine;

public class MakanKerupukSwing : MonoBehaviour
{
    public float minAngle = -45f;
    public float maxAngle = 45f;
    public float swingSpeed = 90f;
    public bool startMovingToMax = true;

    private float currentAngle;
    private int direction;
    private float pauseUntilTime;

    void Start()
    {
        currentAngle = transform.localEulerAngles.z;

        if (currentAngle > 180f)
            currentAngle -= 360f;

        direction = startMovingToMax ? 1 : -1;
        ApplyRotation();
    }

    void Update()
    {
        if (Time.time < pauseUntilTime)
            return;

        currentAngle += direction * swingSpeed * Time.deltaTime;

        if (currentAngle >= maxAngle)
        {
            currentAngle = maxAngle;
            direction = -1;
        }
        else if (currentAngle <= minAngle)
        {
            currentAngle = minAngle;
            direction = 1;
        }

        ApplyRotation();
    }

    public void PauseSwing(float duration)
    {
        if (duration <= 0f)
            return;

        pauseUntilTime = Mathf.Max(pauseUntilTime, Time.time + duration);
    }

    void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }
}
