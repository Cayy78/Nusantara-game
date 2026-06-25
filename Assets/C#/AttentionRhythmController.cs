using UnityEngine;

public class AttentionRhythmController : MonoBehaviour
{
    [Header("Fallback Indicator")]
    public GameObject greenCircle;

    [Header("Per Player Indicators")]
    public GameObject player1Indicator;
    public GameObject player2Indicator;
    public Transform player1Target;
    public Transform player2Target;
    public Vector3 indicatorOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Timing")]
    public float interval = 1f;
    public float activeDuration = 0.4f;

    public bool IsInputWindowOpen { get; private set; }

    private float timer;
    private bool waitingForClose;

    void Start()
    {
        SetIndicatorsActive(false);
        UpdateIndicatorPositions();
    }

    void Update()
    {
        timer += Time.deltaTime;
        UpdateIndicatorPositions();

        if (!waitingForClose && timer >= interval)
        {
            OpenWindow();
        }
        else if (waitingForClose && timer >= activeDuration)
        {
            CloseWindow();
        }
    }

    void OpenWindow()
    {
        IsInputWindowOpen = true;
        waitingForClose = true;
        timer = 0f;
        SetIndicatorsActive(true);
    }

    void CloseWindow()
    {
        IsInputWindowOpen = false;
        waitingForClose = false;
        timer = 0f;
        SetIndicatorsActive(false);
    }

    void SetIndicatorsActive(bool isActive)
    {
        if (greenCircle != null)
            greenCircle.SetActive(isActive);

        if (player1Indicator != null)
            player1Indicator.SetActive(isActive);

        if (player2Indicator != null)
            player2Indicator.SetActive(isActive);
    }

    void UpdateIndicatorPositions()
    {
        if (player1Indicator != null && player1Target != null)
            player1Indicator.transform.position = player1Target.position + indicatorOffset;

        if (player2Indicator != null && player2Target != null)
            player2Indicator.transform.position = player2Target.position + indicatorOffset;
    }
}
