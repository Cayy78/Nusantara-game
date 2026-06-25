using UnityEngine;
using UnityEngine.UI;

public class PowerBar : MonoBehaviour
{
    public Slider slider;
    public Image backgroundImage;
    public Image fillImage;

    public float speed = 2f;
    public bool isHolding = false;

    [Header("Gradient Colors")]
    public Color lowColor = new Color(1f, 0.2f, 0.2f);
    public Color midColor = new Color(1f, 0.8f, 0.1f);
    public Color highColor = new Color(0.7f, 1f, 0.3f);

    [Header("Visuals")]
    [Range(0f, 1f)] public float fillOverlayAlpha = 0.35f;
    [Min(16)] public int gradientWidth = 256;
    [Min(4)] public int gradientHeight = 32;
    [Range(0.1f, 1f)] public float roundedEndRatio = 0.5f;
    [Min(3)] public int segmentCount = 18;
    [Min(1)] public int separatorWidth = 2;
    public Color separatorColor = new Color(0.08f, 0.08f, 0.08f, 1f);

    private Sprite generatedGradientSprite;

    void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (backgroundImage == null && slider != null)
            backgroundImage = FindChildImage("Background");

        if (fillImage == null && slider != null)
            fillImage = FindChildImage("Fill");

        ApplyVisuals();
    }

    void OnDestroy()
    {
        if (generatedGradientSprite != null)
        {
            if (generatedGradientSprite.texture != null)
                Destroy(generatedGradientSprite.texture);

            Destroy(generatedGradientSprite);
        }
    }

    void Update()
    {
        if (!isHolding || slider == null)
            return;

        slider.value += speed * Time.deltaTime;

        if (slider.value >= 1f)
            slider.value = 0f;
    }

    void ApplyVisuals()
    {
        if (backgroundImage != null)
            RefreshBackground();

        if (fillImage != null)
            fillImage.color = new Color(1f, 1f, 1f, fillOverlayAlpha);
    }

    void RefreshBackground()
    {
        if (backgroundImage == null)
            return;

        if (generatedGradientSprite != null)
        {
            if (generatedGradientSprite.texture != null)
                Destroy(generatedGradientSprite.texture);

            Destroy(generatedGradientSprite);
        }

        generatedGradientSprite = CreateGradientSprite();
        backgroundImage.sprite = generatedGradientSprite;
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.color = Color.white;
        backgroundImage.preserveAspect = false;
    }

    Sprite CreateGradientSprite()
    {
        Texture2D texture = new Texture2D(gradientWidth, gradientHeight, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        float radius = Mathf.Clamp(gradientHeight * roundedEndRatio, 1f, gradientHeight * 0.5f);
        Vector2 leftCenter = new Vector2(radius, gradientHeight * 0.5f);
        Vector2 rightCenter = new Vector2(gradientWidth - radius - 1f, gradientHeight * 0.5f);

        for (int y = 0; y < gradientHeight; y++)
        {
            for (int x = 0; x < gradientWidth; x++)
            {
                float t = gradientWidth <= 1 ? 0f : x / (gradientWidth - 1f);
                Color gradientColor = EvaluateGradient(t);
                gradientColor = ApplySegmentSeparators(x, gradientColor);

                bool insideCapsule = x >= leftCenter.x && x <= rightCenter.x;
                if (!insideCapsule)
                {
                    Vector2 point = new Vector2(x, y);
                    float leftDistance = Vector2.Distance(point, leftCenter);
                    float rightDistance = Vector2.Distance(point, rightCenter);
                    insideCapsule = leftDistance <= radius || rightDistance <= radius;
                }

                gradientColor.a = insideCapsule ? 1f : 0f;
                texture.SetPixel(x, y, gradientColor);
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }

    Color EvaluateGradient(float t)
    {
        if (t < 0.5f)
            return Color.Lerp(lowColor, midColor, t / 0.5f);

        return Color.Lerp(midColor, highColor, (t - 0.5f) / 0.5f);
    }

    Color ApplySegmentSeparators(int x, Color sourceColor)
    {
        if (segmentCount <= 1 || separatorWidth <= 0 || gradientWidth <= 1)
            return sourceColor;

        float segmentWidth = gradientWidth / (float)segmentCount;

        for (int i = 1; i < segmentCount; i++)
        {
            float separatorCenter = i * segmentWidth;
            if (Mathf.Abs(x - separatorCenter) <= separatorWidth * 0.5f)
                return separatorColor;
        }

        return sourceColor;
    }

    Image FindChildImage(string childName)
    {
        if (slider == null)
            return null;

        Transform child = slider.transform.Find(childName);
        if (child != null)
            return child.GetComponent<Image>();

        foreach (Image image in slider.GetComponentsInChildren<Image>(true))
        {
            if (image.name == childName)
                return image;
        }

        return null;
    }

    public float Release()
    {
        isHolding = false;
        if (slider == null)
            return 0f;

        float releasedValue = slider.value;
        slider.value = 0f;
        return releasedValue;
    }
}
