using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Tooltip("Pause panel to be shown on pause.")]
    [SerializeField] GameObject pausePanel;

    /** Audio manager for playing sound after time resumes, as playback needs time. */
    private AudioManager audioManager;

    private void Awake()
    {

        audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
    }

    /// <summary>
    /// Function pausing the game and showing the pause panel.
    /// </summary>
    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Function for switching to main menu scene and resuming time.
    /// </summary>
    public void Home()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1f;
        audioManager.playEffect(audioManager.button);
    }
    
    /// <summary>
    /// Function for continuing the game.
    /// </summary>
    public void Continue()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        audioManager.playEffect(audioManager.button);
    }

    /// <summary>
    /// Function for restarting the current level.
    /// </summary>
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
        audioManager.playEffect(audioManager.button);
    }
}
