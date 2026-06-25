using UnityEngine;

public class EgrangDifficultySetup : MonoBehaviour
{
    public GameObject trackEasy;
    public GameObject trackMedium;
    public GameObject trackHard;

    public GameObject finishEasy;
    public GameObject finishMedium;
    public GameObject finishHard;

    void Start()
    {
        SetAllInactive();

        if (GameSelectionManager.Instance == null)
        {
            ActivateMediumAsDefault();
            return;
        }

        switch (GameSelectionManager.Instance.selectedDifficulty)
        {
            case DifficultyLevel.Easy:
                ActivateEasy();
                break;

            case DifficultyLevel.Medium:
                ActivateMedium();
                break;

            case DifficultyLevel.Hard:
                ActivateHard();
                break;
        }
    }

    void SetAllInactive()
    {
        if (trackEasy != null) trackEasy.SetActive(false);
        if (trackMedium != null) trackMedium.SetActive(false);
        if (trackHard != null) trackHard.SetActive(false);

        if (finishEasy != null) finishEasy.SetActive(false);
        if (finishMedium != null) finishMedium.SetActive(false);
        if (finishHard != null) finishHard.SetActive(false);
    }

    void ActivateEasy()
    {
        if (trackEasy != null) trackEasy.SetActive(true);
        if (finishEasy != null) finishEasy.SetActive(true);
    }

    void ActivateMedium()
    {
        if (trackMedium != null) trackMedium.SetActive(true);
        if (finishMedium != null) finishMedium.SetActive(true);
    }

    void ActivateHard()
    {
        if (trackHard != null) trackHard.SetActive(true);
        if (finishHard != null) finishHard.SetActive(true);
    }

    void ActivateMediumAsDefault()
    {
        if (trackMedium != null) trackMedium.SetActive(true);
        if (finishMedium != null) finishMedium.SetActive(true);
    }
}
