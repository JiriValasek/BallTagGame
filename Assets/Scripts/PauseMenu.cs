using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Tooltip("Pause panel to be shown on pause.")]
    [SerializeField] GameObject pausePanel;

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Home()
    {
        SceneManager.LoadScene("Main Menu");
        Time.timeScale = 1f;
    }

    public void Continue()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
}
