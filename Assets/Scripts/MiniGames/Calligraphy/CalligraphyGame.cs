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
        Inactive,              // Game not running
        TransitioningToWide,   // Camera moving to wide view
        ShowingFullPaper,      // Pausing on wide view to show phrase
        TransitioningToZoom,   // Camera moving to zoomed view
        WaitingToStart,        // Ready for player input
        Drawing,               // Player is actively drawing strokes
        TransitioningBackWide, // Camera returning to wide after stroke
        Complete,              // All strokes finished
        BetweenStrokes         // Future: multi-stroke support
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inspector Fields
    // ─────────────────────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CalligraphyUI calligraphyUI;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask paperLayer;

    [Header("Stroke Detection")]
    [Tooltip("How close to start point to begin drawing")]
    [SerializeField] private float startRadius = 0.15f;

    [Tooltip("How close to end point to complete stroke (more forgiving)")]
    [SerializeField] private float endRadius = 0.2f;

    [Header("Corner Validation")]
    [Tooltip("Track player path for corner validation")]
    [SerializeField] private int pathSampleRate = 10; // Sample every N frames

    // Path tracking for corner validation
    private System.Collections.Generic.List<Vector3> playerPath = new System.Collections.Generic.List<Vector3>();
    private int frameCounter = 0;

    [Header("Timing")]
    [Tooltip("Time to show full paper before zooming to stroke")]
    [SerializeField] private float initialPauseTime = 1.0f;

    // ─────────────────────────────────────────────────────────────────────────
    // Runtime State
    // ─────────────────────────────────────────────────────────────────────────
    private CalligraphyDesign design; // Runtime-selected design
    private CalligraphyState currentState = CalligraphyState.Inactive;
    private CalligraphyPaper currentPaper;
    private float startTime;
    private Coroutine gameSequenceCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    // Events
    // ─────────────────────────────────────────────────────────────────────────
    public event Action<CalligraphyResult> OnGameCompleted;

    // ─────────────────────────────────────────────────────────────────────────
    // IMiniGame Implementation
    // ─────────────────────────────────────────────────────────────────────────
    public MiniGameType GameType => MiniGameType.Calligraphy;
    public string GameName => "Calligraphy";
    public string GameDescription => "Trace the brush strokes";

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

        // Select design based on unlock status
        design = SelectDesign();

        // 1. Spawn paper first (can happen while camera transitions)
        SpawnPaper();
        startTime = Time.time;

        // 2. Show UI (rootPanel active, success panel hidden until completion)
        if (calligraphyUI != null)
        {
            calligraphyUI.Show();
        }

        // 3. Start the full game sequence
        if (gameSequenceCoroutine != null)
            StopCoroutine(gameSequenceCoroutine);
        gameSequenceCoroutine = StartCoroutine(GameSequence());
    }

    public void StopGame()
    {
        Debug.Log("[CalligraphyGame] StopGame() called");

        // Stop any running sequence
        if (gameSequenceCoroutine != null)
        {
            StopCoroutine(gameSequenceCoroutine);
            gameSequenceCoroutine = null;
        }

        // Hide all UI
        if (calligraphyUI != null)
        {
            calligraphyUI.Hide();
        }

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
    /// <summary>
    /// Select the appropriate design based on UnlockManager selection
    /// </summary>
    private CalligraphyDesign SelectDesign()
    {
        UnlockManager unlockManager = UnlockManager.Instance;
        if (unlockManager != null)
        {
            CalligraphyDesign selectedDesign = unlockManager.GetCalligraphyDesign();
            if (selectedDesign != null)
            {
                return selectedDesign;
            }
        }

        Debug.LogError("[CalligraphyGame] UnlockManager or design not available!");
        return null;
    }

    private void ValidateReferences()
    {
        // Design is now handled by UnlockManager

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
        // Always show start point and corners when hovering over paper
        if (hitPaper && currentPaper != null)
        {
            currentPaper.ShowStartHighlight(true);
            currentPaper.ShowCornerHighlights(true);
        }
        else if (currentPaper != null)
        {
            // Hide all highlights when cursor is off paper
            currentPaper.ShowStartHighlight(false);
            currentPaper.ShowCornerHighlights(false);
        }

        // Need mouse down to start
        if (!Input.GetMouseButtonDown(0))
            return;

        if (!hitPaper || currentPaper == null)
            return;

        // Check if click is near start point
        Vector3 startPoint2 = currentPaper.GetCurrentStrokeStart();
        float distance = Vector3.Distance(hit.point, startPoint2);

        if (distance <= startRadius)
        {
            // Start drawing!
            playerPath.Clear();
            playerPath.Add(hit.point);
            frameCounter = 0;

            currentPaper.StartDrawing();
            currentState = CalligraphyState.Drawing;
            Debug.Log($"[CalligraphyGame] Started drawing stroke {currentPaper.CurrentStroke != null}! Distance to start: {distance:F3}");
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

            // Sample player path for corner validation
            frameCounter++;
            if (frameCounter >= pathSampleRate)
            {
                playerPath.Add(hit.point);
                frameCounter = 0;
            }

            // Show end highlight only when cursor is near end point
            Vector3 endPoint = currentPaper.GetCurrentStrokeEnd();
            float distanceToEnd = Vector3.Distance(hit.point, endPoint);
            bool nearEnd = distanceToEnd <= endRadius;
            currentPaper.ShowEndHighlight(nearEnd);
        }
        else if (currentPaper != null)
        {
            // Cursor off paper - hide end highlight
            currentPaper.ShowEndHighlight(false);
        }

        // Check for mouse release
        if (Input.GetMouseButtonUp(0))
        {
            if (currentPaper == null)
                return;

            // Add final point
            if (hitPaper)
            {
                playerPath.Add(hit.point);
            }

            // Hide highlights on release
            currentPaper.ShowStartHighlight(false);
            currentPaper.ShowEndHighlight(false);

            // Check if released near end point AND passed through all corners
            if (hitPaper && ValidateStrokePath())
            {
                Vector3 endPoint = currentPaper.GetCurrentStrokeEnd();
                float distance = Vector3.Distance(hit.point, endPoint);

                if (distance <= endRadius)
                {
                    // Success! Complete the stroke
                    currentPaper.CompleteStroke();

                    // Check if all strokes complete
                    if (currentPaper.AllStrokesComplete)
                    {
                        // Fade character to black
                        currentPaper.RevealCharacter();

                        // Change state and finish game
                        currentState = CalligraphyState.TransitioningBackWide;
                        StartCoroutine(PostCompletionSequence());
                    }
                    else
                    {
                        // More strokes remaining - hide current guide and prepare next
                        currentPaper.ShowCornerHighlights(false);
                        currentState = CalligraphyState.WaitingToStart;
                        Debug.Log($"[CalligraphyGame] Stroke complete! Next stroke: {currentPaper.CurrentStroke != null}/{currentPaper.TotalStrokes}");
                    }
                    return;
                }
                else
                {
                    Debug.Log($"[CalligraphyGame] Released too far from end. Distance: {distance:F3}, Required: {endRadius}");
                }
            }
            else
            {
                Debug.Log("[CalligraphyGame] Stroke path invalid - missed corner waypoint(s)");
            }

            // Not near end or invalid path - cancel stroke
            currentPaper.CancelStroke();
            currentState = CalligraphyState.WaitingToStart;
            Debug.Log("[CalligraphyGame] Stroke cancelled - try again");
        }
    }

    /// <summary>
    /// Validate that player path passed through all required corner waypoints.
    /// </summary>
    private bool ValidateStrokePath()
    {
        if (currentPaper == null || currentPaper.CurrentStroke == null)
            return false;

        StrokeData stroke = currentPaper.CurrentStroke;

        // Simple strokes (2 points) don't need corner validation
        if (!stroke.IsCompound)
            return true;

        // Check each corner waypoint
        var corners = stroke.GetCornerPoints();
        foreach (Vector3 corner in corners)
        {
            bool foundCorner = false;

            foreach (Vector3 pathPoint in playerPath)
            {
                if (Vector3.Distance(pathPoint, corner) <= stroke.tolerance)
                {
                    foundCorner = true;
                    break;
                }
            }

            if (!foundCorner)
            {
                Debug.Log($"[CalligraphyGame] Failed validation - missed corner at {corner}");
                return false;
            }
        }

        Debug.Log($"[CalligraphyGame] Path validated - passed through all {corners.Count} corners");
        return true;
    }

    /// <summary>
    /// Main game sequence coroutine - handles full camera flow.
    /// Flow: Spawn → Wait for wide view (MiniGameController) → Pause → Zoom → Player draws
    /// </summary>
    private IEnumerator GameSequence()
    {
        Debug.Log("[CalligraphyGame] Starting game sequence...");

        if (currentPaper == null)
        {
            Debug.LogError("[CalligraphyGame] No paper spawned!");
            yield break;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Step 1: Wait for camera to arrive at wide view (calligraphyPosition)
        // MiniGameController initiates camera movement, we just wait
        // ─────────────────────────────────────────────────────────────────────
        currentState = CalligraphyState.TransitioningToWide;

        if (cameraController != null && cameraController.IsMoving)
        {
            Debug.Log("[CalligraphyGame] Waiting for camera to arrive at wide view...");
            yield return new WaitUntil(() => !cameraController.IsMoving);
            Debug.Log("[CalligraphyGame] Camera arrived at wide view");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Step 2: Pause to show full paper/phrase
        // ─────────────────────────────────────────────────────────────────────
        currentState = CalligraphyState.ShowingFullPaper;
        Debug.Log($"[CalligraphyGame] Showing full paper for {initialPauseTime}s...");
        yield return new WaitForSeconds(initialPauseTime);

        // ─────────────────────────────────────────────────────────────────────
        // Step 3: Transition to zoomed view (from paper prefab)
        // ─────────────────────────────────────────────────────────────────────
        currentState = CalligraphyState.TransitioningToZoom;
        Transform zoomedTarget = currentPaper.GetCameraPositionZoomed();

        if (zoomedTarget != null && cameraController != null)
        {
            cameraController.MoveTo(zoomedTarget);
            Debug.Log("[CalligraphyGame] Camera moving to zoomed view...");
            yield return new WaitUntil(() => !cameraController.IsMoving);
            Debug.Log("[CalligraphyGame] Camera arrived at zoomed view");
        }
        else
        {
            Debug.LogWarning("[CalligraphyGame] Missing zoomed camera position or controller");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Step 4: Ready for player input
        // ─────────────────────────────────────────────────────────────────────
        currentState = CalligraphyState.WaitingToStart;
        Debug.Log("[CalligraphyGame] Ready for player input!");

        // Player draws via Update() → HandleWaitingState/HandleDrawingState
        // PostCompletionSequence() is triggered when stroke completes
    }

    /// <summary>
    /// Post-completion sequence - zoom back to wide view and fire completion event.
    /// </summary>
    private IEnumerator PostCompletionSequence()
    {
        Debug.Log("[CalligraphyGame] Stroke completed! Starting post-completion sequence...");

        // ─────────────────────────────────────────────────────────────────────
        // Step 1: Wait for character fade animation to complete
        // ─────────────────────────────────────────────────────────────────────
        if (currentPaper != null && currentPaper.IsFading)
        {
            Debug.Log("[CalligraphyGame] Waiting for character fade animation...");
            yield return new WaitUntil(() => !currentPaper.IsFading);
            Debug.Log("[CalligraphyGame] Character fade animation complete");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Step 2: Transition back to wide view (calligraphyPosition on CameraController)
        // (State is already TransitioningBackWide from stroke completion - this is a safeguard)
        // ─────────────────────────────────────────────────────────────────────
        currentState = CalligraphyState.TransitioningBackWide;

        if (cameraController != null && cameraController.calligraphyPosition != null)
        {
            cameraController.MoveTo(cameraController.calligraphyPosition);
            Debug.Log("[CalligraphyGame] Camera returning to wide view...");
            yield return new WaitUntil(() => !cameraController.IsMoving);
            Debug.Log("[CalligraphyGame] Camera arrived at wide view");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Step 3: Show success UI (MiniGameController handles wait timing)
        // ─────────────────────────────────────────────────────────────────────
        currentState = CalligraphyState.Complete;

        if (calligraphyUI != null)
        {
            yield return calligraphyUI.ShowSuccessAsync();
            Debug.Log("[CalligraphyGame] Success UI shown - MiniGameController handles wait timing");
        }
        else
        {
            Debug.LogWarning("[CalligraphyGame] CalligraphyUI not assigned - skipping success display");
        }

        // ─────────────────────────────────────────────────────────────────────
        // Step 3: Fire completion event (MiniGameController waits, then handles room return)
        // ─────────────────────────────────────────────────────────────────────
        float completionTime = Time.time - startTime;

        // Debug: verify scrollPrefab exists
        Debug.Log($"[CalligraphyGame] Creating result. design: {design != null}, scrollPrefab: {design?.scrollPrefab?.name ?? "NULL"}");

        // Create result - use explicit assignment to ensure field is set
        CalligraphyResult result = new CalligraphyResult();
        result.design = design;
        result.roomItemPrefab = design.scrollPrefab;
        result.CompletionTime = completionTime;

        // Debug: verify result
        Debug.Log($"[CalligraphyGame] Result created. roomItemPrefab: {result.roomItemPrefab?.name ?? "NULL"}, ItemInstance: {result.ItemInstance?.name ?? "NULL"}");
        Debug.Log($"[CalligraphyGame] Game complete! Total time: {completionTime:F2}s");

        OnGameCompleted?.Invoke(result);
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
