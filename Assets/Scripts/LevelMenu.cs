using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{

    public GameObject levelButtonView;
    private Button[] levelButtons;
    private const string UNLOCKED_LEVEL = "UnlockedLevel";

    private void Awake()
    {
        GetLevelButtons();
        if (!PlayerPrefs.HasKey(UNLOCKED_LEVEL))
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVEL, 1);
        }
        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = false;
        }
        for (int i = 0;i < PlayerPrefs.GetInt(UNLOCKED_LEVEL); i++)
        {
            levelButtons[i].interactable = true;
        }
    }

    public void OpenLevel(int levelId)
    {
        SceneManager.LoadScene("Level " + levelId); 
    }

    private void GetLevelButtons()
    {
        int childCount = levelButtonView.transform.childCount;
        levelButtons = new Button[childCount];
        for(int i = 0; i < childCount; i++)
        {
            levelButtons[i] = levelButtonView.transform.GetChild(i).GetComponent<Button>();
        }
    }
}
