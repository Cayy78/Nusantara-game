using UnityEngine;

[System.Serializable]
public class PlayerArrowIndicatorSettings
{
    public Color indicatorColor = Color.white;
    public float horizontalMargin = 0f;
    public float verticalMargin = -0.4f;
    public int sortingOrderOffset = 100;
    public Vector3 indicatorScale = new Vector3(0.75f, 0.75f, 1f);
}

[DisallowMultipleComponent]
public class PlayerArrowIndicator : MonoBehaviour
{
    public SpriteRenderer targetRenderer;
    public GameObject visibilityTarget;
    public PlayerArrowIndicatorSettings runtimeSettings;
    public Color indicatorColor = Color.white;
    public float horizontalMargin = 0f;
    public float verticalMargin = 0.24f;
    public int sortingOrderOffset = 100;
    public Vector3 indicatorScale = new Vector3(0.35f, 0.35f, 1f);

    SpriteRenderer indicatorRenderer;

    void Awake()
    {
        EnsureRenderer();
    }

    void LateUpdate()
    {
        EnsureRenderer();

        if (indicatorRenderer == null)
            return;

        ApplyRuntimeSettings();

        bool shouldShow = targetRenderer != null &&
                          targetRenderer.gameObject.activeInHierarchy &&
                          (visibilityTarget == null || visibilityTarget.activeInHierarchy);

        if (!shouldShow)
        {
            indicatorRenderer.enabled = false;
            return;
        }

        indicatorRenderer.enabled = true;
        indicatorRenderer.color = indicatorColor;
        indicatorRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        indicatorRenderer.sortingOrder = targetRenderer.sortingOrder + sortingOrderOffset;
        transform.localScale = indicatorScale;

        Bounds targetBounds = targetRenderer.bounds;
        transform.position = targetBounds.center +
                             Vector3.right * horizontalMargin +
                             Vector3.up * (targetBounds.extents.y + verticalMargin);
    }

    void EnsureRenderer()
    {
        if (indicatorRenderer == null)
            indicatorRenderer = GetComponent<SpriteRenderer>();

        if (indicatorRenderer == null)
            indicatorRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (indicatorRenderer.sprite == null)
            indicatorRenderer.sprite = PlayerArrowIndicatorUtility.GetArrowSprite();
    }

    void ApplyRuntimeSettings()
    {
        if (runtimeSettings == null)
            return;

        indicatorColor = runtimeSettings.indicatorColor;
        horizontalMargin = runtimeSettings.horizontalMargin;
        verticalMargin = runtimeSettings.verticalMargin;
        sortingOrderOffset = runtimeSettings.sortingOrderOffset;
        indicatorScale = runtimeSettings.indicatorScale;
    }
}

public static class PlayerArrowIndicatorUtility
{
    static Sprite arrowSprite;

    public static void EnsureIndicator(
        string indicatorName,
        SpriteRenderer targetRenderer,
        GameObject visibilityTarget,
        bool visible,
        PlayerArrowIndicatorSettings settings)
    {
        if (targetRenderer == null)
            return;

        PlayerArrowIndicator indicator = FindOrCreateIndicator(indicatorName, targetRenderer);
        if (indicator == null)
            return;

        indicator.targetRenderer = targetRenderer;
        indicator.visibilityTarget = visibilityTarget != null ? visibilityTarget : targetRenderer.gameObject;
        indicator.runtimeSettings = settings;
        indicator.indicatorColor = settings != null ? settings.indicatorColor : Color.white;
        indicator.horizontalMargin = settings != null ? settings.horizontalMargin : 0f;
        indicator.verticalMargin = settings != null ? settings.verticalMargin : 0.12f;
        indicator.sortingOrderOffset = settings != null ? settings.sortingOrderOffset : 100;
        indicator.indicatorScale = settings != null ? settings.indicatorScale : new Vector3(0.35f, 0.35f, 1f);
        indicator.gameObject.SetActive(visible);
    }

    public static Sprite GetArrowSprite()
    {
        if (arrowSprite != null)
            return arrowSprite;

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color fill = Color.white;
        int centerX = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
                texture.SetPixel(x, y, clear);
        }

        for (int y = 0; y < size; y++)
        {
            float normalizedY = y / (float)(size - 1);
            int halfWidth = Mathf.Max(1, Mathf.RoundToInt(normalizedY * (size * 0.35f)));

            for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
            {
                if (x >= 0 && x < size)
                    texture.SetPixel(x, y, fill);
            }
        }

        texture.Apply();

        arrowSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0f),
            size);

        arrowSprite.name = "GeneratedPlayerArrow";
        return arrowSprite;
    }

    static PlayerArrowIndicator FindOrCreateIndicator(string indicatorName, SpriteRenderer targetRenderer)
    {
        Transform parent = targetRenderer.transform.parent;
        Transform existing = parent != null ? parent.Find(indicatorName) : null;

        if (existing == null)
        {
            GameObject indicatorObject = new GameObject(indicatorName);
            if (parent != null)
                indicatorObject.transform.SetParent(parent, false);

            existing = indicatorObject.transform;
        }

        PlayerArrowIndicator indicator = existing.GetComponent<PlayerArrowIndicator>();
        if (indicator == null)
            indicator = existing.gameObject.AddComponent<PlayerArrowIndicator>();

        return indicator;
    }
}
