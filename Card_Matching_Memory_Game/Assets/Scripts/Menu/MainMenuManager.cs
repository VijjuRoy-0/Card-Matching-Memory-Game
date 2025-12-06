using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelUI
    {
        public Button levelButton;
        public GameObject lockIcon;
        public GameObject[] stars; // Array of 3 images
    }

    [Header("Level Setup")]
    public LevelUI easyLevel;   // Index 0
    public LevelUI mediumLevel; // Index 1
    public LevelUI hardLevel;   // Index 2
    public LevelUI DifficultLevel;

    public GameObject levelPanel;

    // If you want to use that 4th "Difficult" button, you can add it here later
    // public LevelUI difficultLevel; 

    void Start()
    {
        // Check and Update UI for each level
        UpdateLevelStatus(0, easyLevel);
        UpdateLevelStatus(1, mediumLevel);
        UpdateLevelStatus(2, hardLevel);
        UpdateLevelStatus(3, DifficultLevel);
    }

    void UpdateLevelStatus(int levelIndex, LevelUI levelUI)
    {
        // 1. CHECK UNLOCK STATUS
        // Level 0 (Easy) is ALWAYS unlocked.
        // Other levels: Check PlayerPrefs "LevelUnlocked_X" (1 = True, 0 = False)
        bool isUnlocked = (levelIndex == 0) || (PlayerPrefs.GetInt("LevelUnlocked_" + levelIndex, 0) == 1);

        if (isUnlocked)
        {
            // UNLOCKED STATE
            levelUI.levelButton.interactable = true;
            if (levelUI.lockIcon != null) levelUI.lockIcon.SetActive(false); // Hide Lock

            // 2. SHOW STARS
            int starCount = PlayerPrefs.GetInt("LevelStars_" + levelIndex, 0);

            // Loop through the 3 star images
            for (int i = 0; i < levelUI.stars.Length; i++)
            {
                // Example: If starCount is 2, i=0 (Active), i=1 (Active), i=2 (Inactive)
                if (i < starCount)
                    levelUI.stars[i].SetActive(true);
                else
                    levelUI.stars[i].SetActive(false);
            }
        }
        else
        {
            // LOCKED STATE
            levelUI.levelButton.interactable = false;
            if (levelUI.lockIcon != null) levelUI.lockIcon.SetActive(true); // Show Lock

            // Hide all stars
            foreach (var star in levelUI.stars) star.SetActive(false);
        }
    }

    // --- BUTTON FUNCTIONS ---

    public void LoadLevel(int levelIndex)
    {
        // 1. Save which level we selected (0, 1, or 2)
        PlayerPrefs.SetInt("SelectedLevel", levelIndex);

        // 2. Load the Game Scene
        SceneManager.LoadScene("Game");
    }

    // Helper to clear data for testing
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Levels()
    {
        levelPanel.gameObject.SetActive(true);
    }
    public void BackToMenu()
    {
        levelPanel.gameObject.SetActive(false);
    }
    public void Quit()
    {
        Application.Quit();
    }
}