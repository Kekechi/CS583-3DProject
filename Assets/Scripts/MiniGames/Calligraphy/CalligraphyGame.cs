using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Main controller for the Calligraphy mini-game.
/// Phase 1: Spawn paper and detect raycast hits with Debug.Log output.
/// </summary>
public class CalligraphyGame : MonoBehaviour, IMiniGame
{
    // ─────────────────────────────────────────────────────────────────────────
    // State Enum (distinct from GameManager.GameState)
    // ─────────────────────────────────────────────────────────────────────────
    public enum CalligraphyState
    {
        Inactive,           // Game not running
        WaitingToStart,     // Paper spawned, waiting for first input (Phase 1)
        Drawing,            // Player is actively drawing strokes (Phase 2+)
        BetweenStrokes,     // Finished one stroke, waiting for next (Phase 2+)
        Complete            // All strokes finished, showing result (Phase 2+)
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inspector Fields
    // ─────────────────────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private CalligraphyDesign design;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private CameraController cameraController;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask paperLayer;

    [Header("Stroke Detection")]
    [Tooltip("How close to start point to begin drawing")]
    [SerializeField] private float startRadius = 0.15f;

    // ─────────────────────────────────────────────────────────────────────────
    // Runtime State
    // ─────────────────────────────────────────────────────────────────────────
    private CalligraphyState currentState = CalligraphyState.Inactive;
    private CalligraphyPaper currentPaper;
    private float startTime;

    // ─────────────────────────────────────────────────────────────────────────
    // IMiniGame Implementation
    // ─────────────────────────────────────────────────────────────────────────
    public MiniGameType GameType => MiniGameType.Calligraphy;

    // ─────────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        ValidateReferences();
    }

