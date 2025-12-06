using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance { get { return instance; } }

    [Header("Audio Clips")]
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip misMatchSound;
    public AudioClip gameOverSound;
    public AudioClip gameWinSound;

    [Header("Settings")]
    [Range(0f, 1f)] public float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton Pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Keeps sound playing when changing scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Add AudioSource component if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // PlayOneShot allows multiple sounds to overlap (e.g., fast clicking)
    public void FlipSound()
    {
        if (flipSound != null) audioSource.PlayOneShot(flipSound, volume);
    }

    public void MatchSound()
    {
        if (matchSound != null) audioSource.PlayOneShot(matchSound, volume);
    }

    public void MisMatchSound()
    {
        if (misMatchSound != null) audioSource.PlayOneShot(misMatchSound, volume);
    }

    public void GameOverSound()
    {
        if (gameOverSound != null) audioSource.PlayOneShot(gameOverSound, volume);
    }
    public void GameWinSound()
    {
        if (gameWinSound != null) audioSource.PlayOneShot(gameWinSound, volume);
    }

}