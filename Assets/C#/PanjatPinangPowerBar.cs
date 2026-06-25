using UnityEngine;
using UnityEngine.UI;

public class PanjatPinangPowerBar : MonoBehaviour
{
    public enum PowerZone
    {
        Red,
        Yellow,
        Green
    }

    public Slider slider;
    public Image backgroundImage;
    public Image fillImage;

    public float speed = 1f;
    public bool isRunning;

    [Header("Bar Colors")]
    public Color leftColor = Color.red;
    public Color middleColor = Color.yellow;
    public Color rightColor = Color.green;

    [Header("Segments")]
    public int segmentCount = 18;
    public int textureWidth = 512;
    public int textureHeight = 48;
    public int separatorWidth = 2;
    public Color separatorColor = Color.black;

    [Header("Zone Thresholds")]
    [Range(0f, 1f)] public float redMax = 0.3f;
    [Range(0f, 1f)] public float yellowMax = 0.7f;

    [Header("Step Values")]
    public float redStepAmount = 0f;
    public float yellowStepAmount = 0.5f;
    public float greenStepAmount = 1f;

    private Sprite generatedSprite;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        CreateBarVisual();
        ResetBar();
    }

    void Update()
    {
        if (!isRunning || slider == null)
            return;

        slider.value += speed * Time.deltaTime;

        if (slider.value >= 1f)
    slider.value = 0f;

    }

    void CreateBarVisual()
    {
        if (backgroundImage == null)
            return;

        Texture2D texture = new Texture2D(textureWidth, textureHeight);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int x = 0; x < textureWidth; x++)
        {
            float t = x / (float)(textureWidth - 1);
            Color barColor = GetGradientColor(t);

            if (IsSeparatorPixel(x))
                barColor = separatorColor;

            for (int y = 0; y < textureHeight; y++)
            {
                texture.SetPixel(x, y, barColor);
            }
        }

        texture.Apply();

        generatedSprite = Sprite.Create(
            texture,
            new Rect(0, 0, textureWidth, textureHeight),
            new Vector2(0.5f, 0.5f)
        );

        backgroundImage.sprite = generatedSprite;
        backgroundImage.type = Image.Type.Simple;
        backgroundImage.color = Color.white;
    }

   Color GetGradientColor(float t)
{
    if (t <= redMax)
        return leftColor;

    if (t <= yellowMax)
        return middleColor;

    return rightColor;
}


    bool IsSeparatorPixel(int x)
    {
        if (segmentCount <= 1)
            return false;

        float segmentWidth = textureWidth / (float)segmentCount;

        for (int i = 1; i < segmentCount; i++)
        {
            float separatorX = i * segmentWidth;
            if (Mathf.Abs(x - separatorX) <= separatorWidth)
                return true;
        }

        return false;
    }

    public void StartBar()
    {
        isRunning = true;
    }

    public float StopBar()
    {
        isRunning = false;

        if (slider == null)
            return 0f;

        return slider.value;
    }

    public float GetStepAmount(float value)
    {
        PowerZone zone = GetZone(value);

        switch (zone)
        {
            case PowerZone.Green:
                return greenStepAmount;
            case PowerZone.Yellow:
                return yellowStepAmount;
            default:
                return redStepAmount;
        }
    }

    public PowerZone GetCurrentZone()
    {
        if (slider == null)
            return PowerZone.Red;

        return GetZone(slider.value);
    }

    public PowerZone GetZone(float value)
    {
        if (value <= redMax)
            return PowerZone.Red;

        if (value <= yellowMax)
            return PowerZone.Yellow;

        return PowerZone.Green;
    }

    public void ResetBar()
    {
        if (slider != null)
            slider.value = 0f;

        isRunning = false;
    }
}
