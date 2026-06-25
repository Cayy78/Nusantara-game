using UnityEngine;

public class CameraViewportUIFollower : MonoBehaviour
{
    public Transform worldTarget;
    public Camera sourceCamera;
    public Canvas targetCanvas;
    public RectTransform targetRectTransform;
    public CanvasGroup targetCanvasGroup;
    public Vector3 worldOffset = new Vector3(0f, 2f, 0f);
    public Vector2 canvasOffset;
    public bool hideWhenTargetIsOutsideView = true;

    void Awake()
    {
        CacheReferences();
    }

    void LateUpdate()
    {
        CacheReferences();

        if (worldTarget == null || sourceCamera == null || targetCanvas == null || targetRectTransform == null)
            return;

        Vector3 viewportPoint = sourceCamera.WorldToViewportPoint(worldTarget.position + worldOffset);
        bool isVisible = viewportPoint.z > 0f &&
                         viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
                         viewportPoint.y >= 0f && viewportPoint.y <= 1f;

        if (hideWhenTargetIsOutsideView)
            SetVisible(isVisible);

        if (!isVisible)
            return;

        RectTransform canvasRect = targetCanvas.transform as RectTransform;
        if (canvasRect == null)
            return;

        Camera canvasCamera = targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : targetCanvas.worldCamera;

        Vector2 screenPoint = new Vector2(
            viewportPoint.x * sourceCamera.pixelWidth,
            viewportPoint.y * sourceCamera.pixelHeight);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvasCamera,
            out Vector2 localPoint))
        {
            targetRectTransform.anchoredPosition = localPoint + canvasOffset;
        }
    }

    void CacheReferences()
    {
        if (targetRectTransform == null)
            targetRectTransform = transform as RectTransform;

        if (targetCanvas == null)
            targetCanvas = GetComponentInParent<Canvas>();

        if (targetCanvasGroup == null)
            targetCanvasGroup = GetComponent<CanvasGroup>();
    }

    void SetVisible(bool visible)
    {
        if (targetCanvasGroup == null)
            return;

        targetCanvasGroup.alpha = visible ? 1f : 0f;
        targetCanvasGroup.interactable = visible;
        targetCanvasGroup.blocksRaycasts = visible;
    }
}
