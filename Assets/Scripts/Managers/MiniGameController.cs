using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameController : MonoBehaviour
{
    public enum MiniGameType
    {
        Origami,
        Calligraphy,
        Lantern
    }

    [Header("Current Mini-Game")]
    public MiniGameType currentMiniGame;
    private int miniGameRotationIndex = 0;

    [Header("Mini-Game References")]
    public OrigamiGame origamiGame;
    public CalligraphyGame calligraphyGame;
    public LanternGame lanternGame;

    [Header("Transition References")]
    public CameraController cameraController;
    public UIManager uiManager;

    [Header("Transition Settings")]
    [Tooltip("How long to display success panel before transitioning back")]
    public float successDisplayTime = 2f;

    [Tooltip("Allow player to skip wait with any key press")]
    public bool allowSkip = true;

    // Events
    public event Action<object> OnMiniGameFullyComplete;

    // State
    private bool isTransitioning = false;
    private object currentMiniGameResult;
    private bool skipRequested = false;

    void Start()
    {
        // Hide all mini-games initially
        DeactivateAllMiniGames();
    }

    void OnEnable()
    {
        // Subscribe to mini-game completion events
        if (lanternGame != null)
            lanternGame.OnGameCompleted += HandleLanternComplete;

        // TODO: Subscribe to origami and calligraphy when implemented
        // if (origamiGame != null)
        //     origamiGame.OnGameCompleted += HandleOrigamiComplete;
        // if (calligraphyGame != null)
        //     calligraphyGame.OnGameCompleted += HandleCalligraphyComplete;
    }

    void OnDisable()
    {
        // Unsubscribe from events
        if (lanternGame != null)
            lanternGame.OnGameCompleted -= HandleLanternComplete;

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

    public void StartCurrentMiniGame()
    {
        // Cycle through mini-games in order
        MiniGameType[] gameOrder = { MiniGameType.Origami, MiniGameType.Calligraphy, MiniGameType.Lantern };
        currentMiniGame = gameOrder[miniGameRotationIndex % 3];
        miniGameRotationIndex++;

        DeactivateAllMiniGames();
        MoveCameraToMiniGame(currentMiniGame);
        ActivateMiniGame(currentMiniGame);
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

    void DeactivateAllMiniGames()
    {
        if (origamiGame != null) origamiGame.gameObject.SetActive(false);
        if (calligraphyGame != null) calligraphyGame.gameObject.SetActive(false);
        if (lanternGame != null) lanternGame.gameObject.SetActive(false);
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
    IEnumerator HandleMiniGameCompletion(object result)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[MiniGameController] Already transitioning, ignoring completion");
            yield break;
        }

        isTransitioning = true;
        currentMiniGameResult = result;
        skipRequested = false;

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

        // Phase 2: Move camera back to room (success UI still visible during camera move)
        if (cameraController != null)
        {
            cameraController.MoveTo(cameraController.roomViewPosition);

            // Wait for camera to finish moving
            yield return new WaitUntil(() => !cameraController.IsMoving);
        }

        Debug.Log("[MiniGameController] Camera returned to room");

        // Phase 3: Cleanup mini-game (hide UI, deactivate game object)
        DeactivateAllMiniGames();

        // Phase 4: Notify GameManager that transition is complete
        isTransitioning = false;
        OnMiniGameFullyComplete?.Invoke(currentMiniGameResult);

        Debug.Log($"[MiniGameController] Transition complete, result passed to GameManager");
    }

    /// <summary>
    /// Old method for compatibility - no longer used, transitions now automatic
    /// </summary>
    [System.Obsolete("Use automatic transition via OnGameCompleted events instead")]
    public void OnMiniGameComplete()
    {
        Debug.LogWarning("[MiniGameController] OnMiniGameComplete() is obsolete - transitions are now automatic");
    }

    public void ResetMiniGameRotation()
    {
        miniGameRotationIndex = 0;
    }
}
