using System;
using UnityEngine;

/// <summary>
/// Game logic controller for the Lantern mini-game.
/// Tracks brightness, harmony zone status, and win condition.
/// </summary>
public class LanternGame : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Lower bound of harmony zone (0-1)")]
    [Range(0f, 1f)] public float harmonyZoneMin = 0.4f;

    [Tooltip("Upper bound of harmony zone (0-1)")]
    [Range(0f, 1f)] public float harmonyZoneMax = 0.6f;

    [Tooltip("Time player must stay in harmony to win (seconds)")]
    public float goalTime = 10f;

    [Tooltip("How fast brightness changes per second")]
    public float brightnessChangeSpeed = 0.3f;

    [Tooltip("Input key to increase brightness")]
    public KeyCode increaseKey = KeyCode.Space;

    [Header("References")]
    [Tooltip("Reference to the UI controller")]
    public LanternUI ui;

    [Tooltip("Lantern prefab to spawn")]
    public GameObject lanternPrefab;

    [Tooltip("Where to spawn the lantern")]
    public Transform spawnPoint;

    // State
    private float brightness = 0.5f;
    private float timeInHarmony = 0f;
    private bool isPlaying = false;

    // Spawned lantern references
    private GameObject spawnedLantern;
    private LanternVisual lanternVisual;

    // Events
    public event Action OnGameStarted;
    public event Action<LanternResult> OnGameCompleted;

    /// <summary>
    /// Start the mini-game: spawn lantern, show UI, reset state
    /// </summary>
    [ContextMenu("Start Game")]
    public void StartGame()
    {
        isPlaying = true;
        brightness = 0.5f;
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
    /// Stop the game and clean up
    /// </summary>
    public void StopGame()
    {
        isPlaying = false;

        if (ui != null)
        {
            ui.Hide();
        }

        CleanupLantern();

        Debug.Log("[LanternGame] Game stopped");
    }

    /// <summary>
    /// Spawn the lantern prefab and get its visual component
    /// </summary>
    void SpawnLantern()
    {
        if (lanternPrefab == null)
        {
            Debug.LogError("[LanternGame] Lantern prefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        spawnedLantern = Instantiate(lanternPrefab, spawnPosition, spawnRotation);
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

        // Create result data
        LanternResult result = new LanternResult
        {
            lanternInstance = spawnedLantern,
            finalBrightness = brightness,
            completionTime = Time.time
        };

        OnGameCompleted?.Invoke(result);
    }

    /// <summary>
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
public class LanternResult
{
    public GameObject lanternInstance;
    public float finalBrightness;
    public float completionTime;

    // Future customization data can go here
    // public Color paperColor;
    // public int patternID;
}
