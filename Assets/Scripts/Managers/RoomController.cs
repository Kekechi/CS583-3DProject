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

    void OnEnable()
    {
        Debug.Log("[RoomController] OnEnable called");
        // Don't subscribe to events here - serialized refs might not be assigned yet
    }

    void OnDisable()
    {
        Debug.Log("[RoomController] OnDisable called");
        UnsubscribeFromEvents();
    }

    void Start()
    {
        Debug.Log("[RoomController] Start called");

        // Reset room state on scene load (ensures fresh start)
        ResetRoom();

        InitializeRoom();
        SubscribeToEvents();
    }

    void SubscribeToEvents()
    {
        // Subscribe to all spot events
        SubscribeToSpots();

        // Subscribe to GameManager events
        if (gameManager != null)
        {
            gameManager.OnItemReadyToPlace += HandleItemReadyToPlace;
            Debug.Log("[RoomController] Subscribed to GameManager.OnItemReadyToPlace");
        }
        else
        {
            Debug.LogError("[RoomController] gameManager reference is NULL in Start!");
        }
    }

    void UnsubscribeFromEvents()
    {
        // Unsubscribe from spot events
        UnsubscribeFromSpots();

        // Unsubscribe from GameManager events
        if (gameManager != null)
        {
            gameManager.OnItemReadyToPlace -= HandleItemReadyToPlace;
            Debug.Log("[RoomController] Unsubscribed from GameManager.OnItemReadyToPlace");
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

        // Show ghost visuals only for unoccupied spots
        foreach (var spot in allSpots)
        {
            if (spot != null)
            {
                spot.ShowGhost(!spot.isOccupied);
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
        if (gameManager.CurrentState != GameManager.GameState.PlacingItem) return;
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
    /// Places the item at the stored spot and applies customization data
    /// </summary>
    void HandleItemReadyToPlace(GameObject itemPrefab, MiniGameResult result)
    {
        Debug.Log($"[RoomController] HandleItemReadyToPlace called with item: {(itemPrefab != null ? itemPrefab.name : "NULL")}");

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

        PlaceItemAtSpot(currentTriggeredSpot, itemPrefab, result);
    }

    /// <summary>
    /// Place an item at the specified spot and apply customization from mini-game result
    /// </summary>
    void PlaceItemAtSpot(PlacementSpot spot, GameObject itemPrefab, MiniGameResult result)
    {
        // Use itemAnchor if available for precise positioning, otherwise use spot transform
        Transform anchor = spot.itemAnchor != null ? spot.itemAnchor : spot.transform;

        // Instantiate the item at the anchor's position
        GameObject item = Instantiate(itemPrefab);
        item.transform.position = anchor.position;
        item.transform.rotation = Quaternion.Euler(anchor.rotation.eulerAngles + item.transform.rotation.eulerAngles);

        // Apply customization if the item supports it
        ICustomizableItem customizable = item.GetComponent<ICustomizableItem>();
        if (customizable != null && result != null)
        {
            customizable.ApplyCustomization(result);
            Debug.Log($"[RoomController] Applied customization to {item.name}");
        }
        else if (result != null)
        {
            Debug.LogWarning($"[RoomController] Item {item.name} does not support customization (no ICustomizableItem component)");
        }

        // Parent to item container for organization
        if (itemParent != null)
        {
            item.transform.SetParent(itemParent);
        }

        // Mark the spot as occupied
        spot.MarkOccupied(item);

        // Disable all other spots that trigger the same mini-game type
        DisableSpotsForMiniGame(spot.triggersGame);

        // Track the placement
        placedItems[spot] = item;
        itemsPlaced++;

        Debug.Log($"[RoomController] Item placed at {spot.gameObject.name}. Progress: {itemsPlaced}/{totalRequiredItems}");

        // Fire event
        OnItemPlaced?.Invoke(spot, item);

        // Check for room completion
        CheckRoomCompletion();

        // Clear the stored spot
        currentTriggeredSpot = null;
    }

    /// <summary>
    /// Disable all spots that trigger a specific mini-game type.
    /// Called when a mini-game is completed to hide ghosts from all related spots.
    /// </summary>
    void DisableSpotsForMiniGame(MiniGameType gameType)
    {
        if (allSpots == null) return;

        foreach (var spot in allSpots)
        {
            if (spot != null && spot.triggersGame == gameType && !spot.isOccupied)
            {
                // Disable the spot (hides ghost, blocks interactions)
                spot.Disable();
                Debug.Log($"[RoomController] Disabled spot {spot.gameObject.name} (same mini-game type: {gameType})");
            }
        }
    }

    /// <summary>
    /// Check if all required items are placed
    /// </summary>
    void CheckRoomCompletion()
    {
        if (itemsPlaced >= totalRequiredItems)
        {
            Debug.Log("[RoomController] All items placed - waiting for player to click Finish Room button");
            // NOTE: We no longer auto-fire OnRoomComplete here.
            // The ProgressChecklistUI shows the Finish Room button, and player clicks it
            // to trigger GameManager.ChangeState(RoomCompletion).
            // OnRoomComplete?.Invoke();
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
    /// Reset the room to initial state (for replay)
    /// Called on scene Start() to ensure fresh state
    /// </summary>
    public void ResetRoom()
    {
        Debug.Log("[RoomController] Resetting room...");

        // Destroy all placed items
        if (placedItems != null)
        {
            foreach (var kvp in placedItems)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
            placedItems.Clear();
        }

        // Reset all placement spots
        if (allSpots != null)
        {
            foreach (var spot in allSpots)
            {
                if (spot != null)
                {
                    spot.ClearPlacement();
                }
            }
        }

        // Reset counters
        itemsPlaced = 0;
        currentTriggeredSpot = null;

        Debug.Log("[RoomController] Room reset complete");
    }

    /// <summary>
    /// Check if a specific spot is occupied
    /// </summary>
    public bool IsSpotOccupied(PlacementSpot spot)
    {
        return spot != null && spot.isOccupied;
    }

}
