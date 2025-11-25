using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Events
    public event Action<GameState, GameState> OnStateChanged;
    public event Action<GameObject> OnItemReadyToPlace;

    public enum GameState
    {
        PlayingMiniGame,
        PlacingItem,
        RoomCompletion
    }

    public GameState CurrentState { get; private set; }
    private bool isTransitioning = false;

    [Header("References")]
    public RoomController roomController;
    public MiniGameController miniGameController;

    [Header("Room Progress")]
    public int itemsPlacedInCurrentRoom = 0;

    void Awake()
    {
        Debug.Log("Game Awake");
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        Debug.Log("[GameManager] OnEnable called");
        // Don't subscribe to events here - serialized refs might not be assigned yet
    }

    void OnDisable()
    {
        Debug.Log("[GameManager] OnDisable called");
        UnsubscribeFromEvents();
    }

    void OnDestroy()
    {
        Debug.Log("[GameManager] OnDestroy called");
        // For DontDestroyOnLoad objects, also cleanup on destroy
        UnsubscribeFromEvents();
    }

    void Start()
    {
        Debug.Log("[GameManager] Start called");
        SubscribeToEvents();
        ChangeState(GameState.PlacingItem);
    }

    void SubscribeToEvents()
    {
        if (miniGameController != null)
        {
            miniGameController.OnMiniGameComplete += HandleMiniGameComplete;
            Debug.Log("[GameManager] Subscribed to MiniGameController events");
        }
        else
        {
            Debug.LogError("[GameManager] miniGameController is NULL in Start!");
        }

        if (roomController != null)
        {
            roomController.OnItemPlaced += HandleItemPlaced;
            roomController.OnRoomComplete += HandleRoomComplete;
            Debug.Log("[GameManager] Subscribed to RoomController events");
        }
        else
        {
            Debug.LogError("[GameManager] roomController is NULL in Start!");
        }
    }

    void UnsubscribeFromEvents()
    {
        if (miniGameController != null)
        {
            miniGameController.OnMiniGameComplete -= HandleMiniGameComplete;
            Debug.Log("[GameManager] Unsubscribed from MiniGameController events");
        }

        if (roomController != null)
        {
            roomController.OnItemPlaced -= HandleItemPlaced;
            roomController.OnRoomComplete -= HandleRoomComplete;
            Debug.Log("[GameManager] Unsubscribed from RoomController events");
        }
    }

    public void ChangeState(GameState newState)
    {
        GameState oldState = CurrentState;
        CurrentState = newState;

        Debug.Log($"[GameManager] State changed: {oldState} → {newState}");
        OnStateChanged?.Invoke(oldState, newState);
    }

    /// <summary>
    /// Called by MiniGameController when a mini-game completes
    /// Extracts the item and fires event for RoomController
    /// </summary>
    void HandleMiniGameComplete(MiniGameResult result)
    {
        // Guard: Only accept completion if we're actually playing a mini-game
        if (CurrentState != GameState.PlayingMiniGame)
        {
            Debug.LogWarning($"[GameManager] Cannot complete mini-game - wrong state: {CurrentState} (expected PlayingMiniGame)");
            return;
        }

        Debug.Log($"[GameManager] Mini-game complete: {result.GameType}, time: {result.CompletionTime:F2}s");

        // Extract item prefab from result
        GameObject itemPrefab = result.ItemInstance;

        if (itemPrefab != null)
        {
            ChangeState(GameState.PlacingItem);
            OnItemReadyToPlace?.Invoke(itemPrefab);
            isTransitioning = false; // Transition complete
            Debug.Log($"[GameManager] Item ready to place: {itemPrefab.name}");
        }
        else
        {
            Debug.LogWarning("[GameManager] Result has no ItemInstance!");
            isTransitioning = false; // Reset even on error
        }
    }

    /// <summary>
    /// Start a mini-game of the specified type
    /// Called by RoomController when a placement spot is clicked
    /// </summary>
    public void StartMiniGame(MiniGameType gameType)
    {
        // Guard: Only allow starting mini-game from PlacingItem state
        if (CurrentState != GameState.PlacingItem)
        {
            Debug.LogWarning($"[GameManager] Cannot start mini-game - wrong state: {CurrentState} (expected PlacingItem)");
            return;
        }

        // Guard: Prevent rapid clicks during transition
        if (isTransitioning)
        {
            Debug.LogWarning("[GameManager] Cannot start mini-game - transition already in progress");
            return;
        }

        // Guard: Validate reference
        if (miniGameController == null)
        {
            Debug.LogError("[GameManager] MiniGameController reference is missing!");
            return;
        }

        // Valid request - proceed
        Debug.Log($"[GameManager] Starting mini-game: {gameType}");
        isTransitioning = true;
        ChangeState(GameState.PlayingMiniGame);
        miniGameController.StartMiniGame(gameType);
    }

    void HandleItemPlaced(PlacementSpot spot, GameObject item)
    {
        itemsPlacedInCurrentRoom++;
        Debug.Log($"[GameManager] Item placed: {itemsPlacedInCurrentRoom}/3");
    }

    void HandleRoomComplete()
    {
        // Guard: Validate we have the expected number of items
        if (itemsPlacedInCurrentRoom < 3)
        {
            Debug.LogWarning($"[GameManager] Room completion triggered early - only {itemsPlacedInCurrentRoom}/3 items placed");
            return;
        }

        Debug.Log("[GameManager] Room complete! All items placed.");
        ChangeState(GameState.RoomCompletion);
        // Future: Transition to next room, show completion UI, etc.
    }

    // ========== TESTING CONTEXT MENU ==========

    [ContextMenu("Test: Start Room Placement")]
    void TestStartRoomPlacement()
    {
        Debug.Log("[TEST] Starting room placement mode");
        itemsPlacedInCurrentRoom = 0;
        ChangeState(GameState.PlacingItem);
    }

    [ContextMenu("Test: Reset Room Progress")]
    void TestResetRoomProgress()
    {
        Debug.Log("[TEST] Resetting room progress");
        itemsPlacedInCurrentRoom = 0;

        if (roomController != null)
        {
            // Reset all placement spots
            foreach (var spot in roomController.allSpots)
            {
                if (spot != null)
                {
                    spot.ClearPlacement();
                }
            }
        }
    }

    [ContextMenu("Test: Start Lantern Game")]
    void TestStartLanternGame()
    {
        Debug.Log("[TEST] Starting Lantern mini-game");
        ChangeState(GameState.PlacingItem); // Ensure correct state for guard
        StartMiniGame(MiniGameType.Lantern);
    }

    [ContextMenu("Test: Start Origami Game")]
    void TestStartOrigamiGame()
    {
        Debug.Log("[TEST] Starting Origami mini-game");
        ChangeState(GameState.PlacingItem); // Ensure correct state for guard
        StartMiniGame(MiniGameType.Origami);
    }

    [ContextMenu("Test: Start Calligraphy Game")]
    void TestCalligraphyGame()
    {
        Debug.Log("[TEST] Starting Calligraphy mini-game");
        ChangeState(GameState.PlacingItem); // Ensure correct state for guard
        StartMiniGame(MiniGameType.Calligraphy);
    }
}
