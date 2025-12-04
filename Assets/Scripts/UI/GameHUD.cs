using UnityEngine;

/// <summary>
/// Root controller for in-game HUD visibility.
/// Shows/hides HUD based on GameManager state.
/// </summary>
public class GameHUD : MonoBehaviour
{
  // ─────────────────────────────────────────────────────────────────────────
  // Inspector Fields
  // ─────────────────────────────────────────────────────────────────────────
  [Header("References")]
  [SerializeField] private GameObject rootPanel;
  [SerializeField] private ProgressChecklistUI checklistUI;
  [SerializeField] private SpotInfoPanel spotInfoPanel;

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────
  private void Start()
  {
    SubscribeToEvents();

    // Show HUD by default (PlacingItem state)
    Show();

    Debug.Log("[GameHUD] Initialized");
  }

  private void OnDestroy()
  {
    UnsubscribeFromEvents();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Event Subscription
  // ─────────────────────────────────────────────────────────────────────────
  private void SubscribeToEvents()
  {
    if (GameManager.Instance != null)
    {
      GameManager.Instance.OnStateChanged += HandleStateChanged;
      Debug.Log("[GameHUD] Subscribed to GameManager.OnStateChanged");
    }
    else
    {
      Debug.LogWarning("[GameHUD] GameManager.Instance is null in Start");
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
    Debug.Log($"[GameHUD] State changed: {oldState} → {newState}");

    switch (newState)
    {
      case GameManager.GameState.PlacingItem:
        Show();
        break;

      case GameManager.GameState.PlayingMiniGame:
        Hide();
        break;

      case GameManager.GameState.RoomCompletion:
        Hide();
        break;
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Public Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Show the entire HUD.
  /// </summary>
  public void Show()
  {
    if (rootPanel != null)
    {
      rootPanel.SetActive(true);
    }

    Debug.Log("[GameHUD] Shown");
  }

  /// <summary>
  /// Hide the entire HUD.
  /// </summary>
  public void Hide()
  {
    if (rootPanel != null)
    {
      rootPanel.SetActive(false);
    }

    // Also hide spot info panel immediately
    if (spotInfoPanel != null)
    {
      spotInfoPanel.Hide();
    }

    Debug.Log("[GameHUD] Hidden");
  }
}
