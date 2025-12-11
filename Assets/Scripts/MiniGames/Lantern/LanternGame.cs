using System;
using UnityEngine;

/// <summary>
/// Game logic controller for the Lantern mini-game.
/// Tracks brightness, harmony zone status, and win condition.
/// </summary>
public class LanternGame : MonoBehaviour, IMiniGame
{
    [Header("Game Settings")]
    [Tooltip("How fast brightness changes per second")]
    public float brightnessChangeSpeed = 0.3f;

    [Tooltip("Input key to increase brightness")]
    public KeyCode increaseKey = KeyCode.Space;

    [Header("References")]
    [Tooltip("Reference to the UI controller")]
    public LanternUI ui;

    [Tooltip("Lantern prefab to spawn during the game (visual only)")]
    public GameObject lanternPrefab;

    [Tooltip("Where to spawn the lantern")]
    public Transform spawnPoint;

    // State
    private LanternDesign design; // Runtime-selected design
    private float harmonyZoneMin; // Set from design
    private float harmonyZoneMax; // Set from design
    private float goalTime; // Set from design
    private float brightness = 0.5f;
    private float timeInHarmony = 0f;
    private bool isPlaying = false;

    // Spawned lantern references
    private GameObject spawnedLantern;
    private LanternVisual lanternVisual;

    // Events
    public event Action OnGameStarted;
    public event Action<LanternResult> OnGameCompleted;

    // IMiniGame implementation
    public MiniGameType GameType => MiniGameType.Lantern;
    public string GameName => "Lantern";
    public string GameDescription => "Balance the light";



    /// <summary>
    /// Start the mini-game: spawn lantern, show UI, reset state
    /// </summary>
    [ContextMenu("Start Game")]
    public void StartGame()
    {
        // Select design based on unlock status
        design = SelectDesign();

        // Load design settings
        if (design != null)
        {
            goalTime = design.goalTime;
            harmonyZoneMin = design.brightnessRange.x;
            harmonyZoneMax = design.brightnessRange.y;
        }

        isPlaying = true;
        brightness = 0f; // Start at minimum (player builds up from zero)
        timeInHarmony = 0f;

        SpawnLantern();

        if (ui != null)
        {
            ui.Show();
            ui.SetupHarmonyZone(harmonyZoneMin, harmonyZoneMax);
        }

        OnGameStarted?.Invoke();

        Debug.Log("[LanternGame] Game started");
    }

    /// <summary>
    /// Stop the game and hide all UI elements
    /// Called by MiniGameController during cleanup
    /// </summary>
    public void StopGame()
    {
        isPlaying = false;

        // Hide all lantern-specific UI
        if (ui != null)
        {
            ui.Hide();
        }

        // Clean up spawned lantern
        CleanupLantern();

        // Deactivate this game object
        gameObject.SetActive(false);

        Debug.Log("[LanternGame] Game stopped and UI hidden");
    }

    void Update()
    {
        if (!isPlaying) return;

        // Update brightness based on input
        if (Input.GetKey(increaseKey))
        {
            brightness += brightnessChangeSpeed * Time.deltaTime;
        }
        else
        {
            brightness -= brightnessChangeSpeed * Time.deltaTime;
        }

        brightness = Mathf.Clamp01(brightness);

        // Check if in harmony zone
        bool inHarmony = IsInHarmonyZone();

        // Track time in harmony (only counts when in zone)
        if (inHarmony)
        {
            timeInHarmony += Time.deltaTime;
        }

        // Update visuals
        if (ui != null)
        {
            ui.UpdateDisplay(brightness, inHarmony, timeInHarmony, goalTime);
        }

        if (lanternVisual != null)
        {
            lanternVisual.SetBrightness(brightness);
        }

        // Check win condition
        if (timeInHarmony >= goalTime)
        {
            CompleteGame();
        }
    }

    /// <summary>
    /// Check if current brightness is in the harmony zone
    /// </summary>
    public bool IsInHarmonyZone()
    {
        return brightness >= harmonyZoneMin && brightness <= harmonyZoneMax;
    }
    /// <summary>
    /// Spawn the lantern prefab and get its visual component
    /// </summary>
    void SpawnLantern()
    {

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        spawnedLantern = Instantiate(design.roomItemPrefab, spawnPosition, spawnRotation);
        lanternVisual = spawnedLantern.GetComponent<LanternVisual>();

        if (lanternVisual == null)
        {
            Debug.LogError("[LanternGame] Spawned lantern prefab does not have LanternVisual component!");
        }

        Debug.Log($"[LanternGame] Lantern spawned at {spawnPosition}");
    }

    /// <summary>
    /// Complete the game: create result, fire event, show success
    /// </summary>
    void CompleteGame()
    {
        isPlaying = false;

        Debug.Log($"[LanternGame] Game completed! Time: {timeInHarmony:F2}s, Final brightness: {brightness:F2}");

        if (ui != null)
        {
            ui.ShowSuccess();
        }

        // Create result data with the room prefab (not the visual instance)
        LanternResult result = new LanternResult
        {
            roomItemPrefab = design != null ? design.roomItemPrefab : null,  // Prefab from design
            finalBrightness = brightness,         // Customization data
            CompletionTime = Time.time,
            adjustmentsMade = 0 // TODO: Track this if needed
        };

        OnGameCompleted?.Invoke(result);
    }

    /// <summary>
    /// Select the appropriate design based on UnlockManager selection
    /// </summary>
    private LanternDesign SelectDesign()
    {
        UnlockManager unlockManager = UnlockManager.Instance;
        if (unlockManager != null)
        {
            LanternDesign selectedDesign = unlockManager.GetLanternDesign();
            if (selectedDesign != null)
            {
                return selectedDesign;
            }
        }

        Debug.LogError("[LanternGame] UnlockManager or design not available!");
        return null;
    }    /// <summary>
         /// Destroy the spawned lantern instance
         /// </summary>
    void CleanupLantern()
    {
        if (spawnedLantern != null)
        {
            Destroy(spawnedLantern);
            spawnedLantern = null;
            lanternVisual = null;
            Debug.Log("[LanternGame] Lantern cleaned up");
        }
    }
}

/// <summary>
/// Data container for completed lantern mini-game result
/// </summary>
[Serializable]
public class LanternResult : MiniGameResult
{
    public override GameObject ItemInstance => roomItemPrefab;
    public override MiniGameType GameType => MiniGameType.Lantern;

    // Lantern-specific data
    public GameObject roomItemPrefab;  // The prefab to instantiate in the room
    public float finalBrightness;      // Player's final brightness value
    public int adjustmentsMade;        // Number of adjustments made

    // CompletionTime inherited from base class
}
