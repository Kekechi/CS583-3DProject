using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays task completion checklist (0/3 → 3/3).
/// Updates when items are placed in the room.
/// </summary>
public class ProgressChecklistUI : MonoBehaviour
{
  // ─────────────────────────────────────────────────────────────────────────
  // Inspector Fields
  // ─────────────────────────────────────────────────────────────────────────
  [Header("Checklist Items")]
  [SerializeField] private Image lanternCheckbox;
  [SerializeField] private TMP_Text lanternLabel;

  [SerializeField] private Image origamiCheckbox;
  [SerializeField] private TMP_Text origamiLabel;

  [SerializeField] private Image calligraphyCheckbox;
  [SerializeField] private TMP_Text calligraphyLabel;

  [Header("Visual Settings")]
  [SerializeField] private Sprite uncheckedSprite;
  [SerializeField] private Sprite checkedSprite;
  [SerializeField] private Color incompleteColor = new Color(0.5f, 0.5f, 0.5f, 1f);
  [SerializeField] private Color completeColor = new Color(0.2f, 0.2f, 0.2f, 1f);

  [Header("Finish Room")]
  [SerializeField] private GameObject finishRoomButton;

  [Header("References")]
  [SerializeField] private RoomController roomController;

  // ─────────────────────────────────────────────────────────────────────────
  // Runtime State
  // ─────────────────────────────────────────────────────────────────────────
  private bool lanternComplete = false;
  private bool origamiComplete = false;
  private bool calligraphyComplete = false;

  // ─────────────────────────────────────────────────────────────────────────
  // Unity Lifecycle
  // ─────────────────────────────────────────────────────────────────────────
  private void Start()
  {
    ResetAll();
    SubscribeToEvents();

    Debug.Log("[ProgressChecklistUI] Initialized");
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
    if (roomController != null)
    {
      roomController.OnItemPlaced += HandleItemPlaced;
      Debug.Log("[ProgressChecklistUI] Subscribed to RoomController.OnItemPlaced");
    }
    else
    {
      Debug.LogWarning("[ProgressChecklistUI] roomController is null - checklist won't update");
    }
  }

  private void UnsubscribeFromEvents()
  {
    if (roomController != null)
    {
      roomController.OnItemPlaced -= HandleItemPlaced;
    }
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Event Handlers
  // ─────────────────────────────────────────────────────────────────────────
  private void HandleItemPlaced(PlacementSpot spot, GameObject item)
  {
    Debug.Log($"[ProgressChecklistUI] Item placed at spot triggering {spot.triggersGame}");

    // Mark the appropriate game as complete
    MarkComplete(spot.triggersGame);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Public Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Mark a specific mini-game as complete.
  /// </summary>
  public void MarkComplete(MiniGameType gameType)
  {
    switch (gameType)
    {
      case MiniGameType.Lantern:
        if (!lanternComplete)
        {
          lanternComplete = true;
          UpdateItemVisual(lanternCheckbox, lanternLabel, true);
          Debug.Log("[ProgressChecklistUI] Lantern marked complete");
        }
        break;

      case MiniGameType.Origami:
        if (!origamiComplete)
        {
          origamiComplete = true;
          UpdateItemVisual(origamiCheckbox, origamiLabel, true);
          Debug.Log("[ProgressChecklistUI] Origami marked complete");
        }
        break;

      case MiniGameType.Calligraphy:
        if (!calligraphyComplete)
        {
          calligraphyComplete = true;
          UpdateItemVisual(calligraphyCheckbox, calligraphyLabel, true);
          Debug.Log("[ProgressChecklistUI] Calligraphy marked complete");
        }
        break;
    }

    // Check if all complete to show finish button
    CheckAllComplete();
  }

  /// <summary>
  /// Reset all items to incomplete (for replay).
  /// </summary>
  public void ResetAll()
  {
    lanternComplete = false;
    origamiComplete = false;
    calligraphyComplete = false;

    UpdateItemVisual(lanternCheckbox, lanternLabel, false);
    UpdateItemVisual(origamiCheckbox, origamiLabel, false);
    UpdateItemVisual(calligraphyCheckbox, calligraphyLabel, false);

    // Hide finish button
    if (finishRoomButton != null)
    {
      finishRoomButton.SetActive(false);
    }

    Debug.Log("[ProgressChecklistUI] Reset all items to incomplete");
  }

  /// <summary>
  /// Check how many tasks are complete.
  /// </summary>
  public int GetCompletedCount()
  {
    int count = 0;
    if (lanternComplete) count++;
    if (origamiComplete) count++;
    if (calligraphyComplete) count++;
    return count;
  }

  /// <summary>
  /// Check if all tasks are complete.
  /// </summary>
  public bool IsAllComplete()
  {
    return lanternComplete && origamiComplete && calligraphyComplete;
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Private Methods
  // ─────────────────────────────────────────────────────────────────────────

  /// <summary>
  /// Check if all tasks complete and show finish button.
  /// </summary>
  private void CheckAllComplete()
  {
    if (IsAllComplete())
    {
      if (finishRoomButton != null)
      {
        finishRoomButton.SetActive(true);
        Debug.Log("[ProgressChecklistUI] All tasks complete - showing Finish Room button");
      }
    }
  }

  /// <summary>
  /// Called when player clicks the Finish Room button.
  /// Wire this to the button's OnClick event in Inspector.
  /// </summary>
  public void OnFinishRoomClicked()
  {
    Debug.Log("[ProgressChecklistUI] Finish Room clicked");

    if (GameManager.Instance != null)
    {
      GameManager.Instance.ChangeState(GameManager.GameState.RoomCompletion);
    }
    else
    {
      Debug.LogError("[ProgressChecklistUI] GameManager.Instance is null!");
    }
  }

  private void UpdateItemVisual(Image checkbox, TMP_Text label, bool isComplete)
  {
    // Update checkbox sprite
    if (checkbox != null)
    {
      if (isComplete && checkedSprite != null)
      {
        checkbox.sprite = checkedSprite;
      }
      else if (!isComplete && uncheckedSprite != null)
      {
        checkbox.sprite = uncheckedSprite;
      }
    }

    // Update label color
    if (label != null)
    {
      label.color = isComplete ? completeColor : incompleteColor;
    }
  }
}