    private void Update()
    {
        if (currentState == CalligraphyState.Inactive)
            return;

        HandleRaycastInput();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Public Methods
    // ─────────────────────────────────────────────────────────────────────────
    public void StartGame()
    {
        Debug.Log("[CalligraphyGame] StartGame() called");

        SpawnPaper();
        startTime = Time.time;

        // Start camera zoom sequence
        StartCoroutine(ZoomToPaperSequence());
    }

    public void StopGame()
    {
        Debug.Log("[CalligraphyGame] StopGame() called");

        currentState = CalligraphyState.Inactive;

        // Cleanup spawned paper
        if (currentPaper != null)
        {
            Destroy(currentPaper.gameObject);
            currentPaper = null;
        }

        Debug.Log("[CalligraphyGame] Game stopped and cleaned up");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private Methods
    // ─────────────────────────────────────────────────────────────────────────
    private void ValidateReferences()
    {
        if (design == null)
            Debug.LogWarning("[CalligraphyGame] CalligraphyDesign not assigned!");

        if (spawnPoint == null)
            Debug.LogWarning("[CalligraphyGame] SpawnPoint not assigned!");

        if (gameCamera == null)
        {
            gameCamera = Camera.main;
            Debug.Log("[CalligraphyGame] Using Camera.main as gameCamera");
        }

        if (cameraController == null)
            Debug.LogWarning("[CalligraphyGame] CameraController not assigned!");

        if (paperLayer == 0)
            Debug.LogWarning("[CalligraphyGame] PaperLayer not set! Raycast won't hit anything.");
    }

    private void SpawnPaper()
    {
        if (design == null || design.paperPrefab == null)
        {
            Debug.LogError("[CalligraphyGame] Cannot spawn paper - design or prefab is null!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("[CalligraphyGame] Cannot spawn paper - spawnPoint is null!");
            return;
        }

        // Instantiate paper at spawn point
        GameObject paperObj = Instantiate(
            design.paperPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        currentPaper = paperObj.GetComponent<CalligraphyPaper>();

        if (currentPaper == null)
        {
            Debug.LogError("[CalligraphyGame] Spawned paper prefab missing CalligraphyPaper component!");
        }

        Debug.Log($"[CalligraphyGame] Paper spawned at {spawnPoint.position}");
    }

    private void HandleRaycastInput()
    {
        // Perform raycast from camera through mouse position
        Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        bool hitPaper = Physics.Raycast(ray, out RaycastHit hit, 100f, paperLayer);

        // Handle based on current state
        switch (currentState)
        {
            case CalligraphyState.WaitingToStart:
                HandleWaitingState(hitPaper, hit);
                break;

            case CalligraphyState.Drawing:
                HandleDrawingState(hitPaper, hit);
                break;
        }
    }

    /// <summary>
    /// Handle input while waiting for player to click near start point.
    /// </summary>
    private void HandleWaitingState(bool hitPaper, RaycastHit hit)
    {
        // Need mouse down to start
        if (!Input.GetMouseButtonDown(0))
            return;

        if (!hitPaper || currentPaper == null)
            return;

        // Check if click is near start point
        Vector3 startPoint = currentPaper.GetCurrentStrokeStart();
        float distance = Vector3.Distance(hit.point, startPoint);

        if (distance <= startRadius)
        {
            // Start drawing!
            currentPaper.StartDrawing();
            currentState = CalligraphyState.Drawing;
            Debug.Log($"[CalligraphyGame] Started drawing! Distance to start: {distance:F3}");
        }
        else
        {
            Debug.Log($"[CalligraphyGame] Click too far from start. Distance: {distance:F3}, Required: {startRadius}");
        }
    }

    /// <summary>
    /// Handle input while player is drawing (mouse held down).
    /// </summary>
    private void HandleDrawingState(bool hitPaper, RaycastHit hit)
    {
        // Update line position if hitting paper
        if (hitPaper && currentPaper != null)
        {
            currentPaper.UpdateLine(hit.point);
        }

        // Check for mouse release
        if (Input.GetMouseButtonUp(0))
        {
            // Cancel stroke (Phase 2 - just reset, Phase 3 will add completion check)
            if (currentPaper != null)
            {
                currentPaper.CancelStroke();
            }
            currentState = CalligraphyState.WaitingToStart;
            Debug.Log("[CalligraphyGame] Mouse released - stroke cancelled (Phase 2)");
        }
    }

    /// <summary>
    /// Coroutine that handles camera zoom to paper after spawn.
    /// </summary>
    private IEnumerator ZoomToPaperSequence()
    {
        currentState = CalligraphyState.Inactive; // Not ready for input yet
        Debug.Log("[CalligraphyGame] Starting zoom sequence...");

        // Get camera position from paper prefab
        if (currentPaper == null)
        {
            Debug.LogError("[CalligraphyGame] No paper spawned, cannot zoom!");
            yield break;
        }

        Transform cameraTarget = currentPaper.GetCameraPosition();
        if (cameraTarget == null)
        {
            Debug.LogWarning("[CalligraphyGame] Paper has no cameraPosition set, skipping zoom");
            currentState = CalligraphyState.WaitingToStart;
            yield break;
        }

        // Move camera to paper's camera position
        if (cameraController != null)
        {
            cameraController.MoveTo(cameraTarget);
            Debug.Log($"[CalligraphyGame] Camera moving to {cameraTarget.name}");

            // Wait for camera to finish moving
            yield return new WaitUntil(() => !cameraController.IsMoving);
            Debug.Log("[CalligraphyGame] Camera arrived at paper");
        }
        else
        {
            Debug.LogWarning("[CalligraphyGame] No CameraController, skipping zoom");
        }

        // Now ready for input
        currentState = CalligraphyState.WaitingToStart;
        Debug.Log($"[CalligraphyGame] State changed to: {currentState}");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Test Methods (Editor Only)
    // ─────────────────────────────────────────────────────────────────────────
    [ContextMenu("Test - Start Game")]
    private void TestStartGame()
    {
        Debug.Log("=== TEST: Starting Calligraphy Game ===");
        StartGame();
    }

    [ContextMenu("Test - Stop Game")]
    private void TestStopGame()
    {
        Debug.Log("=== TEST: Stopping Calligraphy Game ===");
        StopGame();
    }

    [ContextMenu("Test - Log Paper Stroke Points")]
    private void TestLogStrokePoints()
    {
        if (currentPaper == null)
        {
            Debug.LogWarning("No paper spawned - call Test Start Game first");
            return;
        }

        Vector3 start = currentPaper.GetCurrentStrokeStart();
        Vector3 end = currentPaper.GetCurrentStrokeEnd();
        Debug.Log($"Current stroke: Start={start}, End={end}");
    }
}
