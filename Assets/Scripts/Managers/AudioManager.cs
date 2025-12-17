using UnityEngine;

/// <summary>
/// Singleton AudioManager for playing sound effects and ambience.
/// Uses DontDestroyOnLoad to persist across scenes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────
    // Singleton
    // ─────────────────────────────────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    // ─────────────────────────────────────────────────────────────────────────
    // Audio Sources (assign in Inspector or created at runtime)
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    // ─────────────────────────────────────────────────────────────────────────
    // Audio Clips
    // ─────────────────────────────────────────────────────────────────────────
    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip checklistComplete;

    [Header("Game Sounds")]
    [SerializeField] private AudioClip miniGameComplete;

    [Header("Ambience")]
    [SerializeField] private AudioClip roomAmbience;

    // ─────────────────────────────────────────────────────────────────────────
    // Volume Settings
    // ─────────────────────────────────────────────────────────────────────────
    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.5f;

    // ─────────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Debug.Log("[AudioManager] Duplicate AudioManager found, destroying this one");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create AudioSources if not assigned
        EnsureAudioSources();

        Debug.Log("[AudioManager] Initialized");
    }

    /// <summary>
    /// Ensure we have AudioSource components
    /// </summary>
    private void EnsureAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
            Debug.Log("[AudioManager] Created music AudioSource");
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = sfxVolume;
            Debug.Log("[AudioManager] Created SFX AudioSource");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Generic Playback
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Play a one-shot sound effect.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlaySFX called with null clip");
            return;
        }

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Convenience Methods - UI Sounds
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Play button click sound.
    /// </summary>
    public void PlayButtonClick()
    {
        if (buttonClick != null)
        {
            PlaySFX(buttonClick);
            Debug.Log("[AudioManager] Playing button click");
        }
    }

    /// <summary>
    /// Play spot click sound (when clicking on placement spots).
    /// Uses buttonClick - can be changed to separate clip if desired.
    /// </summary>
    public void PlaySpotClick()
    {
        if (buttonClick != null)
        {
            PlaySFX(buttonClick);
            Debug.Log("[AudioManager] Playing spot click");
        }
    }

    /// <summary>
    /// Play button hover sound (quieter than click for UI hover feedback).
    /// </summary>
    public void PlayButtonHover()
    {
        if (buttonClick != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(buttonClick, sfxVolume * 0.3f); // 30% volume for hover
            Debug.Log("[AudioManager] Playing button hover");
        }
    }

    /// <summary>
    /// Play checklist complete sound.
    /// </summary>
    public void PlayChecklistComplete()
    {
        if (checklistComplete != null)
        {
            PlaySFX(checklistComplete);
            Debug.Log("[AudioManager] Playing checklist complete");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Convenience Methods - Game Sounds
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Play mini-game completion sound.
    /// </summary>
    public void PlayMiniGameComplete()
    {
        if (miniGameComplete != null)
        {
            PlaySFX(miniGameComplete);
            Debug.Log("[AudioManager] Playing mini-game complete");
        }
    }

    /// <summary>
    /// Play harmony chime (when all items placed).
    /// Uses miniGameComplete as fallback.
    /// </summary>
    public void PlayHarmonyChime()
    {
        // Use miniGameComplete for now, can add separate clip later
        PlayMiniGameComplete();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ambience Control
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Start playing room ambience (loops).
    /// </summary>
    public void PlayAmbience()
    {
        if (roomAmbience == null)
        {
            Debug.LogWarning("[AudioManager] roomAmbience clip not assigned");
            return;
        }

        if (musicSource != null)
        {
            musicSource.clip = roomAmbience;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
            Debug.Log("[AudioManager] Playing room ambience");
        }
    }

    /// <summary>
    /// Stop playing ambience.
    /// </summary>
    public void StopAmbience()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
            Debug.Log("[AudioManager] Stopped ambience");
        }
    }

    /// <summary>
    /// Check if ambience is currently playing.
    /// </summary>
    public bool IsAmbiencePlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Volume Control
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Set SFX volume (0-1).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// Set music/ambience volume (0-1).
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    /// <summary>
    /// Get current SFX volume.
    /// </summary>
    public float GetSFXVolume() => sfxVolume;

    /// <summary>
    /// Get current music volume.
    /// </summary>
    public float GetMusicVolume() => musicVolume;
}
