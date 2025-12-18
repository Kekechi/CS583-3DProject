using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Room completion popup UI.
/// Shows when all 3 items are placed (RoomCompletion state).
/// Handles "Continue" button to return to main menu.
/// </summary>
public class RoomCompleteUI : MonoBehaviour
{
  // ─────────────────────────────────────────────────────────────────────────
  // Inspector Fields
  // ─────────────────────────────────────────────────────────────────────────
  [Header("References")]
  [SerializeField] private GameObject rootPanel;
  [SerializeField] private TMP_Text titleText;
  [SerializeField] private Button continueButton;

  [Header("Unlock Notification")]
  [SerializeField] private GameObject unlockNotificationPanel;
  [SerializeField] private TMP_Text unlockText;
  [SerializeField] private UnityEngine.UI.Image unlockIconImage;

  [Header("Future: Quality Rating")]
  [SerializeField] private GameObject ratingContainer;
  // Future: Add star images, harmony score text, etc.

  [Header("Settings")]
  [SerializeField] private string mainMenuSceneName = "MainMenu";
  [SerializeField] private string completionTitle = "Room Complete!";

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────
  private void Start()
  {
    Hide();
    SetupButton();
    SubscribeToEvents();

    Debug.Log("[RoomCompleteUI] Initialized");
  }

  private void OnDestroy()
  {
    UnsubscribeFromEvents();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Setup
  // ─────────────────────────────────────────────────────────────────────────
  private void SetupButton()
  {
    if (continueButton != null)
    {
      continueButton.onClick.AddListener(OnContinueClicked);
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Event Subscription
  // ─────────────────────────────────────────────────────────────────────────
  private void SubscribeToEvents()
  {
    if (GameManager.Instance != null)
    {
      GameManager.Instance.OnStateChanged += HandleStateChanged;
      Debug.Log("[RoomCompleteUI] Subscribed to GameManager.OnStateChanged");
    }
    else
    {
      Debug.LogWarning("[RoomCompleteUI] GameManager.Instance is null in Start");
    }
  }

  private void UnsubscribeFromEvents()
  {
    if (GameManager.Instance != null)
    {
      GameManager.Instance.OnStateChanged -= HandleStateChanged;
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Event Handlers
  // ─────────────────────────────────────────────────────────────────────────
  private void HandleStateChanged(GameManager.GameState oldState, GameManager.GameState newState)
  {
    if (newState == GameManager.GameState.RoomCompletion)
    {
      Show();
    }
    else
    {
      Hide();
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Public Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Show the completion popup.
  /// </summary>
  public void Show()
  {
    if (rootPanel != null)
    {
      rootPanel.SetActive(true);
    }

    // Set title text
    if (titleText != null)
    {
      titleText.text = completionTitle;
    }

    // Check for new unlocks
    ShowUnlockNotification();

    // Hide rating container for now (future feature)
    if (ratingContainer != null)
    {
      ratingContainer.SetActive(false);
    }

    Debug.Log("[RoomCompleteUI] Shown");
  }

  /// <summary>
  /// Check and display any new unlock notifications.
  /// </summary>
  private void ShowUnlockNotification()
  {
    if (UnlockManager.Instance == null) return;

    string latestUnlock = UnlockManager.Instance.GetLatestUnlock();

    if (!string.IsNullOrEmpty(latestUnlock))
    {
      // Show unlock notification
      if (unlockNotificationPanel != null)
      {
        unlockNotificationPanel.SetActive(true);
      }

      if (unlockText != null)
      {
        unlockText.text = $"{latestUnlock}";
      }

      // Display unlock icon
      if (unlockIconImage != null)
      {
        Sprite unlockIcon = UnlockManager.Instance.GetLatestUnlockIcon();
        if (unlockIcon != null)
        {
          unlockIconImage.sprite = unlockIcon;
          unlockIconImage.enabled = true;
        }
        else
        {
          unlockIconImage.enabled = false;
          Debug.LogWarning($"[RoomCompleteUI] No preview icon assigned for unlock: {latestUnlock}");
        }
      }

      // Play unlock sound
      if (AudioManager.Instance != null)
      {
        AudioManager.Instance.PlayChecklistComplete(); // Celebration sound
      }

      Debug.Log($"[RoomCompleteUI] Showing unlock notification: {latestUnlock}");
    }
    else
    {
      // Hide unlock notification
      if (unlockNotificationPanel != null)
      {
        unlockNotificationPanel.SetActive(false);
      }
    }
  }

  /// <summary>
  /// Hide the completion popup.
  /// </summary>
  public void Hide()
  {
    if (rootPanel != null)
    {
      rootPanel.SetActive(false);
    }
  }

  /// <summary>
  /// Called when Continue button is clicked.
  /// Returns to main menu.
  /// </summary>
  public void OnContinueClicked()
  {
    Debug.Log("[RoomCompleteUI] Continue clicked - returning to main menu");

    // Play button click sound
    if (AudioManager.Instance != null)
    {
      AudioManager.Instance.PlayButtonClick();
    }

    // Disable button to prevent double-clicks
    if (continueButton != null)
    {
      continueButton.interactable = false;
    }

    // Load main menu scene
    SceneLoader.LoadScene(mainMenuSceneName);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Future: Quality Rating Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Future: Set the quality rating display.
  /// </summary>
  /// <param name="rating">0-5 star rating</param>
  public void SetRating(int rating)
  {
    // TODO: Implement star display
    // - Show filled stars for rating
    // - Show empty stars for remaining
    Debug.Log($"[RoomCompleteUI] Rating: {rating}/5 stars (not implemented)");
  }

  /// <summary>
  /// Future: Set harmony score display.
  /// </summary>
  /// <param name="score">Harmony score percentage</param>
  public void SetHarmonyScore(float score)
  {
    // TODO: Implement harmony score display
    Debug.Log($"[RoomCompleteUI] Harmony: {score:P0} (not implemented)");
  }
}
