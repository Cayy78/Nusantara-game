using UnityEngine;

public class TarikTambangCameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 5f;
    public float fixedY = 0f;
    public float zPosition = -10f;

    [Header("Follow Limits")]
    public float startFollowLeftX = -2f;
    public float startFollowRightX = 2f;

    [Header("Offset")]
    public float offsetX = 0f;

    void LateUpdate()
    {
        if (target == null)
            return;

        float targetX = transform.position.x;

        if (target.position.x < startFollowLeftX)
        {
            targetX = target.position.x + offsetX;
        }
        else if (target.position.x > startFollowRightX)
        {
            targetX = target.position.x + offsetX;
        }
        else
        {
            return;
        }

        Vector3 desiredPosition = new Vector3(targetX, fixedY, zPosition);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
