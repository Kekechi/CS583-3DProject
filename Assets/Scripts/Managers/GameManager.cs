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
        // Subscribe to events
        if (miniGameController != null)
            miniGameController.OnMiniGameComplete += HandleMiniGameComplete;

        if (roomController != null)
        {
            roomController.OnItemPlaced += HandleItemPlaced;
            roomController.OnRoomComplete += HandleRoomComplete;
        }
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        if (miniGameController != null)
            miniGameController.OnMiniGameComplete -= HandleMiniGameComplete;

        if (roomController != null)
        {
            roomController.OnItemPlaced -= HandleItemPlaced;
            roomController.OnRoomComplete -= HandleRoomComplete;
        }
    }

    void Start()
    {
        ChangeState(GameState.PlacingItem);
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
        Debug.Log($"[GameManager] Mini-game complete: {result.GameType}, time: {result.CompletionTime:F2}s");

        // Extract item prefab from result
        GameObject itemPrefab = result.ItemInstance;

        if (itemPrefab != null)
        {
            ChangeState(GameState.PlacingItem);
            OnItemReadyToPlace?.Invoke(itemPrefab);
            Debug.Log($"[GameManager] Item ready to place: {itemPrefab.name}");
        }
        else
        {
            Debug.LogWarning("[GameManager] Result has no ItemInstance!");
        }
    }

    /// <summary>
    /// Start a mini-game of the specified type
    /// Called by RoomController when a placement spot is clicked
    /// </summary>
    public void StartMiniGame(MiniGameType gameType)
    {
        Debug.Log($"[GameManager] Starting mini-game: {gameType}");

        if (miniGameController != null)
        {
            ChangeState(GameState.PlayingMiniGame);
            miniGameController.StartMiniGame(gameType);
        }
        else
        {
            Debug.LogError("[GameManager] MiniGameController reference is missing!");
        }
    }

    void HandleItemPlaced(PlacementSpot spot, GameObject item)
    {
        itemsPlacedInCurrentRoom++;
        Debug.Log($"[GameManager] Item placed: {itemsPlacedInCurrentRoom}/3");
    }

    void HandleRoomComplete()
    {
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
        StartMiniGame(MiniGameType.Lantern);

        if (miniGameController != null)
        {
            miniGameController.StartMiniGame(MiniGameType.Lantern);
        }
    }

    [ContextMenu("Test: Start Origami Game")]
    void TestStartOrigamiGame()
    {
        Debug.Log("[TEST] Starting Origami mini-game");
        StartMiniGame(MiniGameType.Origami);

        if (miniGameController != null)
        {
            miniGameController.StartMiniGame(MiniGameType.Origami);
        }
    }

    [ContextMenu("Test: Start Calligraphy Game")]
    void TestCalligraphyGame()
    {
        Debug.Log("[TEST] Starting Calligraphy mini-game");
        StartMiniGame(MiniGameType.Calligraphy);

        if (miniGameController != null)
        {
            miniGameController.StartMiniGame(MiniGameType.Calligraphy);
        }
    }
}
