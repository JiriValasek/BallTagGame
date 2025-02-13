using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource effectsSource;

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
        musicSource.enabled = true;
        effectsSource.enabled = true;
    }

    private void Start()
    {
        if (background != null && effectsSource.isActiveAndEnabled)
        {
            musicSource.clip = background;
            musicSource.Play();
        }   
    }

    public void playEffect(AudioClip effect)
    {
        if (effect != null && effectsSource.isActiveAndEnabled)
        {
            effectsSource.PlayOneShot(effect);
        }
    }
}
