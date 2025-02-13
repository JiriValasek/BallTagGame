using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndMenu : MonoBehaviour
{
    [Tooltip("End panel to be shown when player won/lost.")]
    [SerializeField] GameObject endPanel;
    [Tooltip("End title to be shown to the player.")]
    [SerializeField] TextMeshProUGUI endTitle;
    [Tooltip("Next level button to be disabled if user lost.")]
    [SerializeField] Button nextButton;

    public void End(bool playerWon)
    {
        if (playerWon)
        {
            endTitle.text = "You have won!";
            nextButton.interactable = true;
        }
        else
        {
            endTitle.text = "You have lost!";
            nextButton.interactable = false;
        }
        endPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    public void Next()
    {
        Time.timeScale = 1f;
        int sceneIndex = SceneUtility.GetBuildIndexByScenePath("Scenes/Level " + (SceneManager.GetActiveScene().buildIndex + 1));
        if (sceneIndex >= SceneManager.GetActiveScene().buildIndex)
        {
            SceneManager.LoadSceneAsync(sceneIndex);
        }
        else
        {
            SceneManager.LoadScene("Main Menu");
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
