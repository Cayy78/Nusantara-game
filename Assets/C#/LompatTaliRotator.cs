using UnityEngine;

public class LompatTaliRotator : MonoBehaviour
{
    public float rotationSpeed = 240f;
    public float startAngle = 90f;
    public bool rotateClockwise = true;
    public Transform ropeVisual;
    public Collider2D ropeCollider;
    public SpriteRenderer ropeRenderer;
    public float topY = 2f;
    public float bottomY = -2f;
    public int frontSortingOrder = 10;
    public int backSortingOrder = -10;
    [Range(0f, 1f)] public float frontAlpha = 1f;
    [Range(0f, 1f)] public float backAlpha = 0.35f;

    private float phase;
    private Vector3 ropeStartLocalPosition;

    void Start()
    {
        if (ropeVisual == null)
            ropeVisual = transform.childCount > 0 ? transform.GetChild(0) : transform;

        if (ropeCollider == null)
            ropeCollider = GetComponentInChildren<Collider2D>();

        if (ropeRenderer == null)
            ropeRenderer = GetComponentInChildren<SpriteRenderer>();

        ropeStartLocalPosition = ropeVisual.localPosition;
        phase = startAngle * Mathf.Deg2Rad;
        UpdateRopeVisual();
    }

    void Update()
    {
        float direction = rotateClockwise ? -1f : 1f;
        phase += direction * rotationSpeed * Mathf.Deg2Rad * Time.deltaTime;
        UpdateRopeVisual();
    }

    void UpdateRopeVisual()
    {
        if (ropeVisual == null)
            return;

        float normalizedHeight = (Mathf.Sin(phase) + 1f) * 0.5f;

        Vector3 localPosition = ropeStartLocalPosition;
        localPosition.y = Mathf.Lerp(bottomY, topY, normalizedHeight);
        ropeVisual.localPosition = localPosition;
        ropeVisual.localRotation = Quaternion.identity;

        bool isFrontHalf = IsDescendingInFront();

        if (ropeRenderer != null)
        {
            ropeRenderer.sortingOrder = isFrontHalf ? frontSortingOrder : backSortingOrder;
            Color color = ropeRenderer.color;
            color.a = isFrontHalf ? frontAlpha : backAlpha;
            ropeRenderer.color = color;
        }

        if (ropeCollider != null)
            ropeCollider.enabled = isFrontHalf;
    }

    bool IsDescendingInFront()
    {
        float direction = rotateClockwise ? -1f : 1f;
        float verticalVelocity = Mathf.Cos(phase) * direction;
        return verticalVelocity < 0f;
    }
}
