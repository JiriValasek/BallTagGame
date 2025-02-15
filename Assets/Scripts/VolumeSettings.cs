using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    // Player preferences keys
    /** PlayerPrefs key music volume in dB. */
    private const string MUSIC_VOLUME_KEY = "musicVolumeSetting";
    /** PlayerPrefs key SFX volume in dB. */
    private const string EFFECTS_VOLUME_KEY = "effectsVolumeSetting";

    // Old values for change reset
    /** Music volume in dB when settings were opened to be able to cancel. */
    private float oldMusicSliderValue = float.NaN;
    /** SFX volume in dB when settings were opened to be able to cancel. */
    private float oldEffectsSliderValue = float.NaN;


    private void Start()
    {
        // Try to load preferences and update sliders accordingly
        LoadVolume();
        // Init volumes by initial slider positions
        SetMusicVolume();
        SetEffectsVolume();
    }

    /// <summary>
    /// Function converting linear slider motion to logarithmic music volume scale,
    /// setting it to the music source and saving it to preferences.
    /// </summary>
    public void SetMusicVolume()
    {
        float volume = 0f;
        if (musicSlider != null)
        {
            volume = 20 * Mathf.Log10(musicSlider.value);
        }
        audioMixer.SetFloat("musicVolume", volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Function converting linear slider motion to logarithmic SFX volume scale,
    /// setting it to the SFX source and saving it to preferences.
    /// </summary>
    public void SetEffectsVolume()
    {
        float volume = 0f;
        if (effectsSlider != null)
        {
            volume = 20 * Mathf.Log10(effectsSlider.value);
        }
        audioMixer.SetFloat("effectsVolume", volume);
        PlayerPrefs.SetFloat(EFFECTS_VOLUME_KEY, volume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Function for stashing old preferences when audio setting are opened.
    /// </summary>
    public void StashVolumes()
    {
        if (musicSlider != null)
        {
            this.oldMusicSliderValue = musicSlider.value;
        }
        if (effectsSlider!= null)
        {
            this.oldEffectsSliderValue = effectsSlider.value;
        }
    }


    /// <summary>
    /// Function for restoring stashed preferences when audio setting chages are canceled.
    /// </summary>
    public void RestoreVolumes()
    {
        if (this.oldMusicSliderValue != float.NaN)
        {
             musicSlider.value = this.oldMusicSliderValue;
            SetMusicVolume();
        }
        if (this.oldEffectsSliderValue != float.NaN)
        {
            effectsSlider.value = this.oldEffectsSliderValue;
            SetEffectsVolume();
        }
    }

    /// <summary>
    /// Function loading music and SFX volume preferences and updating sliders if available.
    /// </summary>
    private void LoadVolume()
    {
        if (PlayerPrefs.HasKey(MUSIC_VOLUME_KEY))
        {
            audioMixer.SetFloat("musicVolume", PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY));
            if (musicSlider != null )
            {
                float sliderPosition = Mathf.Pow(10,(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY) / 20));
                sliderPosition = Mathf.Clamp(sliderPosition, musicSlider.minValue, musicSlider.maxValue);
                musicSlider.value = sliderPosition;
            }
        }

        if (PlayerPrefs.HasKey(EFFECTS_VOLUME_KEY))
        {
            audioMixer.SetFloat("effectsVolume", PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY));
            if (effectsSlider != null)
            {
                float sliderPosition = Mathf.Pow(10,(PlayerPrefs.GetFloat(EFFECTS_VOLUME_KEY) / 20));
                sliderPosition = Mathf.Clamp(sliderPosition, effectsSlider.minValue, effectsSlider.maxValue);
                effectsSlider.value = sliderPosition;
            }

        }

    }
}
