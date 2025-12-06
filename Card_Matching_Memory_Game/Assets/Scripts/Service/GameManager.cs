using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    public GameObject cardPrefab;
    public Transform gridParent;
    public Sprite[] cardSprite;

    [Header("Level Settings")]
    public LevelData[] levels; // Drag your 3 levels here in the Inspector
    private LevelData currentLevelData;

    [Header("Game State")]
    public bool isGameActive = false;
    public bool isChecking = false; // To prevent clicking while animations play

    // Logic Lists
    private List<Card> flippedCards = new List<Card>();
    private List<int> cardIds = new List<int>();

    // Stats
    private float currentTime;
    private int movesTaken;
    private int currentScore;
    private int currentCombo;

    private GridLayoutGroup gridLayoutGroup;

    private void Awake()
    {
        // Singleton Pattern
        if (instance == null) instance = this;
        else Destroy(gameObject);

        gridLayoutGroup = gridParent.GetComponent<GridLayoutGroup>();
    }

    void Start()
    {
        // 1. Check which level was selected in the Menu (Defaults to 0/Easy)
        int levelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
        StartLevel(levelIndex);
    }

    private void Update()
    {
        // 2. Handle the Timer
        if (isGameActive)
        {
            currentTime -= Time.deltaTime;

            // Update UI
            if (ScoreManager.instance != null)
                ScoreManager.instance.UpdateTimer(currentTime);

            // Check Time Limit
            if (currentTime <= 0)
            {
                currentTime = 0;
                GameOver(false); // Lose due to time
            }
        }
    }

    public void StartLevel(int levelIndex)
    {
        // Safety check to prevent errors if index is wrong
        if (levelIndex >= levels.Length || levelIndex < 0) levelIndex = 0;

        currentLevelData = levels[levelIndex];

        // 3. Reset Game Stats
        currentTime = currentLevelData.timeLimit;
        movesTaken = 0;
        currentScore = 0;
        currentCombo = 0;
        flippedCards.Clear();
        isGameActive = true;
        isChecking = false;

        // 4. Update UI to show starting state
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.UpdateScore(currentScore, currentCombo);
            ScoreManager.instance.UpdateMoves(movesTaken, currentLevelData.maxMoves);
        }

        // 5. Generate the Grid
        GenerateBoard(currentLevelData.rows, currentLevelData.cols);
    }

    public void GenerateBoard(int rows, int columns)
    {
        // Clear old cards
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        int totalCards = rows * columns;
        cardIds.Clear();

        // Create pairs (0,0, 1,1, 2,2...)
        for (int i = 0; i < totalCards / 2; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        Shuffle(cardIds);
        StartCoroutine(SetupBoardCoroutine(rows, columns, totalCards));
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void OnCardFlipped(Card card)
    {
        if (!isGameActive) return;

        flippedCards.Add(card);

        if (flippedCards.Count == 2 && !isChecking)
        {
            isChecking = true; // Block input while checking

            // Increment Moves
            movesTaken++;
            if (ScoreManager.instance != null)
                ScoreManager.instance.UpdateMoves(movesTaken, currentLevelData.maxMoves);

            // Check Move Limit
            if (movesTaken > currentLevelData.maxMoves)
            {
                GameOver(false); // Lose due to moves
                return;
            }

            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        // Wait for flip animation to finish
        yield return new WaitForSeconds(0.6f);

        if (flippedCards[0].cardId == flippedCards[1].cardId)
        {
            // --- MATCH ---
            flippedCards[0].SetMatched();
            flippedCards[1].SetMatched();

            currentCombo++;
            // Simple scoring formula
            currentScore += 10 + (currentCombo - 1) * 5;

            SoundManager.Instance.MatchSound();
        }
        else
        {
            // --- NO MATCH ---
            flippedCards[0].FlipBack();
            flippedCards[1].FlipBack();

            currentCombo = 0; // Reset combo

            SoundManager.Instance.MisMatchSound();
        }

        // Update Score UI
        if (ScoreManager.instance != null)
            ScoreManager.instance.UpdateScore(currentScore, currentCombo);

        flippedCards.Clear();
        isChecking = false; // Allow input again

        // Check if won
        if (AllCardMatched())
        {
            GameOver(true);
        }
    }

    bool AllCardMatched()
    {
        // Simple check: if any card is NOT matched, we haven't won yet
        foreach (Card card in FindObjectsOfType<Card>())
        {
            if (!card.IsMatched()) return false;
        }
        return true;
    }

    void GameOver(bool victory)
    {
        isGameActive = false;

        if (victory)
        {
            Debug.Log("Victory!");
            SoundManager.Instance.GameOverSound();

            // Calculate Stars
            int stars = 1;
            float timeRatio = currentTime / currentLevelData.timeLimit;
            if (timeRatio > 0.5f) stars = 3;
            else if (timeRatio > 0.25f) stars = 2;

            // Save Data (Logic from previous step)
            int currentIdx = System.Array.IndexOf(levels, currentLevelData);
            int oldStars = PlayerPrefs.GetInt("LevelStars_" + currentIdx, 0);
            if (stars > oldStars) PlayerPrefs.SetInt("LevelStars_" + currentIdx, stars);
            if (currentIdx + 1 < levels.Length) PlayerPrefs.SetInt("LevelUnlocked_" + (currentIdx + 1), 1);
            PlayerPrefs.Save();

            // CALL THE NEW UI MANAGER
            if (UIManager.Instance != null) UIManager.Instance.ShowVictory(stars);
        }
        else
        {
            // CALL THE NEW UI MANAGER
            if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
        }
    }

    private IEnumerator SetupBoardCoroutine(int numRows, int numCols, int totalCards)
    {
        // Wait frame for GridLayout to calculate screen size
        yield return new WaitForEndOfFrame();

        // 1. Configure Grid Layout
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = numCols;

        // 2. Dynamic Cell Sizing (Fits cards to screen)
        float panelWidth = ((RectTransform)gridParent.transform).rect.width;
        float panelHeight = ((RectTransform)gridParent.transform).rect.height;

        float spacing = gridLayoutGroup.spacing.x;
        float padding = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;

        // Calculate available width per card
        float cellWidth = (panelWidth - padding - (spacing * (numCols - 1))) / numCols;

        // Keep them square
        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellWidth);

        // 3. Instantiate Cards
        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridParent);
            Card card = cardObj.GetComponent<Card>();

            // Use Modulo (%) to cycle through sprites if you have more cards than sprites
            card.Initialize(cardSprite[cardIds[i] % cardSprite.Length], cardIds[i]);
        }

        // 4. Preview Cards
        StartCoroutine(RevealAndHideCards());
    }

    private IEnumerator RevealAndHideCards()
    {
        // Temporarily show all cards so player can memorize
        List<Card> allCards = new List<Card>(FindObjectsOfType<Card>());

        foreach (Card card in allCards) card.ForceShowFront();

        yield return new WaitForSeconds(1.5f); // Duration of preview

        foreach (Card card in allCards) card.ForceHideFront();
    }
}
[System.Serializable]
public class LevelData
{
    public string levelName;
    public int rows;
    public int cols;
    public float timeLimit;
    public int maxMoves;
}
