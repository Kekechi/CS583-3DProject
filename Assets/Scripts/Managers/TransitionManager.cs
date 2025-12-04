using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene transitions with fade effects.
/// Singleton that persists across scene loads.
/// </summary>
public class TransitionManager : MonoBehaviour
{
  // ─────────────────────────────────────────────────────────────────────────
  // Singleton
  // ─────────────────────────────────────────────────────────────────────────
  private static TransitionManager instance;
  public static TransitionManager Instance => instance;

  // ─────────────────────────────────────────────────────────────────────────
  // Inspector Fields
  // ─────────────────────────────────────────────────────────────────────────
  [Header("Fade Settings")]
  [SerializeField] private CanvasGroup fadeCanvasGroup;

  [Tooltip("Duration of fade in/out in seconds")]
  [SerializeField] private float defaultFadeDuration = 0.5f;

  [Tooltip("Time to hold at black before fading in")]
  [SerializeField] private float holdDuration = 0.2f;

  // ─────────────────────────────────────────────────────────────────────────
  // Runtime State
  // ─────────────────────────────────────────────────────────────────────────
  private bool isTransitioning = false;

  /// <summary>
  /// Check if currently in a transition (use to block input).
  /// </summary>
  public bool IsTransitioning => isTransitioning;

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────
  private void Awake()
  {
    // Singleton pattern - destroy duplicate
    if (instance != null && instance != this)
    {
      Debug.Log("[TransitionManager] Duplicate detected, destroying self");
      Destroy(gameObject);
      return;
    }

    instance = this;
    DontDestroyOnLoad(gameObject);

    // Initialize fade canvas
    if (fadeCanvasGroup != null)
    {
      fadeCanvasGroup.alpha = 0f;
      fadeCanvasGroup.blocksRaycasts = false;
    }
    else
    {
      Debug.LogWarning("[TransitionManager] fadeCanvasGroup not assigned!");
    }

    Debug.Log("[TransitionManager] Initialized");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Public Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Start transition to a new scene with default fade duration.
  /// </summary>
  /// <param name="sceneName">Name of scene to load</param>
  public void TransitionToScene(string sceneName)
  {
    TransitionToScene(sceneName, defaultFadeDuration);
  }

  /// <summary>
  /// Start transition to a new scene with custom fade duration.
  /// </summary>
  /// <param name="sceneName">Name of scene to load</param>
  /// <param name="fadeDuration">Duration of fade in/out</param>
  public void TransitionToScene(string sceneName, float fadeDuration)
  {
    if (isTransitioning)
    {
      Debug.LogWarning("[TransitionManager] Already transitioning, ignoring request");
      return;
    }

    StartCoroutine(TransitionCoroutine(sceneName, fadeDuration));
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private Methods
  // ─────────────────────────────────────────────────────────────────────────

  private IEnumerator TransitionCoroutine(string sceneName, float fadeDuration)
  {
    isTransitioning = true;
    Debug.Log($"[TransitionManager] Starting transition to '{sceneName}'");

    // Phase 1: Fade out (to black)
    yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

    // Phase 2: Load scene
    Debug.Log($"[TransitionManager] Loading scene '{sceneName}'");
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

    if (asyncLoad == null)
    {
      Debug.LogError($"[TransitionManager] Failed to load scene '{sceneName}'");
      yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
      isTransitioning = false;
      yield break;
    }

    // Wait for scene to load
    while (!asyncLoad.isDone)
    {
      yield return null;
    }

    Debug.Log($"[TransitionManager] Scene loaded");

    // Phase 3: Hold at black
    yield return new WaitForSeconds(holdDuration);

    // Phase 4: Fade in (from black)
    yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

    isTransitioning = false;
    Debug.Log($"[TransitionManager] Transition complete");
  }

  private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
  {
    if (fadeCanvasGroup == null)
    {
      Debug.LogWarning("[TransitionManager] fadeCanvasGroup is null, skipping fade");
      yield break;
    }

    // Block raycasts during fade to black
    fadeCanvasGroup.blocksRaycasts = endAlpha > 0.5f;
    fadeCanvasGroup.alpha = startAlpha;

    float elapsed = 0f;

    while (elapsed < duration)
    {
      elapsed += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
      float t = Mathf.Clamp01(elapsed / duration);
      fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
      yield return null;
    }

    fadeCanvasGroup.alpha = endAlpha;

    // Unblock raycasts when fully transparent
    fadeCanvasGroup.blocksRaycasts = endAlpha > 0.5f;
  }
}
