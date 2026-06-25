using UnityEngine;

public class LemparSandalMovingTarget : MonoBehaviour
{
    public float leftLimit = 6f;
    public float rightLimit = 10f;
    public float speed = 1.5f;

    private int direction = 1;

    void Update()
    {
        Vector3 position = transform.position;
        position.x += direction * speed * Time.deltaTime;

        if (position.x >= rightLimit)
        {
            position.x = rightLimit;
            direction = -1;
        }
        else if (position.x <= leftLimit)
        {
            position.x = leftLimit;
            direction = 1;
        }

        transform.position = position;
    }
}
