using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Card Data")]
    public int cardId;
    public bool isFlipped = false;
    public bool isMatched = false;

    [Header("References")]
    public Image backImage;
    public Image frontImage;
    public Transform visualRoot; // The parent object that rotates
    private Button cardButton;

    private void Awake()
    {
        cardButton = GetComponent<Button>();
        // Automatically add the click listener
        cardButton.onClick.AddListener(OnCardClicked);
    }

    public void Initialize(Sprite faceSprite, int id)
    {
        cardId = id;
        frontImage.sprite = faceSprite;

        // Reset state
        isFlipped = false;
        isMatched = false;
        backImage.gameObject.SetActive(true);
        frontImage.gameObject.SetActive(false); // Ensure front is hidden initially

        cardButton.interactable = true;

        // Reset rotation and opacity in case of object pooling/restart
        visualRoot.rotation = Quaternion.identity;
        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = 1f;
    }

    public void OnCardClicked()
    {
        // 1. Safety Checks: Don't flip if already matched, flipped, or if GM is busy
        if (isFlipped || isMatched) return;
        if (GameManager.instance.isChecking) return;
        if (!GameManager.instance.isGameActive) return;

        // 2. Perform the Flip
        FlipCard();

        // 3. Notify Game Manager
        GameManager.instance.OnCardFlipped(this);
    }

    public void FlipCard()
    {
        isFlipped = true;
        cardButton.interactable = false;

        SoundManager.Instance.FlipSound(); // Plays sound

        StartCoroutine(FlipAnimation(true));
    }

    public void FlipBack()
    {
        isFlipped = false;
        cardButton.interactable = true;
        StartCoroutine(FlipAnimation(false));
    }

    public void SetMatched()
    {
        isMatched = true;
        cardButton.interactable = false;
        frontImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
        // StartCoroutine(FadeOut());
    }

    public bool IsMatched() => isMatched;

    // --- Visuals & Animations ---

    private IEnumerator FlipAnimation(bool showFront)
    {
        float time = 0f;
        float duration = 0.3f;

        Quaternion start = visualRoot.rotation;
        Quaternion mid = Quaternion.Euler(0, 90f, 0);
        // If showing front, end at 0 (or 180 depending on your setup). 
        // Based on your previous code: Front = 0, Back = 0 (reset). 
        // Let's rotate 180 to show back, 0 to show front.
        Quaternion end = showFront ? Quaternion.Euler(0, 180f, 0) : Quaternion.Euler(0, 0f, 0);

        // First half of rotation (0 to 90)
        while (time < duration / 2)
        {
            visualRoot.rotation = Quaternion.Slerp(start, mid, (time / (duration / 2)));
            time += Time.deltaTime;
            yield return null;
        }

        // Swap Images at the 90-degree mark
        if (showFront)
        {
            frontImage.gameObject.SetActive(true);
            backImage.gameObject.SetActive(false);
        }
        else
        {
            frontImage.gameObject.SetActive(false);
            backImage.gameObject.SetActive(true);
        }

        time = 0f;

        // Second half of rotation (90 to 180/0)
        while (time < duration / 2)
        {
            visualRoot.rotation = Quaternion.Slerp(mid, end, (time / (duration / 2)));
            time += Time.deltaTime;
            yield return null;
        }
        visualRoot.rotation = end;
    }

    //private IEnumerator FadeOut()
    //{
    //    CanvasGroup group = GetComponent<CanvasGroup>();
    //    // Add a CanvasGroup component if it doesn't exist
    //    if (group == null) group = gameObject.AddComponent<CanvasGroup>();

    //    float duration = 0.5f;
    //    float elapsed = 0f;

    //    while (elapsed < duration)
    //    {
    //        group.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
    //        elapsed += Time.deltaTime;
    //        yield return null;
    //    }
    //    group.alpha = 0f;
    //}

    // --- Helper functions for Game Start Preview ---

    public void ForceShowFront()
    {
        isFlipped = true;
        backImage.gameObject.SetActive(false);
        frontImage.gameObject.SetActive(true);
        visualRoot.rotation = Quaternion.Euler(0, 180f, 0); // Match the flip rotation
        cardButton.interactable = false;
    }

    public void ForceHideFront()
    {
        isFlipped = false;
        backImage.gameObject.SetActive(true);
        frontImage.gameObject.SetActive(false);
        visualRoot.rotation = Quaternion.Euler(0, 0f, 0);
        cardButton.interactable = true;
    }
}