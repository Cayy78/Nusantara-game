using UnityEngine;
using UnityEngine.UI;

public class KelerengPowerBar : MonoBehaviour
{
    public Slider slider;
    public Image fillImage;
    public float speed = 1f;

    private bool isRunning;

    void Start()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

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

    public float GetCurrentValue()
    {
        if (slider == null)
            return 0f;

        return slider.value;
    }

    public void ResetBar()
    {
        if (slider != null)
            slider.value = 0f;

        isRunning = false;
    }
}
