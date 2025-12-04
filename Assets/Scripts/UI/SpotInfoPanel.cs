using UnityEngine;
using TMPro;

/// <summary>
/// Shows tooltip when hovering over placement spots.
/// Displays mini-game name, description, and hint.
/// </summary>
public class SpotInfoPanel : MonoBehaviour
{
  // ─────────────────────────────────────────────────────────────────────────
  // Inspector Fields
  // ─────────────────────────────────────────────────────────────────────────
  [Header("References")]
  [SerializeField] private GameObject rootPanel;
  [SerializeField] private TMP_Text nameText;
  [SerializeField] private TMP_Text descriptionText;
  [SerializeField] private TMP_Text hintText;

  [Header("Positioning")]
  [Tooltip("Offset from mouse position")]
  [SerializeField] private Vector2 offset = new Vector2(20f, 20f);

  [Header("Dependencies")]
  [SerializeField] private MiniGameController miniGameController;
  [SerializeField] private RoomController roomController;

  [Header("Settings")]
  [SerializeField] private string defaultHint = "Click to start";

  // ─────────────────────────────────────────────────────────────────────────
  // Runtime State
  // ─────────────────────────────────────────────────────────────────────────
  private RectTransform rectTransform;
  private Canvas parentCanvas;
  private bool isShowing = false;

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────
  private void Awake()
  {
    if (rootPanel != null)
    {
      rectTransform = rootPanel.GetComponent<RectTransform>();
    }

    // Find parent canvas for proper positioning
    parentCanvas = GetComponentInParent<Canvas>();
  }

  private void Start()
  {
    Hide();
    SubscribeToEvents();

    Debug.Log("[SpotInfoPanel] Initialized");
  }

  private void OnDestroy()
  {
    UnsubscribeFromEvents();
  }

  private void Update()
  {
    if (isShowing)
    {
      UpdatePosition();
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Event Subscription
  // ─────────────────────────────────────────────────────────────────────────
  private void SubscribeToEvents()
  {
    if (roomController != null)
    {
      foreach (var spot in roomController.allSpots)
      {
        if (spot != null)
        {
          spot.OnTargeted += HandleSpotTargeted;
          spot.OnUntargeted += HandleSpotUntargeted;
        }
      }
      Debug.Log("[SpotInfoPanel] Subscribed to all PlacementSpot events");
    }
    else
    {
      Debug.LogWarning("[SpotInfoPanel] roomController is null - hover info won't work");
    }
  }

  private void UnsubscribeFromEvents()
  {
    if (roomController != null)
    {
      foreach (var spot in roomController.allSpots)
      {
        if (spot != null)
        {
          spot.OnTargeted -= HandleSpotTargeted;
          spot.OnUntargeted -= HandleSpotUntargeted;
        }
      }
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Event Handlers
  // ─────────────────────────────────────────────────────────────────────────
  private void HandleSpotTargeted(PlacementSpot spot)
  {
    // Don't show info for occupied spots
    if (spot.isOccupied)
    {
      return;
    }

    Show(spot);
  }

  private void HandleSpotUntargeted(PlacementSpot spot)
  {
    Hide();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Public Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Show panel with info from the specified spot's mini-game.
  /// </summary>
  public void Show(PlacementSpot spot)
  {
    if (rootPanel == null) return;

    // Get game info from MiniGameController
    UpdateContent(spot.triggersGame);

    rootPanel.SetActive(true);
    isShowing = true;

    // Immediate position update
    UpdatePosition();

    Debug.Log($"[SpotInfoPanel] Showing info for {spot.triggersGame}");
  }

  /// <summary>
  /// Hide the panel.
  /// </summary>
  public void Hide()
  {
    if (rootPanel == null) return;

    rootPanel.SetActive(false);
    isShowing = false;
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private Methods
  // ─────────────────────────────────────────────────────────────────────────
  private void UpdateContent(MiniGameType gameType)
  {
    string gameName = gameType.ToString();
    string gameDescription = "";

    // Get info from MiniGameController
    if (miniGameController != null)
    {
      IMiniGame game = miniGameController.GetMiniGame(gameType);
      if (game != null)
      {
        gameName = game.GameName;
        gameDescription = game.GameDescription;
      }
    }

    // Update text fields
    if (nameText != null)
    {
      nameText.text = gameName;
    }

    if (descriptionText != null)
    {
      descriptionText.text = gameDescription;
    }

    if (hintText != null)
    {
      hintText.text = defaultHint;
    }
  }

  private void UpdatePosition()
  {
    if (rectTransform == null || parentCanvas == null) return;

    Vector2 mousePosition = Input.mousePosition;

    // Apply offset (position panel to the right and up from cursor)
    Vector2 targetPosition = mousePosition + offset;

    // Clamp to screen bounds
    targetPosition = ClampToScreen(targetPosition);

    // Convert to canvas space if needed
    if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
    {
      rectTransform.position = targetPosition;
    }
    else
    {
      // For other render modes, use ScreenPointToLocalPointInRectangle
      RectTransformUtility.ScreenPointToLocalPointInRectangle(
          parentCanvas.transform as RectTransform,
          targetPosition,
          parentCanvas.worldCamera,
          out Vector2 localPoint
      );
      rectTransform.localPosition = localPoint;
    }
  }

  private Vector2 ClampToScreen(Vector2 position)
  {
    if (rectTransform == null) return position;

    // Get panel size
    Vector2 panelSize = rectTransform.sizeDelta;

    // Get screen bounds
    float minX = 0;
    float maxX = Screen.width - panelSize.x;
    float minY = 0;
    float maxY = Screen.height - panelSize.y;

    // Clamp position
    position.x = Mathf.Clamp(position.x, minX, maxX);
    position.y = Mathf.Clamp(position.y, minY, maxY);

    return position;
  }
}
