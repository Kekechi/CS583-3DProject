using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Store UI for viewing and selecting item variants.
/// Displays unlocked items and allows player to choose which variants to use.
/// </summary>
public class StoreUI : MonoBehaviour
{
  [Header("UI References")]
  [SerializeField] private GameObject storePanel;
  [SerializeField] private CanvasGroup canvasGroup;
  [SerializeField] private Button closeButton;

  [Header("Lantern Buttons")]
  [SerializeField] private StoreItemButton lanternDefaultButton;
  [SerializeField] private StoreItemButton lanternStyleBButton;

  [Header("Origami Buttons")]
  [SerializeField] private StoreItemButton origamiDefaultButton;
  [SerializeField] private StoreItemButton origamiStyleBButton;

  [Header("Calligraphy Buttons")]
  [SerializeField] private StoreItemButton calligraphyDefaultButton;
  [SerializeField] private StoreItemButton calligraphyStyleBButton;

  [Header("Animation")]
  [SerializeField] private float fadeTime = 0.3f;

  private UnlockManager unlockManager;

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────

  private void Awake()
  {
    // Setup close button
    if (closeButton != null)
    {
      closeButton.onClick.AddListener(Hide);
    }

    // Setup all item buttons
    SetupItemButton(lanternDefaultButton, MiniGameType.Lantern, ItemVariant.Default);
    SetupItemButton(lanternStyleBButton, MiniGameType.Lantern, ItemVariant.StyleB);
    SetupItemButton(origamiDefaultButton, MiniGameType.Origami, ItemVariant.Default);
    SetupItemButton(origamiStyleBButton, MiniGameType.Origami, ItemVariant.StyleB);
    SetupItemButton(calligraphyDefaultButton, MiniGameType.Calligraphy, ItemVariant.Default);
    SetupItemButton(calligraphyStyleBButton, MiniGameType.Calligraphy, ItemVariant.StyleB);

    // Hide initially
    if (storePanel != null)
    {
      storePanel.SetActive(false);
    }
  }

  private void Start()
  {
    unlockManager = UnlockManager.Instance;

    if (unlockManager == null)
    {
      Debug.LogError("[StoreUI] UnlockManager not found!");
    }
    else
    {
      // Initialize all button states immediately
      RefreshStore();
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Public Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Show the store panel with fade-in animation.
  /// </summary>
  public void Show()
  {
    if (storePanel == null) return;

    // IMPORTANT: Activate panel FIRST so coroutines can run
    storePanel.SetActive(true);

    // Set initial alpha for fade-in
    if (canvasGroup != null)
    {
      canvasGroup.alpha = 0f;
    }

    RefreshStore();

    // Play audio
    if (AudioManager.Instance != null)
    {
      AudioManager.Instance.PlayButtonClick();
    }

    // Fade in (now that panel is active)
    if (canvasGroup != null)
    {
      StartCoroutine(FadeIn());
    }

    Debug.Log("[StoreUI] Store opened");
  }

  /// <summary>
  /// Hide the store panel with fade-out animation.
  /// </summary>
  public void Hide()
  {
    if (storePanel == null) return;

    // Play audio
    if (AudioManager.Instance != null)
    {
      AudioManager.Instance.PlayButtonClick();
    }

    // Fade out
    if (canvasGroup != null)
    {
      StartCoroutine(FadeOut());
    }
    else
    {
      storePanel.SetActive(false);
    }

    Debug.Log("[StoreUI] Store closed");
  }

  /// <summary>
  /// Refresh all buttons to reflect current unlock/selection state.
  /// </summary>
  public void RefreshStore()
  {
    // Ensure we have UnlockManager reference
    if (unlockManager == null)
    {
      unlockManager = UnlockManager.Instance;
    }

    if (unlockManager == null)
    {
      Debug.LogWarning("[StoreUI] Cannot refresh - UnlockManager not found!");
      return;
    }

    // Refresh all buttons
    RefreshItemButton(lanternDefaultButton, MiniGameType.Lantern, ItemVariant.Default);
    RefreshItemButton(lanternStyleBButton, MiniGameType.Lantern, ItemVariant.StyleB);
    RefreshItemButton(origamiDefaultButton, MiniGameType.Origami, ItemVariant.Default);
    RefreshItemButton(origamiStyleBButton, MiniGameType.Origami, ItemVariant.StyleB);
    RefreshItemButton(calligraphyDefaultButton, MiniGameType.Calligraphy, ItemVariant.Default);
    RefreshItemButton(calligraphyStyleBButton, MiniGameType.Calligraphy, ItemVariant.StyleB);

    Debug.Log("[StoreUI] Store refreshed");
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Setup a single item button with click handler.
  /// </summary>
  private void SetupItemButton(StoreItemButton itemButton, MiniGameType type, ItemVariant variant)
  {
    if (itemButton == null) return;

    itemButton.Setup(type, variant);
    itemButton.OnClicked += () => OnItemSelected(type, variant);
  }

  /// <summary>
  /// Refresh a single item button's state (locked/unlocked/selected).
  /// </summary>
  private void RefreshItemButton(StoreItemButton itemButton, MiniGameType type, ItemVariant variant)
  {
    if (itemButton == null || unlockManager == null) return;

    bool isUnlocked = unlockManager.IsUnlocked(type, variant);
    bool isSelected = unlockManager.GetSelectedVariant(type) == variant;

    itemButton.SetState(isUnlocked, isSelected);
  }

  /// <summary>
  /// Handle item selection.
  /// </summary>
  private void OnItemSelected(MiniGameType type, ItemVariant variant)
  {
    if (unlockManager == null) return;

    // Check if unlocked
    if (!unlockManager.IsUnlocked(type, variant))
    {
      Debug.Log($"[StoreUI] Cannot select locked item: {type} - {variant}");
      return;
    }

    // Set selection in UnlockManager
    unlockManager.SetSelectedVariant(type, variant);

    // Play audio
    if (AudioManager.Instance != null)
    {
      AudioManager.Instance.PlayButtonClick();
    }

    // Refresh UI to show new selection
    RefreshStore();

    Debug.Log($"[StoreUI] Selected: {type} - {variant}");
  }

  /// <summary>
  /// Fade in animation.
  /// </summary>
  private IEnumerator FadeIn()
  {
    float elapsed = 0f;
    canvasGroup.alpha = 0f;

    while (elapsed < fadeTime)
    {
      elapsed += Time.deltaTime;
      canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeTime);
      yield return null;
    }

    canvasGroup.alpha = 1f;
  }

  /// <summary>
  /// Fade out animation.
  /// </summary>
  private IEnumerator FadeOut()
  {
    float elapsed = 0f;
    canvasGroup.alpha = 1f;

    while (elapsed < fadeTime)
    {
      elapsed += Time.deltaTime;
      canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeTime);
      yield return null;
    }

    canvasGroup.alpha = 0f;
    storePanel.SetActive(false);
  }
}
