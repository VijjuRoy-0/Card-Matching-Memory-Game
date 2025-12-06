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
    public LevelData[] levels; // Drag your 3 levels here in Inspector
    private LevelData currentLevelData;

    [Header("Game State")]
    public bool isGameActive = false;
    public bool isChecking = false;

    // Lists & Stats
    private List<Card> flippedCards = new List<Card>();
    private List<int> cardIds = new List<int>();
    private float currentTime;
    private int movesTaken;
    private int currentScore;
    private int currentCombo;

    private GridLayoutGroup gridLayoutGroup;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        gridLayoutGroup = gridParent.GetComponent<GridLayoutGroup>();
    }

    void Start()
    {
        // 1. Read which level the player clicked in the Menu
        // Default to 0 (Easy) if nothing is found
        int levelIndex = PlayerPrefs.GetInt("SelectedLevel", 0);
        StartLevel(levelIndex);
    }

    private void Update()
    {
        // 2. Timer Logic
        if (isGameActive)
        {
            currentTime -= Time.deltaTime;

            if (ScoreManager.instance != null)
                ScoreManager.instance.UpdateTimer(currentTime);

            if (currentTime <= 0)
            {
                currentTime = 0;
                GameOver(false); // Time ran out
            }
        }
    }

    public void StartLevel(int levelIndex)
    {
        // Safety check
        if (levelIndex >= levels.Length || levelIndex < 0) levelIndex = 0;

        currentLevelData = levels[levelIndex];

        // 3. Reset Stats
        currentTime = currentLevelData.timeLimit;
        movesTaken = 0;
        currentScore = 0;
        currentCombo = 0;
        flippedCards.Clear();
        isGameActive = true;
        isChecking = false;

        // 4. Update UI
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.UpdateScore(currentScore, currentCombo);
            ScoreManager.instance.UpdateMoves(movesTaken, currentLevelData.maxMoves);
        }

        // 5. Build the Grid (Easy/Medium/Hard)
        GenerateBoard(currentLevelData.rows, currentLevelData.cols);
    }

    public void GenerateBoard(int rows, int columns)
    {
        foreach (Transform child in gridParent) Destroy(child.gameObject);

        int totalCards = rows * columns;
        cardIds.Clear();

        // Create pairs
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
            isChecking = true;

            movesTaken++;
            if (ScoreManager.instance != null)
                ScoreManager.instance.UpdateMoves(movesTaken, currentLevelData.maxMoves);

            if (movesTaken > currentLevelData.maxMoves)
            {
                GameOver(false); // Out of moves
                return;
            }

            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(0.6f);

        if (flippedCards[0].cardId == flippedCards[1].cardId)
        {
            // --- MATCH ---
            flippedCards[0].SetMatched();
            flippedCards[1].SetMatched();

            currentCombo++;
            currentScore += 10 + (currentCombo - 1) * 5;
            SoundManager.Instance.MatchSound();
        }
        else
        {
            // --- NO MATCH ---
            flippedCards[0].FlipBack();
            flippedCards[1].FlipBack();
            currentCombo = 0;
            SoundManager.Instance.MisMatchSound();
        }

        if (ScoreManager.instance != null)
            ScoreManager.instance.UpdateScore(currentScore, currentCombo);

        flippedCards.Clear();
        isChecking = false;

        if (AllCardMatched())
        {
            GameOver(true);
        }
    }

    bool AllCardMatched()
    {
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
            

            // --- SAVE PROGRESS ---
            // 1. Calculate Stars
            int stars = 1;
            float timeRatio = currentTime / currentLevelData.timeLimit;
            if (timeRatio > 0.5f) stars = 3;
            else if (timeRatio > 0.25f) stars = 2;

            // 2. Unlock Next Level Logic
            int currentIdx = System.Array.IndexOf(levels, currentLevelData);

            // Save Stars
            int oldStars = PlayerPrefs.GetInt("LevelStars_" + currentIdx, 0);
            if (stars > oldStars) PlayerPrefs.SetInt("LevelStars_" + currentIdx, stars);

            // Unlock next level (if not last level)
            if (currentIdx + 1 < levels.Length)
            {
                PlayerPrefs.SetInt("LevelUnlocked_" + (currentIdx + 1), 1);
            }

            PlayerPrefs.Save();

            if (UIManager.Instance != null) UIManager.Instance.ShowVictory(stars);
        }
        else
        {
            if (UIManager.Instance != null) UIManager.Instance.ShowGameOver();
        }
    }

    // --- GRID LAYOUT LOGIC (Prevent Overlap) ---
    private IEnumerator SetupBoardCoroutine(int numRows, int numCols, int totalCards)
    {
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        RectTransform panelRect = gridParent.GetComponent<RectTransform>();
        float width = panelRect.rect.width;
        float height = panelRect.rect.height;

        float spacingX = gridLayoutGroup.spacing.x;
        float spacingY = gridLayoutGroup.spacing.y;
        float paddingX = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
        float paddingY = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;

        // MATH: Subtract gaps so cards don't touch
        float availableWidth = width - paddingX - (spacingX * (numCols - 1));
        float availableHeight = height - paddingY - (spacingY * (numRows - 1));

        float cardWidth = availableWidth / numCols;
        float cardHeight = availableHeight / numRows;

        // Prevent negative size
        if (cardWidth <= 10) cardWidth = 100;
        if (cardHeight <= 10) cardHeight = 100;

        // Aspect Ratio: Make them rectangles (Width x 1.4)
        float finalWidth = cardWidth;
        float finalHeight = cardWidth * 1.4f;

        // If too tall, shrink both to fit height
        if (finalHeight > cardHeight)
        {
            finalHeight = cardHeight;
            finalWidth = finalHeight / 1.4f;
        }

        gridLayoutGroup.cellSize = new Vector2(finalWidth, finalHeight);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = numCols;

        // Spawn Cards
        for (int i = 0; i < totalCards; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridParent);
            Card card = cardObj.GetComponent<Card>();
            card.Initialize(cardSprite[cardIds[i] % cardSprite.Length], cardIds[i]);
        }

        StartCoroutine(RevealAndHideCards());
    }

    private IEnumerator RevealAndHideCards()
    {
        List<Card> allCards = new List<Card>(FindObjectsOfType<Card>());
        foreach (Card card in allCards) card.ForceShowFront();

        yield return new WaitForSeconds(1.5f);

        foreach (Card card in allCards) card.ForceHideFront();
    }
}
[System.Serializable]
public class LevelData
{
    public string levelName;  // Name like "Easy", "Medium"
    public int rows;          // Number of rows (e.g., 3)
    public int cols;          // Number of columns (e.g., 4)
    public float timeLimit;   // Time in seconds (e.g., 60)
    public int maxMoves;      // Max moves allowed (e.g., 20)
}