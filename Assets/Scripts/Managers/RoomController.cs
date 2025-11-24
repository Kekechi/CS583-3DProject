using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the room exploration phase, placement spots, and item placement
/// Tracks progress and coordinates with GameManager
/// </summary>
public class RoomController : MonoBehaviour
{
    // Events
    public event Action<PlacementSpot, GameObject> OnItemPlaced;
    public event Action OnRoomComplete;

    [Header("Spot References")]
    [Tooltip("All placement spots in the room (assign in Inspector)")]
    public List<PlacementSpot> allSpots;

    [Header("Item Management")]
    [Tooltip("Parent transform for placed items (for organization)")]
    public Transform itemParent;

    [Header("Settings")]
    [Tooltip("How many items required to complete the room")]
    public int totalRequiredItems = 3;

    [Header("Manager References")]
    public GameManager gameManager; // Only for StartMiniGame - consider making this an event too

    // Runtime state
    private int itemsPlaced = 0;
    private PlacementSpot currentTriggeredSpot;
    private Dictionary<PlacementSpot, GameObject> placedItems;

    void Awake()
    {
        placedItems = new Dictionary<PlacementSpot, GameObject>();
    }

    void Start()
    {
        InitializeRoom();
    }

    void OnEnable()
    {
        // Subscribe to all spot events
        SubscribeToSpots();

        // Subscribe to GameManager events
        if (gameManager != null)
        {
            gameManager.OnItemReadyToPlace += HandleItemReadyToPlace;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from spot events
        UnsubscribeFromSpots();

        // Unsubscribe from GameManager events
        if (gameManager != null)
        {
            gameManager.OnItemReadyToPlace -= HandleItemReadyToPlace;
        }
    }

    /// <summary>
    /// Initialize the room - subscribe to spots, show ghosts
    /// </summary>
    void InitializeRoom()
    {
        Debug.Log("[RoomController] Initializing room");

        // Validate spots
        if (allSpots == null || allSpots.Count == 0)
        {
            Debug.LogWarning("[RoomController] No placement spots assigned!");
            return;
        }

        // Show all ghost visuals
        foreach (var spot in allSpots)
        {
            if (spot != null)
            {
                spot.ShowGhost(true);
            }
        }

        Debug.Log($"[RoomController] Room initialized with {allSpots.Count} spots");
    }

    /// <summary>
    /// Subscribe to all spot click events
    /// </summary>
    void SubscribeToSpots()
    {
        if (allSpots == null) return;

        foreach (var spot in allSpots)
        {
            if (spot != null)
            {
                spot.OnClicked += HandleSpotClicked;
            }
        }
    }

    /// <summary>
    /// Unsubscribe from all spot events
    /// </summary>
    void UnsubscribeFromSpots()
    {
        if (allSpots == null) return;

        foreach (var spot in allSpots)
        {
            if (spot != null)
            {
                spot.OnClicked -= HandleSpotClicked;
            }
        }
    }

    /// <summary>
    /// Handle when a placement spot is clicked
    /// Store the spot and request mini-game start from GameManager
    /// </summary>
    void HandleSpotClicked(PlacementSpot spot)
    {
        Debug.Log($"[RoomController] Spot clicked: {spot.gameObject.name} (triggers {spot.triggersGame})");

        // Store which spot triggered this mini-game
        currentTriggeredSpot = spot;

        // Request GameManager to start the appropriate mini-game
        if (gameManager != null)
        {
            gameManager.StartMiniGame(spot.triggersGame);
        }
        else
        {
            Debug.LogWarning("[RoomController] GameManager reference missing!");
        }
    }

    /// <summary>
    /// Event handler: Called when GameManager has an item ready to place
    /// Places the item at the stored spot
    /// </summary>
    void HandleItemReadyToPlace(GameObject itemPrefab)
    {
        if (currentTriggeredSpot == null)
        {
            Debug.LogWarning("[RoomController] No triggered spot stored!");
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning("[RoomController] Item prefab is null!");
            return;
        }

        PlaceItemAtSpot(currentTriggeredSpot, itemPrefab);
    }

    /// <summary>
    /// Place an item at the specified spot
    /// </summary>
    void PlaceItemAtSpot(PlacementSpot spot, GameObject itemPrefab)
    {
        // Instantiate the item at the spot's position
        GameObject item = Instantiate(itemPrefab, spot.transform.position, spot.transform.rotation);

        // Parent to item container for organization
        if (itemParent != null)
        {
            item.transform.SetParent(itemParent);
        }

        // Mark the spot as occupied
        spot.MarkOccupied(item);

        // Track the placement
        placedItems[spot] = item;
        itemsPlaced++;

        Debug.Log($"[RoomController] Item placed at {spot.gameObject.name}. Progress: {itemsPlaced}/{totalRequiredItems}");

        // Fire event
        OnItemPlaced?.Invoke(spot, item);

        // Update harmony meter
        UpdateHarmonyMeter(itemsPlaced);

        // Check for room completion
        CheckRoomCompletion();

        // Clear the stored spot
        currentTriggeredSpot = null;
    }

    /// <summary>
    /// Check if all required items are placed
    /// </summary>
    void CheckRoomCompletion()
    {
        if (itemsPlaced >= totalRequiredItems)
        {
            Debug.Log("[RoomController] Room complete!");
            OnRoomComplete?.Invoke();
            // GameManager subscribes to OnRoomComplete event and changes its own state
        }
    }

    /// <summary>
    /// Get current harmony percentage (0-1)
    /// </summary>
    public float GetHarmonyPercentage()
    {
        return (float)itemsPlaced / totalRequiredItems;
    }

    /// <summary>
    /// Get number of items placed
    /// </summary>
    public int GetItemsPlaced()
    {
        return itemsPlaced;
    }

    /// <summary>
    /// Check if a specific spot is occupied
    /// </summary>
    public bool IsSpotOccupied(PlacementSpot spot)
    {
        return spot != null && spot.isOccupied;
    }

    // ===== LEGACY METHODS (for GameManager compatibility) =====

    public void UpdateHarmonyMeter(int itemsPlaced)
    {
        Debug.Log($"[RoomController] UpdateHarmonyMeter: {itemsPlaced}/{totalRequiredItems}");
        // TODO: Update UI harmony meter when implemented

        if (itemsPlaced >= totalRequiredItems)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayHarmonyChime();
        }
    }
}
