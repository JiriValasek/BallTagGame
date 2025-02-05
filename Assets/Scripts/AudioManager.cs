using UnityEngine;

public class AudioManager : MonoBehaviour
{

    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource effectsSource;

    [Header("Audio Clip")]
    [SerializeField] AudioClip background;
    [SerializeField] AudioClip lifeExtension;
    [SerializeField] AudioClip jump;
    [SerializeField] AudioClip teleport;
    [SerializeField] AudioClip speedUp;
    [SerializeField] AudioClip slowDown;
    [SerializeField] AudioClip death;

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void playEffect(AudioClip effect)
    {
        effectsSource.PlayOneShot(effect);
    }
}
