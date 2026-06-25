using UnityEngine;

public class ViewportContainerFollower : MonoBehaviour
{
    public Transform worldTarget;
    public Camera sourceCamera;
    public RectTransform viewportContainer;
    public RectTransform targetRectTransform;
    public CanvasGroup targetCanvasGroup;
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f);
    public Vector2 containerOffset;
    public bool hideWhenTargetIsOutsideView = true;

    void Awake()
    {
        CacheReferences();
    }

    void LateUpdate()
    {
        CacheReferences();

        if (worldTarget == null || sourceCamera == null || viewportContainer == null || targetRectTransform == null)
            return;

        Vector3 viewportPoint = sourceCamera.WorldToViewportPoint(worldTarget.position + worldOffset);
        bool isVisible = viewportPoint.z > 0f &&
                         viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
                         viewportPoint.y >= 0f && viewportPoint.y <= 1f;

        if (hideWhenTargetIsOutsideView)
            SetVisible(isVisible);

        if (!isVisible)
            return;

        float x = (viewportPoint.x - 0.5f) * viewportContainer.rect.width;
        float y = (viewportPoint.y - 0.5f) * viewportContainer.rect.height;
        targetRectTransform.anchoredPosition = new Vector2(x, y) + containerOffset;
    }

    void CacheReferences()
    {
        if (targetRectTransform == null)
            targetRectTransform = transform as RectTransform;

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
