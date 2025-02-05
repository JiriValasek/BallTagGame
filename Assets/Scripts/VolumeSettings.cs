using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    // Keys for PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "musicVolumeSetting";
    private const string EFFECTS_VOLUME_KEY = "effectsVolumeSetting";

    // Old values for change reset
    private float oldMusicSliderValue = float.NaN;
    private float oldEffectsSliderValue = float.NaN;


    private void Start()
    {
        LoadVolume();
        // Init volumes by initial slider positions
        SetMusicVolume();
        SetEffectsVolume();
    }

    public void SetMusicVolume()
    {
        float volume = 0f;
        if (musicSlider != null)
        {
            volume = 20 * Mathf.Log10(musicSlider.value);
        }
        audioMixer.SetFloat("musicVolume", volume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
    }

    public void SetEffectsVolume()
    {
        float volume = 0f;
        if (effectsSlider != null)
        {
            volume = 20 * Mathf.Log10(effectsSlider.value);
        }
        audioMixer.SetFloat("effectsVolume", volume);
        PlayerPrefs.SetFloat(EFFECTS_VOLUME_KEY, volume);
    }

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
    public void RestoreVolumes()
    {
        if (this.oldMusicSliderValue != float.NaN)
        {
             musicSlider.value = this.oldMusicSliderValue;
        }
        if (this.oldEffectsSliderValue != float.NaN)
        {
            effectsSlider.value = this.oldEffectsSliderValue;
        }
    }

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
