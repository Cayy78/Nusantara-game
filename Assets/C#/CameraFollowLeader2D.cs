using UnityEngine;

public class CameraFollowLeader2D : MonoBehaviour
{
    public enum FollowMode
    {
        Horizontal,
        Vertical,
        Both
    }

    public Transform player1;
    public Transform player2;

    public FollowMode followMode = FollowMode.Horizontal;
    public float smoothSpeed = 5f;
    public Vector2 offset;
    public float fixedX = 0f;
    public float fixedY = 0f;
    public float zPosition = -10f;
    public float startFollowX = 0f;
    public float startFollowY = 0f;

    void LateUpdate()
    {
        Transform leader = GetLeader();
        if (leader == null)
            return;

        float targetX = fixedX;
        float targetY = fixedY;

        if (followMode == FollowMode.Horizontal)
        {
            if (leader.position.x < startFollowX)
                return;

            targetX = leader.position.x + offset.x;
            targetY = fixedY;
        }
        else if (followMode == FollowMode.Vertical)
        {
            if (leader.position.y < startFollowY)
                return;

            targetX = fixedX;
            targetY = leader.position.y + offset.y;
        }
        else
        {
            if (leader.position.x < startFollowX && leader.position.y < startFollowY)
                return;

            targetX = leader.position.x + offset.x;
            targetY = leader.position.y + offset.y;
        }

        Vector3 targetPosition = new Vector3(targetX, targetY, zPosition);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

    Transform GetLeader()
    {
        if (player1 == null && player2 == null)
            return null;

        if (player1 != null && (player2 == null || !player2.gameObject.activeInHierarchy))
            return player1;

        if (player2 != null && (player1 == null || !player1.gameObject.activeInHierarchy))
            return player2;

        if (followMode == FollowMode.Vertical)
            return player1.position.y >= player2.position.y ? player1 : player2;

        return player1.position.x >= player2.position.x ? player1 : player2;
    }
}
