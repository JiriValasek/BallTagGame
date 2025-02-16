using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    // References to UI GameObjects
    [Tooltip("Parent GameObject for all level buttons.")]
    public GameObject levelButtonParent;
    /** List of buttons for each level in ascending order. */
    private Button[] levelButtons;

    // Player preferences keys
    /** PlayerPrefs key for unlocked level based on finished levels. */
    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";

    private void Awake()
    {
        GetLevelButtons();
        if (!PlayerPrefs.HasKey(UNLOCKED_LEVEL_KEY))
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, 1);
        }
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = false;
        }
        for (int i = 1;i <= PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY); i++)
        {
            int sceneIndex = SceneUtility.GetBuildIndexByScenePath("Scenes/Level " + (i));
            if (sceneIndex > -1)
            {
                levelButtons[i-1].interactable = true;
            }
            
        }
    }

    public void OpenLevel(int levelId)
    {
        SceneManager.LoadScene("Level " + levelId); 
    }

    private void GetLevelButtons()
    {
        int childCount = levelButtonParent.transform.childCount;
        levelButtons = new Button[childCount];
        for(int i = 0; i < childCount; i++)
        {
            levelButtons[i] = levelButtonParent.transform.GetChild(i).GetComponent<Button>();
        }
    }
}
