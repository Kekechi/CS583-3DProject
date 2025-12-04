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
    [SerializeField] private CalligraphyDesign design;
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

    [Header("Timing")]
    [Tooltip("Time to show full paper before zooming to stroke")]
    [SerializeField] private float initialPauseTime = 1.0f;

    // ─────────────────────────────────────────────────────────────────────────
    // Runtime State
    // ─────────────────────────────────────────────────────────────────────────
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
        // Check hover for start highlight (always, not just on click)
        if (hitPaper && currentPaper != null)
        {
            Vector3 startPoint = currentPaper.GetCurrentStrokeStart();
            float hoverDistance = Vector3.Distance(hit.point, startPoint);
            bool nearStart = hoverDistance <= startRadius;
            currentPaper.ShowStartHighlight(nearStart);
        }
        else if (currentPaper != null)
        {
            currentPaper.ShowStartHighlight(false);
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

            // Hide start highlight
            currentPaper.ShowStartHighlight(false);
            // Hide end highlight
            currentPaper.ShowEndHighlight(false);

            // Check if released near end point
            if (hitPaper)
            {
                Vector3 endPoint = currentPaper.GetCurrentStrokeEnd();
                float distance = Vector3.Distance(hit.point, endPoint);

                if (distance <= endRadius)
                {
                    // Success! Complete the stroke
                    currentPaper.CompleteStroke();
                    currentPaper.RevealCharacter();

                    // Start post-completion sequence (zoom back to wide)
                    StartCoroutine(PostCompletionSequence());
                    return;
                }
                else
                {
                    Debug.Log($"[CalligraphyGame] Released too far from end. Distance: {distance:F3}, Required: {endRadius}");
                }
            }

            // Not near end - cancel stroke
            currentPaper.CancelStroke();
            currentState = CalligraphyState.WaitingToStart;
            Debug.Log("[CalligraphyGame] Stroke cancelled - try again");
        }
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
        // Step 1: Transition back to wide view (calligraphyPosition on CameraController)
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
        // Step 2: Show success UI (MiniGameController handles wait timing)
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
