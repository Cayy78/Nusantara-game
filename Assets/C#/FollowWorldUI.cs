using UnityEngine;

public class FollowWorldUI : MonoBehaviour
{
    public Transform target;
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    public Camera mainCamera;

    void LateUpdate()
    {
        if (target == null || mainCamera == null)
            return;

        Vector3 worldPosition = target.position + worldOffset;
        transform.position = mainCamera.WorldToScreenPoint(worldPosition);
    }
}
