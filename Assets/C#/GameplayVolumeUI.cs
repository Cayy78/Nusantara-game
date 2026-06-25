using UnityEngine;
using UnityEngine.UI;

public class GameplayVolumeUI : MonoBehaviour
{
    public Slider volumeSlider;
    public Image muteToggleImage;
    public Sprite speakerOnSprite;
    public Sprite speakerOffSprite;
    public bool applyVolumeImmediately;

    void Start()
    {
        if (volumeSlider == null)
            volumeSlider = GetComponent<Slider>();

        if (volumeSlider == null)
            return;

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        float currentVolume = PauseMenuUI.GetSavedGameplayVolume();
        volumeSlider.value = currentVolume;
        volumeSlider.onValueChanged.AddListener(SetGameplayVolume);
        UpdateMuteToggleVisual(currentVolume);
    }

    public void SetGameplayVolume(float volume)
    {
        if (volume > 0f)
            PauseMenuUI.SaveLastNonZeroGameplayVolume(volume);

        PauseMenuUI.SaveGameplayVolume(volume);

        if (applyVolumeImmediately)
            PauseMenuUI.ApplyGameplayVolume(volume);

        UpdateMuteToggleVisual(volume);
    }

    public void ToggleMuteGameplayVolume()
    {
        float currentVolume = PauseMenuUI.GetSavedGameplayVolume();

        if (currentVolume > 0.0001f)
        {
            PauseMenuUI.SaveLastNonZeroGameplayVolume(currentVolume);
            SetGameplayVolume(0f);
            return;
        }

        float restoredVolume = PauseMenuUI.GetLastNonZeroGameplayVolume();
        SetGameplayVolume(restoredVolume);
    }

    void UpdateMuteToggleVisual(float volume)
    {
        if (muteToggleImage != null)
        {
            bool isMuted = volume <= 0.0001f;

            if (isMuted && speakerOffSprite != null)
                muteToggleImage.sprite = speakerOffSprite;
            else if (!isMuted && speakerOnSprite != null)
                muteToggleImage.sprite = speakerOnSprite;
        }

        if (volumeSlider != null && !Mathf.Approximately(volumeSlider.value, volume))
            volumeSlider.SetValueWithoutNotify(volume);
    }
}
