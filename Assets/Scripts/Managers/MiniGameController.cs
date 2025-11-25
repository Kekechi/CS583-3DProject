using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    [Header("Current Mini-Game")]
    public MiniGameType currentMiniGame;

    [Header("Mini-Game References")]
    public OrigamiGame origamiGame;
    public CalligraphyGame calligraphyGame;
    public LanternGame lanternGame;

    [Header("Transition References")]
    public CameraController cameraController;

    [Header("Transition Settings")]
    [Tooltip("How long to display success panel before transitioning back")]
    public float successDisplayTime = 2f;

    [Tooltip("Allow player to skip wait with any key press")]
    public bool allowSkip = true;

    // Events
    public event Action<MiniGameResult> OnMiniGameComplete;

    // State
    private bool isTransitioning = false;
    private MiniGameResult currentMiniGameResult;
    private bool skipRequested = false;
    private MiniGameType? pendingGameActivation = null; // Game to activate when camera arrives

    /// <summary>
    /// Check if currently transitioning between mini-game and room
    /// </summary>
    public bool IsTransitioning => isTransitioning;

    void Start()
    {
        Debug.Log("[MiniGameController] Start called");

        // Hide all mini-games initially
        if (origamiGame != null) origamiGame.gameObject.SetActive(false);
        if (calligraphyGame != null) calligraphyGame.gameObject.SetActive(false);
        if (lanternGame != null) lanternGame.gameObject.SetActive(false);

        SubscribeToEvents();
    }

    void OnEnable()
    {
        Debug.Log("[MiniGameController] OnEnable called");
        // Don't subscribe to events here - serialized refs might not be assigned yet
    }

    void OnDisable()
    {
        Debug.Log("[MiniGameController] OnDisable called");
        UnsubscribeFromEvents();
    }

    void SubscribeToEvents()
    {
        // Subscribe to mini-game completion events
        if (lanternGame != null)
        {
            lanternGame.OnGameCompleted += HandleLanternComplete;
            Debug.Log("[MiniGameController] Subscribed to LanternGame events");
        }
        else
        {
            Debug.LogWarning("[MiniGameController] lanternGame is NULL in Start!");
        }

        // Subscribe to camera events
        if (cameraController != null)
        {
            cameraController.OnMovementComplete += HandleCameraArrived;
            Debug.Log("[MiniGameController] Subscribed to CameraController events");
        }
        else
        {
            Debug.LogError("[MiniGameController] cameraController is NULL in Start!");
        }

        // TODO: Subscribe to origami and calligraphy when implemented
        // if (origamiGame != null)
        //     origamiGame.OnGameCompleted += HandleOrigamiComplete;
        // if (calligraphyGame != null)
        //     calligraphyGame.OnGameCompleted += HandleCalligraphyComplete;
    }

    void UnsubscribeFromEvents()
    {
        // Unsubscribe from mini-game events
        if (lanternGame != null)
        {
            lanternGame.OnGameCompleted -= HandleLanternComplete;
            Debug.Log("[MiniGameController] Unsubscribed from LanternGame events");
        }

        // Unsubscribe from camera events
        if (cameraController != null)
        {
            cameraController.OnMovementComplete -= HandleCameraArrived;
            Debug.Log("[MiniGameController] Unsubscribed from CameraController events");
        }

        // TODO: Unsubscribe from other mini-games
    }

    void Update()
    {
        // Listen for skip input during transition
        if (isTransitioning && allowSkip && Input.anyKeyDown)
        {
            skipRequested = true;
        }
    }

    /// <summary>
    /// Start a specific mini-game by type
    /// Called by GameManager when player clicks a placement spot
    /// Camera will fire event when it arrives, then we activate the game
    /// </summary>
    public void StartMiniGame(MiniGameType gameType)
    {
        currentMiniGame = gameType;
        Debug.Log($"[MiniGameController] Starting {gameType} mini-game");

        // Stop any currently running game
        StopCurrentMiniGame();

        // Set pending activation (will trigger when camera event fires)
        pendingGameActivation = gameType;

        // Move camera (HandleCameraArrived will activate game when done)
        MoveCameraToMiniGame(gameType);
    }

    /// <summary>
    /// Event handler: Called when camera movement completes
    /// Activates the pending mini-game if one is waiting
    /// </summary>
    void HandleCameraArrived()
    {
        if (pendingGameActivation.HasValue)
        {
            Debug.Log($"[MiniGameController] Camera arrived, activating {pendingGameActivation.Value} UI");
            ActivateMiniGame(pendingGameActivation.Value);
            pendingGameActivation = null;
        }
    }

    void MoveCameraToMiniGame(MiniGameType gameType)
    {
        if (cameraController == null) return;

        switch (gameType)
        {
            case MiniGameType.Lantern:
                cameraController.MoveTo(cameraController.lanternGamePosition);
                break;
            case MiniGameType.Origami:
                cameraController.MoveTo(cameraController.origamiGamePosition);
                break;
            case MiniGameType.Calligraphy:
                cameraController.MoveTo(cameraController.calligraphyPosition);
                break;
        }
    }

    void ActivateMiniGame(MiniGameType gameType)
    {
        switch (gameType)
        {
            case MiniGameType.Origami:
                if (origamiGame != null)
                {
                    origamiGame.gameObject.SetActive(true);
                    origamiGame.StartGame();
                }
                break;

            case MiniGameType.Calligraphy:
                if (calligraphyGame != null)
                {
                    calligraphyGame.gameObject.SetActive(true);
                    calligraphyGame.StartGame();
                }
                break;

            case MiniGameType.Lantern:
                if (lanternGame != null)
                {
                    lanternGame.gameObject.SetActive(true);
                    lanternGame.StartGame();
                }
                break;
        }

        Debug.Log($"Started {gameType} mini-game");
    }

    /// <summary>
    /// Stop the currently active mini-game and let it clean up its own UI
    /// </summary>
    void StopCurrentMiniGame()
    {
        switch (currentMiniGame)
        {
            case MiniGameType.Lantern:
                if (lanternGame != null)
                    lanternGame.StopGame();
                break;

            case MiniGameType.Origami:
                if (origamiGame != null && origamiGame.gameObject.activeSelf)
                    origamiGame.gameObject.SetActive(false); // TODO: Add StopGame() when implemented
                break;

            case MiniGameType.Calligraphy:
                if (calligraphyGame != null && calligraphyGame.gameObject.activeSelf)
                    calligraphyGame.gameObject.SetActive(false); // TODO: Add StopGame() when implemented
                break;
        }
    }

    /// <summary>
    /// Handle lantern game completion - start transition
    /// </summary>
    void HandleLanternComplete(LanternResult result)
    {
        StartCoroutine(HandleMiniGameCompletion(result));
    }

    /// <summary>
    /// Orchestrate the full post-game transition: success display → camera return → cleanup
    /// </summary>
    IEnumerator HandleMiniGameCompletion(MiniGameResult result)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[MiniGameController] Already transitioning, ignoring completion");
            yield break;
        }

        isTransitioning = true;
        currentMiniGameResult = result;
        skipRequested = false;

        try
        {
            Debug.Log($"[MiniGameController] Starting transition for {currentMiniGame}");

            // Phase 1: Show success UI (already shown by mini-game)
            // Just wait for display time or skip
            float elapsed = 0f;
            while (elapsed < successDisplayTime && !skipRequested)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.Log($"[MiniGameController] Success displayed for {elapsed:F2}s (skipped: {skipRequested})");

            // Phase 2: Hide mini-game UI before camera transition
            // Tell the specific game to stop and clean itself up
            StopCurrentMiniGame();

            Debug.Log("[MiniGameController] Mini-game UI hidden");

            // Phase 3: Move camera back to room
            if (cameraController != null)
            {
                cameraController.MoveTo(cameraController.roomViewPosition);

                // Wait for camera to finish moving
                yield return new WaitUntil(() => !cameraController.IsMoving);
            }

            Debug.Log("[MiniGameController] Camera returned to room");

            // Phase 4: Fire completion event
            if (currentMiniGameResult != null)
            {
                OnMiniGameComplete?.Invoke(currentMiniGameResult);
                Debug.Log($"[MiniGameController] Transition complete, fired OnMiniGameComplete event");
            }
            else
            {
                Debug.LogWarning("[MiniGameController] Cannot complete transition - result is null");
            }
        }
        finally
        {
            // Always reset state, even if coroutine errors or stops early
            isTransitioning = false;
            skipRequested = false;
            currentMiniGameResult = null;
        }
    }
}
