using UnityEngine;

public class AudioManager : MonoBehaviour
{

    // Audio sources
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource effectsSource;

    // AudioClips for playing from scripts
    [Header("Audio Clips")]
    public AudioClip background;
    public AudioClip pickUp;
    public AudioClip jump;
    public AudioClip teleport;
    public AudioClip speedUp;
    public AudioClip slowDown;
    public AudioClip death;
    public AudioClip button;

    private void Awake()
    {
        // Enable sources
        musicSource.enabled = true;
        effectsSource.enabled = true;
    }

    private void Start()
    {
        // Play background, don't forget to tick out 'loop' at the sample
        if (background != null && effectsSource.isActiveAndEnabled)
        {
            musicSource.clip = background;
            musicSource.Play();
        }   
    }

    /// <summary>
    /// Function from playing effects from UI buttons or GameObject scripts.
    /// </summary>
    /// <param name="effect">Music sample, for scripts use one of public ones in this class.</param>
    public void playEffect(AudioClip effect)
    {
        if (effect != null && effectsSource.isActiveAndEnabled)
        {
            effectsSource.PlayOneShot(effect);
        }
    }
}
