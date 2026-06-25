using UnityEngine;

public class TrailingPlayerCamera : MonoBehaviour
{
    public enum TrackingAxis
    {
        Vertical,
        Horizontal
    }

    public Transform player1Target;
    public Transform player2Target;
    public Camera targetCamera;
    public GameObject[] visibilityTargets;
    public GameObject[] player1VisibilityTargets;
    public GameObject[] player2VisibilityTargets;
    public TrackingAxis trackingAxis = TrackingAxis.Vertical;
    public Vector3 followOffset = new Vector3(0f, 0f, -10f);
    [Min(0f)] public float smoothTime = 0.15f;
    public bool hideWhenPlayersAreClose;
    [Min(0f)] public float showWhenGapExceeds = 1.5f;

    private Vector3 velocity;

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        Transform trailingTarget = GetTrailingTarget();
        bool shouldShow = trailingTarget != null && ShouldShowCamera();

        if (targetCamera != null)
            targetCamera.enabled = shouldShow;

        SetVisibilityTargetsActive(visibilityTargets, shouldShow);
        UpdatePlayerSpecificVisibility(trailingTarget, shouldShow);

        if (trailingTarget == null)
            return;

        Vector3 desiredPosition = trailingTarget.position + followOffset;

        if (smoothTime <= 0f)
        {
            transform.position = desiredPosition;
            return;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime);
    }

    Transform GetTrailingTarget()
    {
        if (player1Target == null && player2Target == null)
            return null;

        if (player1Target == null)
            return player2Target;

        if (player2Target == null)
            return player1Target;

        if (trackingAxis == TrackingAxis.Horizontal)
        {
            return player1Target.position.x <= player2Target.position.x
                ? player1Target
                : player2Target;
        }

        return player1Target.position.y <= player2Target.position.y
            ? player1Target
            : player2Target;
    }

    bool ShouldShowCamera()
    {
        if (!hideWhenPlayersAreClose || player1Target == null || player2Target == null)
            return true;

        float gap = trackingAxis == TrackingAxis.Horizontal
            ? Mathf.Abs(player1Target.position.x - player2Target.position.x)
            : Mathf.Abs(player1Target.position.y - player2Target.position.y);

        return gap >= showWhenGapExceeds;
    }

    void UpdatePlayerSpecificVisibility(Transform trailingTarget, bool shouldShow)
    {
        bool showPlayer1Targets = shouldShow && trailingTarget == player1Target;
        bool showPlayer2Targets = shouldShow && trailingTarget == player2Target;

        SetPlayerSpecificVisibility(player1VisibilityTargets, showPlayer1Targets);
        SetPlayerSpecificVisibility(player2VisibilityTargets, showPlayer2Targets);
    }

    void SetVisibilityTargetsActive(GameObject[] targets, bool shouldShow)
    {
        if (targets == null)
            return;

        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i];
            if (target != null)
                target.SetActive(shouldShow);
        }
    }

    void SetPlayerSpecificVisibility(GameObject[] targets, bool shouldShow)
    {
        if (targets == null)
            return;

        for (int i = 0; i < targets.Length; i++)
        {
            GameObject target = targets[i];
            if (target == null)
                continue;

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = shouldShow ? 1f : 0f;
                canvasGroup.interactable = shouldShow;
                canvasGroup.blocksRaycasts = shouldShow;
                continue;
            }

            target.SetActive(shouldShow);
        }
    }
}
