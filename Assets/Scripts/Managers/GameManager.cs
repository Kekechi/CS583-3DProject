using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Events for external systems to trigger
    public static event Action OnMiniGameCompleted;
    public static event Action OnItemPlacedEvent;
    public static event Action OnPauseRequested;

    public enum GameState
    {
        MainMenu,
        SelectingRoom,
        PlayingMiniGame,
        PlacingItem,
        RoomCompletion,
        Paused
    }

    public GameState CurrentState { get; private set; }

    [Header("References")]
    public UIManager uiManager;
    public AudioManager audioManager;
    public RoomController roomController;
    public MiniGameController miniGameController;

    [Header("Room Progress")]
    public int currentRoomIndex = 0;
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
        // OnMiniGameCompleted += HandleMiniGameComplete; // Now handled by MiniGameController transition
        OnItemPlacedEvent += HandleItemPlaced;
        OnPauseRequested += PauseGame;

        // Subscribe to MiniGameController's completion event
        if (miniGameController != null)
            miniGameController.OnMiniGameFullyComplete += HandleMiniGameFullyComplete;
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        // OnMiniGameCompleted -= HandleMiniGameComplete;
        OnItemPlacedEvent -= HandleItemPlaced;
        OnPauseRequested -= PauseGame;

        if (miniGameController != null)
            miniGameController.OnMiniGameFullyComplete -= HandleMiniGameFullyComplete;
    }

    void Start()
    {
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"Game State Changed to: {newState}");

        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenu();
                break;
            case GameState.SelectingRoom:
                HandleRoomSelection();
                break;
            case GameState.PlayingMiniGame:
                HandleMiniGame();
                break;
            case GameState.PlacingItem:
                HandlePlacement();
                break;
            case GameState.RoomCompletion:
                HandleRoomCompletion();
                break;
            case GameState.Paused:
                HandlePause();
                break;
        }
    }

    void HandleMainMenu()
    {
        if (uiManager != null) uiManager.ShowMainMenu();
    }

    void HandleRoomSelection()
    {
        if (uiManager != null) uiManager.ShowRoomSelection();
    }

    void HandleMiniGame()
    {
        if (miniGameController != null) miniGameController.StartCurrentMiniGame();
    }

    void HandlePlacement()
    {
        if (roomController != null) roomController.ShowPlacementUI();
    }

    void HandleRoomCompletion()
    {
        if (roomController != null) roomController.PlayCompletionSequence();
    }

    void HandlePause()
    {
        Time.timeScale = 0f;
        if (uiManager != null) uiManager.ShowPauseMenu();
    }

    public void StartGame()
    {
        currentRoomIndex = 0;
        itemsPlacedInCurrentRoom = 0;
        ChangeState(GameState.SelectingRoom);
    }

    // Event handlers (private - triggered by events)
    private void HandleMiniGameFullyComplete(object result)
    {
        Debug.Log($"[GameManager] Mini-game fully complete with result: {result?.GetType().Name}");

        // Store result for placement (can be accessed by RoomController)
        // For now, just transition to placement state
        // RoomController will handle the actual placement UI and logic

        ChangeState(GameState.PlacingItem);
    }

    private void HandleItemPlaced()
    {
        itemsPlacedInCurrentRoom++;
        if (roomController != null) roomController.UpdateHarmonyMeter(itemsPlacedInCurrentRoom);

        if (itemsPlacedInCurrentRoom >= 3) // 3 items per room (Origami, Calligraphy, Lantern)
        {
            ChangeState(GameState.RoomCompletion);
        }
        else
        {
            ChangeState(GameState.PlayingMiniGame);
        }
    }

    // Public methods for compatibility (invoke events)
    public void OnMiniGameComplete() => OnMiniGameCompleted?.Invoke();
    public void OnItemPlaced() => OnItemPlacedEvent?.Invoke();

    public void PauseGame()
    {
        ChangeState(GameState.Paused);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.PlayingMiniGame);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.MainMenu);
    }
}
