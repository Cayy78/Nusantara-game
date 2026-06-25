using UnityEngine;

public class CameraFollowVertical2D : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public float yOffset = 1.5f;
    public float xPosition = 0f;
    public float zPosition = -10f;
    public float startFollowY = 0f;

    void LateUpdate()
    {
        if (target == null)
            return;

        if (target.position.y < startFollowY)
            return;

        float targetY = target.position.y + yOffset;
        Vector3 targetPosition = new Vector3(xPosition, targetY, zPosition);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
