using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance { get { return instance; } }

    [Header("Panels")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;

    [Header("Victory UI")]
    public GameObject[] stars; // Array of 3 Star Images in the Victory Panel
    public TextMeshProUGUI victoryText;

    private void Awake()
    {
        // Singleton Pattern
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Ensure panels are hidden at start
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    // --- Button Functions ---

    public void PauseGame()
    {
        if (pausePanel != null) pausePanel.SetActive(true);
        Time.timeScale = 0f; // Freezes the game
    }

    public void ResumeGame()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f; // Unfreezes
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Assumes Menu is Scene 0
    }

    // --- Game Over Logic ---

    public void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        SoundManager.Instance.GameOverSound();
        Time.timeScale = 0f; // Stop the game
    }

    public void ShowVictory(int starCount)
    {
        if (victoryPanel != null) victoryPanel.SetActive(true);
        SoundManager.Instance.GameWinSound();
        Time.timeScale = 0f;

        // Show the correct number of stars
        for (int i = 0; i < stars.Length; i++)
        {
            if (i < starCount)
                stars[i].SetActive(true); // Show filled star
            else
                stars[i].SetActive(false); // Hide star (or show empty star if you have logic for that)
        }
    }
  
}