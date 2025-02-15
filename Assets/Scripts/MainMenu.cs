using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{


    [DllImport("__Internal")]
    private static extern void closewindow();

    /// <summary>
    /// Function loading a scene with build index 1, for testing.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }

    /// <summary>
    /// Function for quiting the game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        closewindow();
    }
}
