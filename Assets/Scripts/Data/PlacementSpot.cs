using System;
using UnityEngine;

/// <summary>
/// Represents a single placement location in the room.
/// Fires events when targeted, untargeted, or clicked.
/// </summary>
public class PlacementSpot : MonoBehaviour
{
  // Events
  public event Action<PlacementSpot> OnTargeted;
  public event Action<PlacementSpot> OnUntargeted;
  public event Action<PlacementSpot> OnClicked;
  [Header("Spot Configuration")]
  [Tooltip("Which mini-game does this spot trigger when clicked")]
  public MiniGameType triggersGame;

  [Header("Visual References")]
  [Tooltip("Reference to the ghost visual child GameObject")]
  public GameObject ghostVisual;

  [Tooltip("Reference to the highlight visual (optional)")]
  public GameObject highlightVisual;

  [Tooltip("Optional anchor transform for precise item placement (e.g., bottom of lantern aligns here). If null, uses spot's transform.")]
  public Transform itemAnchor;

  [Header("Runtime State")]
  [Tooltip("Is this spot currently occupied by a placed item?")]
  public bool isOccupied = false;

  // Runtime reference to the placed item
  private GameObject placedItem;

  // Internal state: is this spot currently being looked at?
  private bool isTargeted = false;

  /// <summary>
  /// Show or hide the ghost visual
  /// </summary>
  public void ShowGhost(bool show)
  {
    if (ghostVisual != null)
    {
      ghostVisual.SetActive(show);
    }
    else
    {
      Debug.LogWarning($"[PlacementSpot] {gameObject.name}: ghostVisual is not assigned!");
    }
  }

  /// <summary>
  /// Set whether this spot is currently being targeted by camera
  /// Spot manages its own highlight state and fires events
  /// </summary>
  public void SetTargeted(bool targeted)
  {
    // Only update if state changed
    if (isTargeted != targeted)
    {
      isTargeted = targeted;

      if (highlightVisual != null)
      {
        highlightVisual.SetActive(targeted);
      }

      // Fire appropriate event
      if (targeted)
      {
        OnTargeted?.Invoke(this);
        Debug.Log($"[PlacementSpot] {gameObject.name} targeted - triggers {triggersGame}");
      }
      else
      {
        OnUntargeted?.Invoke(this);
        Debug.Log($"[PlacementSpot] {gameObject.name} untargeted");
      }
    }
  }

  /// <summary>
  /// Called when this spot is clicked
  /// Fires OnClicked event if not occupied
  /// </summary>
  public void Click()
  {
    if (!isOccupied)
    {
      OnClicked?.Invoke(this);
      Debug.Log($"[PlacementSpot] {gameObject.name} clicked - starting {triggersGame} mini-game");
    }
    else
    {
      Debug.LogWarning($"[PlacementSpot] {gameObject.name} is occupied, cannot click");
    }
  }

  /// <summary>
  /// Unity mouse click event - handles direct mouse clicks on the collider
  /// </summary>
  void OnMouseDown()
  {
    Click();
  }

  /// <summary>
  /// Unity mouse enter event - handles mouse hover
  /// </summary>
  void OnMouseEnter()
  {
    SetTargeted(true);
  }

  /// <summary>
  /// Unity mouse exit event - handles mouse leaving the collider
  /// </summary>
  void OnMouseExit()
  {
    SetTargeted(false);
  }

  /// <summary>
  /// Mark this spot as occupied and hide the ghost
  /// </summary>
  public void MarkOccupied(GameObject item)
  {
    isOccupied = true;
    placedItem = item;
    ShowGhost(false);

    Debug.Log($"[PlacementSpot] {gameObject.name} is now occupied by {item.name}");
  }

  /// <summary>
  /// Get the placed item (null if not occupied)
  /// </summary>
  public GameObject GetPlacedItem()
  {
    return placedItem;
  }

  /// <summary>
  /// Clear placement (for testing/reset)
  /// </summary>
  public void ClearPlacement()
  {
    if (placedItem != null)
    {
      Destroy(placedItem);
      placedItem = null;
    }

    isOccupied = false;
    ShowGhost(true);

    Debug.Log($"[PlacementSpot] {gameObject.name} cleared");
  }

  void OnValidate()
  {
    // Editor validation: warn if ghost visual is missing
    if (ghostVisual == null)
    {
      Debug.LogWarning($"[PlacementSpot] {gameObject.name}: Ghost visual is not assigned! Please assign a child GameObject.");
    }
  }

  void OnDrawGizmos()
  {
    // Show spot position in Scene view
    Gizmos.color = isOccupied ? Color.red : Color.green;
    Gizmos.DrawWireSphere(transform.position, 0.2f);

    // Draw arrow pointing up
    Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
  }
}
