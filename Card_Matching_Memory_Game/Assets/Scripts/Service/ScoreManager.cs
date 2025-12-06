using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Required for TextMeshPro elements

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI movesText;

    private void Awake()
    {
        // Singleton Pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Updates the score display.
    /// </summary>
    public void UpdateScore(int score, int combo)
    {
        if (scoreText != null)
        {
            // Shows Score and current Combo multiplier
            scoreText.text = $"Score: {score} (x{combo})";
        }
    }

    /// <summary>
    /// Updates the timer display in MM:SS format.
    /// </summary>
    public void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            // Clamp to 0 so we don't show negative numbers
            if (timeRemaining < 0) timeRemaining = 0;

            // Calculate minutes and seconds
            float minutes = Mathf.FloorToInt(timeRemaining / 60);
            float seconds = Mathf.FloorToInt(timeRemaining % 60);

            // Format as 00:00
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    /// <summary>
    /// Updates the moves counter (Current / Max).
    /// </summary>
    public void UpdateMoves(int currentMoves, int maxMoves)
    {
        if (movesText != null)
        {
            movesText.text = $"Moves: {currentMoves}/{maxMoves}";

            // Optional: Change color to red if running low on moves
            if (currentMoves >= maxMoves - 5)
                movesText.color = Color.red;
            else
                movesText.color = Color.white;
        }
    }
}
