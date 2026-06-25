using UnityEngine;
using UnityEngine.UI;

public class MenuMusicVolumeUI : MonoBehaviour
{
    const string MenuMusicVolumePlayerPrefsKey = "MenuMusicVolume";
    const string LastNonZeroMenuMusicVolumePlayerPrefsKey = "LastNonZeroMenuMusicVolume";

    public Slider volumeSlider;
    public Image muteToggleImage;
    public Sprite speakerOnSprite;
    public Sprite speakerOffSprite;

    void Start()
    {
        if (volumeSlider == null)
            volumeSlider = GetComponent<Slider>();

        if (volumeSlider == null)
            return;

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;

        float currentVolume = MenuMusicManager.Instance != null
            ? MenuMusicManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat(MenuMusicVolumePlayerPrefsKey, 1f);

        volumeSlider.value = currentVolume;
        volumeSlider.onValueChanged.AddListener(SetMenuMusicVolume);
        UpdateMuteToggleVisual(currentVolume);
    }

    public void SetMenuMusicVolume(float volume)
    {
        if (volume > 0f)
            SaveLastNonZeroMenuMusicVolume(volume);

        if (MenuMusicManager.Instance != null)
            MenuMusicManager.Instance.SetVolume(volume);
        else
        {
            PlayerPrefs.SetFloat(MenuMusicVolumePlayerPrefsKey, volume);
            PlayerPrefs.Save();
        }

        UpdateMuteToggleVisual(volume);
    }

    public void ToggleMuteMenuMusicVolume()
    {
        float currentVolume = MenuMusicManager.Instance != null
            ? MenuMusicManager.Instance.GetVolume()
            : PlayerPrefs.GetFloat(MenuMusicVolumePlayerPrefsKey, 1f);

        if (currentVolume > 0.0001f)
        {
            SaveLastNonZeroMenuMusicVolume(currentVolume);
            SetMenuMusicVolume(0f);
            return;
        }

        float restoredVolume = GetLastNonZeroMenuMusicVolume();
        SetMenuMusicVolume(restoredVolume);
    }

    float GetLastNonZeroMenuMusicVolume()
    {
        return PlayerPrefs.GetFloat(LastNonZeroMenuMusicVolumePlayerPrefsKey, 1f);
    }

    void SaveLastNonZeroMenuMusicVolume(float volume)
    {
        if (volume <= 0f)
            return;

        PlayerPrefs.SetFloat(LastNonZeroMenuMusicVolumePlayerPrefsKey, volume);
        PlayerPrefs.Save();
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
